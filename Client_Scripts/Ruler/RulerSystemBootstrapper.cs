using UnityEngine;

public static class RulerSystemBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        EnsureRulerManager();
        EnsureRulerCameraController();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        EnsureRulerSystemsInScene();
    }

    public static void EnsureRulerManager()
    {
        if (RulerManager.Instance == null)
        {
            GameObject rulerGo = new GameObject("RulerManager");
            rulerGo.AddComponent<RulerManager>();
            Object.DontDestroyOnLoad(rulerGo);
        }
    }

    public static void EnsureRulerCameraController()
    {
        if (RulerCameraController.Instance == null)
        {
            GameObject camGo = new GameObject("RulerCameraController");
            camGo.AddComponent<RulerCameraController>();
            Object.DontDestroyOnLoad(camGo);
        }
    }

    public static void EnsureRulerSystemsInScene()
    {
        EnsureRulerManager();
        EnsureRulerCameraController();

        if (RulerGridOverlay.Instance == null)
        {
            GameObject gridGo = new GameObject("RulerGridOverlay");
            gridGo.AddComponent<RulerGridOverlay>();
        }

        if (BuildingPlacementSystem.Instance == null)
        {
            GameObject bpGo = new GameObject("BuildingPlacementSystem");
            bpGo.AddComponent<BuildingPlacementSystem>();
        }

        if (L2GameUI.Instance != null && RulerHUDWindow.Instance == null)
        {
            L2GameUI.Instance.gameObject.AddComponent<RulerHUDWindow>();
        }
    }
}
