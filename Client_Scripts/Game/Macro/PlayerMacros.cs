using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerMacros : MonoBehaviour
{
    private static PlayerMacros _instance;
    public static PlayerMacros Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("PlayerMacros");
                _instance = go.AddComponent<PlayerMacros>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private readonly Dictionary<int, Macro> _macros = new Dictionary<int, Macro>();
    public event Action OnMacrosChanged;

    private int _lastRevision = -1;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void OnReceiveMacroList(SendMacroListPacket packet)
    {
        if (packet == null) return;

        // If revision changed and count is 0 or it's a fresh update, handle clear
        if (packet.Count == 0)
        {
            _macros.Clear();
            _lastRevision = packet.Revision;
            OnMacrosChanged?.Invoke();
            return;
        }

        if (packet.Revision != _lastRevision && packet.Count > 0 && _macros.Count >= packet.Count)
        {
            // Reset for full resync if revision changed
            _macros.Clear();
        }
        _lastRevision = packet.Revision;

        if (packet.HasMacro && packet.MacroData != null)
        {
            _macros[packet.MacroData.Id] = packet.MacroData;
        }

        OnMacrosChanged?.Invoke();
    }

    public Macro GetMacro(int id)
    {
        _macros.TryGetValue(id, out Macro macro);
        return macro;
    }

    public List<Macro> GetAllMacros()
    {
        return _macros.Values.OrderBy(m => m.Id).ToList();
    }

    public void RequestMakeMacro(Macro macro)
    {
        if (macro == null) return;
        GameClient.Instance?.ClientPacketHandler?.RequestMakeMacro(macro);
    }

    public void RequestDeleteMacro(int macroId)
    {
        GameClient.Instance?.ClientPacketHandler?.RequestDeleteMacro(macroId);
        _macros.Remove(macroId);
        OnMacrosChanged?.Invoke();
    }
}
