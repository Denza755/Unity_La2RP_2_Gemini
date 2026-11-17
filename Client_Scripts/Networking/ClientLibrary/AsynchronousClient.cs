using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

public class AsynchronousClient
{
    private Socket _socket;
    private string _ipAddress;
    private int _port;
    private volatile bool _connected;
    private int _disconnected;
    private ClientPacketHandler _clientPacketHandler;
    private ServerPacketHandler _serverPacketHandler;
    private DefaultClient _client;
    private bool _cryptEnabled = false;
    private bool _initPacket = true;
    private bool _initPacketEnabled;
    private readonly ConcurrentQueue<byte[]> _packetQueue = new ConcurrentQueue<byte[]>();
    private readonly SemaphoreSlim _packetSemaphore = new SemaphoreSlim(0);
    private CancellationTokenSource _processingCancellation;
    private Task _processingTask;

    public bool InitPacket { get { return _initPacket; } set { _initPacket = value; } }
    public bool CryptEnabled
    {
        get { return _cryptEnabled; }
        set
        {
            Debug.Log("Crypt" + (value ? " enabled." : " disabled."));
            _cryptEnabled = value;
        }
    }

    public AsynchronousClient(string ip, int port, DefaultClient client, ClientPacketHandler clientPacketHandler,
        ServerPacketHandler serverPacketHandler, bool enableInitPacket, bool enableChecksum)
    {
        _ipAddress = ip;
        _port = port;
        _clientPacketHandler = clientPacketHandler;
        _serverPacketHandler = serverPacketHandler;
        _clientPacketHandler.SetClient(this);
        _serverPacketHandler.SetClient(this, _clientPacketHandler, enableChecksum);
        _client = client;
        _initPacketEnabled = enableInitPacket;
        _initPacket = enableInitPacket;
    }

    public bool Connect()
    {
        IPAddress ipAddress;
        if (!IPAddress.TryParse(_ipAddress, out ipAddress))
        {
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(_ipAddress);
                ipAddress = ipHostInfo.AddressList[0];
            }
            catch (Exception ex)
            {
                Debug.LogError($"DNS resolution failed for '{_ipAddress}': {ex.Message}");
                EventProcessor.Instance.QueueEvent(() => _client.OnConnectionFailed());
                return false;
            }
        }

        try
        {
            _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (_initPacketEnabled)
            {
                _initPacket = true;
            }

            Debug.Log("Connecting...");

            IAsyncResult result = _socket.BeginConnect(ipAddress, _port, null, null);

            bool success = result.AsyncWaitHandle.WaitOne(5000, true);

            if (success && _socket.Connected)
            {
                Debug.Log("Connection success.");
                _socket.EndConnect(result);
                Interlocked.Exchange(ref _disconnected, 0);
                _connected = true;

                Task.Run(StartReceiving);
                return true;
            }
            else
            {
                Debug.Log("Connection failed.");
                EventProcessor.Instance.QueueEvent(() => _client.OnConnectionFailed());
                _socket.Close();
                _socket = null;
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connect socket exception: {ex.Message}");
            EventProcessor.Instance.QueueEvent(() => _client.OnConnectionFailed());
            if (_socket != null)
            {
                try { _socket.Close(); } catch { }
                _socket = null;
            }
            return false;
        }
    }

    public void Disconnect()
    {
        if ((!_connected && _socket == null) || Interlocked.Exchange(ref _disconnected, 1) != 0)
        {
            return;
        }

        // Cancel packet processing
        _processingCancellation?.Cancel();

        ClientCleanup();

        Debug.Log("Disconnect");

        EventProcessor.Instance.QueueEvent(() => _client.OnDisconnect());
    }

    public bool IsConnected()
    {
        if (!_connected)
        {
            return false;
        }

        Socket socket = _socket;
        if (socket == null)
        {
            return false;
        }

        try
        {
            // Avoid SelectMode.SelectRead + Available == 0: StartReceiving() reads on a background
            // thread and can drain incoming bytes between Poll() and Available, causing false disconnects.
            return socket.Connected && !socket.Poll(0, SelectMode.SelectError);
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    private void ClientCleanup()
    {
        try
        {
            _connected = false;
            _serverPacketHandler?.CancelTokens();
            if (_socket != null)
            {
                try
                {
                    if (_socket.Connected)
                    {
                        _socket.Shutdown(SocketShutdown.Both);
                    }
                }
                catch { }
                _socket.Close();
                _socket.Dispose();
                _socket = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void SendPacket(ClientPacket packet)
    {
        if (_socket == null || !_connected || !_socket.Connected)
        {
            return;
        }

        try
        {
            byte[] payload = packet.GetData();
            if (payload == null) return;

            int len = payload.Length + 2;
            byte[] buffer = new byte[len];
            buffer[0] = (byte)(len & 0xff);
            buffer[1] = (byte)((len >> 8) & 0xff);
            Buffer.BlockCopy(payload, 0, buffer, 2, payload.Length);

            int totalSent = 0;
            while (totalSent < len)
            {
                int sent = _socket.Send(buffer, totalSent, len - totalSent, SocketFlags.None);
                if (sent <= 0)
                {
                    Disconnect();
                    break;
                }
                totalSent += sent;
            }
        }
        catch (SocketException ex)
        {
            Debug.LogWarning($"SendPacket socket error: {ex.Message}");
            Disconnect();
        }
        catch (ObjectDisposedException)
        {
            Disconnect();
        }
        catch (Exception e)
        {
            Debug.LogError($"SendPacket error: {e}");
        }
    }

    public void StartReceiving()
    {
        Debug.Log("Start receiving");

        // Start the packet processing task
        _processingCancellation = new CancellationTokenSource();
        _processingTask = Task.Run(() => ProcessPacketsAsync(_processingCancellation.Token));

        try
        {
            using (NetworkStream stream = new NetworkStream(_socket, ownsSocket: false))
            {
                int lengthHi;
                int lengthLo;
                int length;

                for (; ; )
                {
                    if (!_connected)
                    {
                        Debug.LogWarning("Disconnected.");
                        break;
                    }

                    lengthLo = stream.ReadByte();
                    lengthHi = stream.ReadByte();

                    if (lengthLo == -1 || lengthHi == -1 || !_connected)
                    {
                        Debug.Log("Server terminated the connection.");
                        Disconnect();
                        break;
                    }

                    length = (lengthHi * 256) + lengthLo;
                    length = length - 2;
                    if (length <= 0)
                    {
                        continue;
                    }

                    byte[] data = new byte[length];

                    int receivedBytes = 0;
                    while (receivedBytes < length)
                    {
                        int read = stream.Read(data, receivedBytes, length - receivedBytes);
                        if (read <= 0)
                        {
                            Debug.Log("Server terminated the connection during packet read.");
                            Disconnect();
                            return;
                        }
                        receivedBytes += read;
                    }

                    // Queue the packet for processing instead of processing immediately
                    if (_serverPacketHandler.HandlePacketCrypto(data, _initPacket))
                    {
                        _packetQueue.Enqueue(data);
                        _packetSemaphore.Release(); // Signal that a packet is available
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (_connected)
            {
                Debug.LogWarning($"StartReceiving exception: {ex.Message}");
                Disconnect();
            }
        }
        finally
        {
            // Clean shutdown of processing task
            _processingCancellation?.Cancel();
            try
            {
                _processingTask?.Wait(TimeSpan.FromSeconds(2));
            }
            catch { }
        }
    }

    private async Task ProcessPacketsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for a packet to be available
                await _packetSemaphore.WaitAsync(cancellationToken);

                // Process all available packets in order
                while (_packetQueue.TryDequeue(out byte[] data))
                {
                    try
                    {
                        await _serverPacketHandler.HandlePacketAsync(data);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error processing packet: {ex}");
                        // Continue processing other packets even if one fails
                    }

                    // Check cancellation between packets
                    if (cancellationToken.IsCancellationRequested)
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Packet processing cancelled");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Packet processing task error: {ex.Message}");
        }
    }

    public int GetQueuedPacketCount()
    {
        return _packetQueue.Count;
    }
}
