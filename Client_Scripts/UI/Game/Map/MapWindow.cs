using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class MapWindow : L2PopupWindow
{
    private static MapWindow _instance;
    public static MapWindow Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<MapWindow>();
                if (_instance == null && L2GameUI.Instance != null)
                {
                    _instance = L2GameUI.Instance.gameObject.AddComponent<MapWindow>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            _isWindowHidden = true;
            if (_windowTemplate == null)
            {
                LoadAssets();
            }
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected override void LoadAssets()
    {
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/MapWindow/MapWindow");
    }

    protected override void InitWindow(VisualElement root)
    {
        base.InitWindow(root);

        var dragArea = GetElementByClass("drag-area");
        if (dragArea != null)
        {
            DragManipulator drag = new DragManipulator(dragArea, _windowEle, this);
            dragArea.AddManipulator(drag);
        }

        RegisterCloseWindowEvent("btn-close-frame");
        RegisterClickWindowEvent(_windowEle, dragArea);
    }

    protected override IEnumerator BuildWindow(VisualElement root)
    {
        InitWindow(root);

        yield return new WaitForEndOfFrame();

        _windowEle.style.left = new Length(50, LengthUnit.Percent);
        _windowEle.style.top = new Length(50, LengthUnit.Percent);
        _windowEle.style.translate = new StyleTranslate(new Translate(new Length(-50, LengthUnit.Percent), new Length(-50, LengthUnit.Percent)));

        Label windowName = (Label)GetElementById("windows-name-label");
        if (windowName != null)
        {
            windowName.text = "World Map";
        }

        Image mapImg = _windowEle.Q<Image>("WorldMapImage");
        if (mapImg != null)
        {
            Texture2D mapTex = Resources.Load<Texture2D>("Data/UI/Map/world_map");
            if (mapTex != null)
            {
                mapImg.image = mapTex;
            }
        }

        HideWindow(true);

        if (L2GameUI.Instance != null)
        {
            L2GameUI.Instance.WindowLoadComplete();
        }
    }

    public override void ShowWindow()
    {
        // ShowWindow is called by the server response (ShowMiniMap packet).
        // Do NOT send RequestShowMiniMap here — use RequestShowMap() for that.
        base.ShowWindow();
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUISound("system_open_01");
        }
        if (L2GameUI.Instance != null)
        {
            L2GameUI.Instance.WindowOpened(this);
        }
    }

    /// <summary>
    /// Sends RequestShowMiniMap to the server if window is hidden,
    /// or closes the window locally if it's already open.
    /// </summary>
    public void RequestToggleMap()
    {
        // If window is currently open (and visible), close it locally
        if (!_isWindowHidden && _windowEle != null && _windowEle.style.display == DisplayStyle.Flex)
        {
            HideWindow(false);
            return;
        }

        // Otherwise send request to the server to open/show the map
        if (GameClient.Instance != null && GameClient.Instance.ClientPacketHandler != null)
        {
            GameClient.Instance.ClientPacketHandler.SendRequestShowMiniMap();
        }
        else
        {
            // Offline/fallback: toggle locally if no network
            ShowWindow();
        }
    }

    public override void HideWindow(bool silent)
    {
        base.HideWindow(silent);
        if (!silent && L2GameUI.Instance != null)
        {
            L2GameUI.Instance.WindowClosed(this);
        }
    }
}
