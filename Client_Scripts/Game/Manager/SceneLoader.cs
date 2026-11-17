using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string _menuScene = "Menu";
    [SerializeField] private string _lobbyScene = "l2_lobby";
    [SerializeField] private string _gameScene = "Game";
    [SerializeField] private List<SceneListObject> _mapList = new List<SceneListObject>();
    [SerializeField] private List<string> _mapsToLoad = new List<string>();

    private readonly HashSet<string> _loadedTiles = new HashSet<string>();
    private bool _worldStreaming = false;
    private bool _streamingBusy = false;

    public string GameScene => _gameScene;
    public HashSet<string> LoadedTiles => _loadedTiles;

    public static SceneLoader _instance;
    public static SceneLoader Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(this);
            return;
        }

        FillMapsToLoadList();
    }

    private void FillMapsToLoadList()
    {
        _mapsToLoad = new List<string>();
        for (int i = 0; i < _mapList.Count; i++)
        {
            var map = _mapList[i];
            if (map.enabled && !string.IsNullOrEmpty(map.name))
            {
                _mapsToLoad.Add(map.name);
            }
        }

        // Default Talking Island sectors fallback
        if (_mapsToLoad.Count == 0)
        {
            _mapsToLoad.AddRange(new string[] { "17_25", "17_24", "16_24", "16_25" });
        }
    }

    public void LoadMenu()
    {
        _worldStreaming = false;
        _streamingBusy = false;
        _loadedTiles.Clear();

        SwitchScene(_menuScene, (AsyncOperation o) =>
        {
            GameManager.Instance.NotifyEvent(GameEvent.LOADING_STARTED);

            StartCoroutine(LoadLobbyRoutine());
        });
    }

    private IEnumerator LoadLobbyRoutine()
    {
        yield return LoadSceneAdditiveIfNeeded(_lobbyScene);
        GameManager.Instance.NotifyEvent(GameEvent.WORLD_LOADED);
    }

    public void LoadGame()
    {
        _worldStreaming = false;
        _streamingBusy = false;
        _loadedTiles.Clear();

        SwitchScene(_gameScene, (AsyncOperation o) =>
        {
            GameManager.Instance.NotifyEvent(GameEvent.LOADING_STARTED);

            Scene game = SceneManager.GetSceneByName(_gameScene);
            if (game.IsValid() && game.isLoaded)
            {
                SceneManager.SetActiveScene(game);
            }

            Vector3 startPos = Vector3.zero;
            if (GameClient.Instance != null && GameClient.Instance.PlayerInfo.Identity != null)
            {
                startPos = GameClient.Instance.PlayerInfo.Identity.Position;
            }

            List<string> initialSectors = ResolveInitialSectors(startPos);
            StartCoroutine(LoadInitialSectorsRoutine(initialSectors));
        });
    }

    private List<string> ResolveInitialSectors(Vector3 startPos)
    {
        List<string> result = new List<string>();

        if (startPos != Vector3.zero)
        {
            string primary = VectorUtils.GetSwMapNameFromUnityPos(startPos);
            result = GetSurroundingSectors(startPos, radius: 1);

            if (Application.CanStreamedLevelBeLoaded(primary))
            {
                if (!result.Contains(primary)) result.Insert(0, primary);
                else
                {
                    result.Remove(primary);
                    result.Insert(0, primary);
                }
            }
        }

        // If list is empty or primary not in build, fallback to preconfigured maps
        if (result.Count == 0)
        {
            for (int i = 0; i < _mapsToLoad.Count; i++)
            {
                if (Application.CanStreamedLevelBeLoaded(_mapsToLoad[i]) && !result.Contains(_mapsToLoad[i]))
                {
                    result.Add(_mapsToLoad[i]);
                }
            }
        }

        // Guarantee all available Talking Island sectors are included for seamless exploration
        string[] talkingIsland = new string[] { "17_25", "17_24", "16_24", "16_25" };
        for (int i = 0; i < talkingIsland.Length; i++)
        {
            if (Application.CanStreamedLevelBeLoaded(talkingIsland[i]) && !result.Contains(talkingIsland[i]))
            {
                result.Add(talkingIsland[i]);
            }
        }

        return result;
    }

    private IEnumerator LoadInitialSectorsRoutine(List<string> sectors)
    {
        Debug.Log($"[SceneLoader] Loading {sectors.Count} initial sector(s): {string.Join(", ", sectors)}");

        for (int i = 0; i < sectors.Count; i++)
        {
            string sector = sectors[i];
            yield return LoadSceneAdditiveIfNeeded(sector);
            _loadedTiles.Add(sector);
            SanitizeLoadedScene(SceneManager.GetSceneByName(sector));
        }

        StartCoroutine(StartGame());
    }

    public void EnsureSectorsLoaded(Vector3 unityPosition, Action onComplete)
    {
        StartCoroutine(EnsureSectorsLoadedRoutine(unityPosition, onComplete));
    }

    private IEnumerator EnsureSectorsLoadedRoutine(Vector3 unityPosition, Action onComplete)
    {
        string primary = VectorUtils.GetSwMapNameFromUnityPos(unityPosition);
        List<string> wanted = GetSurroundingSectors(unityPosition, radius: 1);

        if (Application.CanStreamedLevelBeLoaded(primary))
        {
            if (!wanted.Contains(primary)) wanted.Insert(0, primary);
            else
            {
                wanted.Remove(primary);
                wanted.Insert(0, primary);
            }
        }

        for (int i = 0; i < wanted.Count; i++)
        {
            string tile = wanted[i];
            if (!_loadedTiles.Contains(tile) || !SceneManager.GetSceneByName(tile).isLoaded)
            {
                yield return LoadSceneAdditiveIfNeeded(tile);
                _loadedTiles.Add(tile);
                SanitizeLoadedScene(SceneManager.GetSceneByName(tile));
            }
        }

        onComplete?.Invoke();
    }

    private void Update()
    {
        if (!_worldStreaming || _streamingBusy || PlayerEntity.Instance == null)
        {
            return;
        }

        Vector3 playerPos = PlayerEntity.Instance.transform.position;
        List<string> wanted = GetSurroundingSectors(playerPos, radius: 1);
        if (wanted.Count == 0) return;

        // 1. Stream in missing wanted sectors (one by one)
        for (int i = 0; i < wanted.Count; i++)
        {
            string tile = wanted[i];
            if (!_loadedTiles.Contains(tile) && Application.CanStreamedLevelBeLoaded(tile))
            {
                StartCoroutine(StreamInTileRoutine(tile));
                return;
            }
        }

        // 2. Stream out far away sectors (beyond radius 2)
        List<string> broadArea = GetSurroundingSectors(playerPos, radius: 2);
        foreach (string loaded in _loadedTiles)
        {
            // Do not unload if still in broad 5x5 area
            if (broadArea.Contains(loaded))
            {
                continue;
            }

            StartCoroutine(StreamOutTileRoutine(loaded));
            return;
        }
    }

    private IEnumerator StreamInTileRoutine(string tile)
    {
        _streamingBusy = true;
        Debug.Log($"[SceneLoader] Streaming in sector '{tile}'");

        yield return LoadSceneAdditiveIfNeeded(tile);
        _loadedTiles.Add(tile);
        SanitizeLoadedScene(SceneManager.GetSceneByName(tile));

        _streamingBusy = false;
    }

    private IEnumerator StreamOutTileRoutine(string tile)
    {
        _streamingBusy = true;
        Debug.Log($"[SceneLoader] Streaming out sector '{tile}'");

        UnloadScene(tile);
        _loadedTiles.Remove(tile);

        _streamingBusy = false;
        yield break;
    }

    public static List<string> GetSurroundingSectors(Vector3 unityPos, int radius = 1)
    {
        Vector3 l2Pos = VectorUtils.ConvertPosUnityToL2j(unityPos);
        int tileX, tileY;
        VectorUtils.GetSwMapTile(l2Pos, out tileX, out tileY);
        return GetSurroundingSectors(tileX, tileY, radius);
    }

    public static List<string> GetSurroundingSectors(int centerTileX, int centerTileY, int radius = 1)
    {
        List<string> sectors = new List<string>();
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                string name = VectorUtils.FormatSwMapName(centerTileX + dx, centerTileY + dy);
                if (Application.CanStreamedLevelBeLoaded(name) && !sectors.Contains(name))
                {
                    sectors.Add(name);
                }
            }
        }
        return sectors;
    }

    private IEnumerator LoadSceneAdditiveIfNeeded(string sceneName)
    {
        Scene existing = SceneManager.GetSceneByName(sceneName);
        if (existing.IsValid() && existing.isLoaded)
        {
            yield break;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning($"[SceneLoader] Scene '{sceneName}' is not in Build Settings!");
            yield break;
        }

        Debug.Log($"[SceneLoader] Loading scene additively: {sceneName}");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (asyncLoad == null)
        {
            Debug.LogError($"[SceneLoader] Failed to start loading scene: {sceneName}");
            yield break;
        }

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void SwitchScene(string sceneName, Action<AsyncOperation> p)
    {
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            Debug.Log("[SceneLoader] Switching to scene " + sceneName);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.completed += p;
        }
        else
        {
            Debug.Log("[SceneLoader] Skipping scene switch " + sceneName);
            AsyncOperation dummyOperation = new AsyncOperation();
            p.Invoke(dummyOperation);
        }
    }

    private void UnloadScene(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).IsValid())
        {
            return;
        }

        SceneManager.UnloadSceneAsync(sceneName);
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.2f);

        Debug.Log("[SceneLoader] All initial sectors loaded, notifying LOADING_COMPLETE.");

        if (World.Instance != null && !World.Instance.OfflineMode)
        {
            GameManager.Instance.NotifyEvent(GameEvent.LOADING_COMPLETE);
        }

        _worldStreaming = true;
    }

    public static void SanitizeLoadedScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded) return;
        if (scene.name == "Game" || scene.name == "Menu") return;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] == null) continue;

            // 1. AudioListeners
            var listeners = roots[i].GetComponentsInChildren<AudioListener>(true);
            for (int j = 0; j < listeners.Length; j++)
            {
                if (listeners[j].enabled)
                {
                    Debug.Log($"[SanitizeScene] Disabling duplicate AudioListener in {scene.name}: {listeners[j].gameObject.name}");
                    listeners[j].enabled = false;
                }
            }

            // 2. Cameras
            var cameras = roots[i].GetComponentsInChildren<Camera>(true);
            for (int j = 0; j < cameras.Length; j++)
            {
                if (cameras[j].enabled)
                {
                    Debug.Log($"[SanitizeScene] Disabling duplicate Camera in {scene.name}: {cameras[j].gameObject.name}");
                    cameras[j].enabled = false;
                }
            }

            // 3. EventSystems
            var eventSystems = roots[i].GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true);
            for (int j = 0; j < eventSystems.Length; j++)
            {
                if (eventSystems[j].enabled)
                {
                    Debug.Log($"[SanitizeScene] Disabling duplicate EventSystem in {scene.name}: {eventSystems[j].gameObject.name}");
                    eventSystems[j].enabled = false;
                }
            }

            // 4. Directional Lights (to prevent 4x overexposure/whiteout)
            var lights = roots[i].GetComponentsInChildren<Light>(true);
            for (int j = 0; j < lights.Length; j++)
            {
                if (lights[j].type == LightType.Directional && lights[j].enabled)
                {
                    Debug.Log($"[SanitizeScene] Disabling duplicate Directional Light in {scene.name}: {lights[j].gameObject.name}");
                    lights[j].enabled = false;
                }
            }
        }
    }
}
