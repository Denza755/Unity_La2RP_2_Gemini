using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static NativeFunctions;

public class L2GameUI : L2UI
{
    private NativeCoords _lastMousePosition;
    [SerializeField] private bool _mouseEnabled = true;

    private List<L2PopupWindow> _openedWindows;

    private static L2GameUI _instance;
    public static L2GameUI Instance { get { return _instance; } }
    public bool IsTyping { get; set; }

    public bool IsFocusedOnTextInput()
    {
        if (IsTyping) return true;
        if (ChatWindow.Instance != null && ChatWindow.Instance.ChatOpened) return true;
        var focused = _rootVisualContainer?.panel?.focusController?.focusedElement;
        if (focused != null)
        {
            if (focused is TextField) return true;
            string typeName = focused.GetType().Name;
            if (typeName.Contains("TextInput") || typeName.Contains("TextField")) return true;
        }
        return false;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }

        _openedWindows = new List<L2PopupWindow>();
    }

    private void Start()
    {
        MouseOverUI = false;
        _windowsLoaded = 0;

        if (RulerHUDWindow.Instance == null)
        {
            gameObject.AddComponent<RulerHUDWindow>();
        }
        if (ChatBubbleManager.Instance == null)
        {
            gameObject.AddComponent<ChatBubbleManager>();
        }
        if (QuestWindow.Instance == null)
        {
            gameObject.AddComponent<QuestWindow>();
        }
        if (DropListWindow.Instance == null)
        {
            gameObject.AddComponent<DropListWindow>();
        }
        if (TradeWindow.Instance == null)
        {
            gameObject.AddComponent<TradeWindow>();
        }
        if (WarehouseWindow.Instance == null)
        {
            gameObject.AddComponent<WarehouseWindow>();
        }
        if (PrivateStoreWindow.Instance == null)
        {
            gameObject.AddComponent<PrivateStoreWindow>();
        }
        if (ClanWindow.Instance == null)
        {
            gameObject.AddComponent<ClanWindow>();
        }
        if (RankingWindow.Instance == null)
        {
            gameObject.AddComponent<RankingWindow>();
        }
        if (MacroWindow.Instance == null)
        {
            gameObject.AddComponent<MacroWindow>();
        }
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    protected override void Update()
    {
        base.Update();

        if (InputManager.Instance != null && InputManager.Instance.TurnCamera)
        {
            DisableMouse();
        }
        else
        {
            EnableMouse();
        }

        CheckUIInputs();
    }

    protected override void LoadUI()
    {
        base.LoadUI();

        if (L2LoginUI.Instance != null)
        {
            StartLoading();
        }

        if (ChatWindow.Instance != null)
        {
            ChatWindow.Instance.AddWindow(_rootVisualContainer);
        }
        if (SystemMenuWindow.Instance != null)
        {
            SystemMenuWindow.Instance.AddWindow(_rootVisualContainer);
        }
        if (MenuWindow.Instance != null)
        {
            MenuWindow.Instance.AddWindow(_rootVisualContainer);
        }
        if (StatusWindow.Instance != null)
        {
            StatusWindow.Instance.AddWindow(_rootVisualContainer);
        }
        if (SkillbarWindow.Instance != null)
        {
            SkillbarWindow.Instance.AddWindow(_rootVisualContainer);
        }
        if (InventoryWindow.Instance != null)
        {
            InventoryWindow.Instance.AddWindow(_rootVisualContainer);
            InventoryWindow.Instance.HideWindow(true);
        }
        if (CharacterInfoWindow.Instance != null)
        {
            CharacterInfoWindow.Instance.AddWindow(_rootVisualContainer);
            CharacterInfoWindow.Instance.HideWindow(true);
        }
        if (ActionWindow.Instance != null)
        {
            ActionWindow.Instance.AddWindow(_rootVisualContainer);
            ActionWindow.Instance.HideWindow(true);
        }
        if (NpcHtmlWindow.Instance != null)
        {
            NpcHtmlWindow.Instance.AddWindow(_rootVisualContainer);
        }
        if (ShopWindow.Instance != null)
        {
            ShopWindow.Instance.AddWindow(_rootVisualContainer);
            ShopWindow.Instance.HideWindow(true);
        }
        if (RestartLocationWindow.Instance != null)
        {
            RestartLocationWindow.Instance.AddWindow(_rootVisualContainer);
        }
        if (TargetWindow.Instance != null)
        {
            TargetWindow.Instance.AddWindow(_rootVisualContainer);
        }
        if (ExitWindow.Instance != null)
        {
            ExitWindow.Instance.AddWindow(_rootVisualContainer);
        }
        if (L2ToolTip.Instance != null)
        {
            L2ToolTip.Instance.AddWindow(_tooltipVisualContainer);
            L2ToolTip.Instance.HideWindow(true);
        }
        if (L2SlotManager.Instance != null)
        {
            L2SlotManager.Instance.AddWindow(_slotVisualContainer);
            L2SlotManager.Instance.HideWindow(true);
        }
        if (SkillWindow.Instance != null)
        {
            SkillWindow.Instance.AddWindow(_rootVisualContainer);
            SkillWindow.Instance.HideWindow(true);
        }
        if (SkillLearnWindow.Instance != null)
        {
            SkillLearnWindow.Instance.AddWindow(_rootVisualContainer);
            SkillLearnWindow.Instance.HideWindow(true);
        }
        if (BuffWindow.Instance != null)
        {
            BuffWindow.Instance.AddWindow(_rootVisualContainer);
            BuffWindow.Instance.HideWindow(true);
        }
        if (DebuffWindow.Instance != null)
        {
            DebuffWindow.Instance.AddWindow(_rootVisualContainer);
            DebuffWindow.Instance.HideWindow(true);
        }
        if (L2ConfirmWindow.Instance != null)
        {
            L2ConfirmWindow.Instance.AddWindow(_popupVisualContainer);
            L2ConfirmWindow.Instance.HideWindow(true);
        }
        if (L2InputAmountWindow.Instance != null)
        {
            L2InputAmountWindow.Instance.AddWindow(_popupVisualContainer);
            L2InputAmountWindow.Instance.HideWindow(true);
        }
        if (MapWindow.Instance != null)
        {
            MapWindow.Instance.AddWindow(_rootVisualContainer);
            MapWindow.Instance.HideWindow(true);
        }
        if (RulerHUDWindow.Instance != null)
        {
            RulerHUDWindow.Instance.AddWindow(_rootVisualContainer);
            if (RulerManager.Instance != null && !RulerManager.Instance.HasRulerAccess())
            {
                RulerHUDWindow.Instance.HideWindow(true);
            }
        }
        if (QuestWindow.Instance != null)
        {
            QuestWindow.Instance.AddWindow(_rootVisualContainer);
            QuestWindow.Instance.HideWindow(true);
        }
        if (DropListWindow.Instance != null)
        {
            DropListWindow.Instance.AddWindow(_rootVisualContainer);
            DropListWindow.Instance.HideWindow(true);
        }
        if (TradeWindow.Instance != null)
        {
            TradeWindow.Instance.AddWindow(_rootVisualContainer);
            TradeWindow.Instance.HideWindow(true);
        }
        if (WarehouseWindow.Instance != null)
        {
            WarehouseWindow.Instance.AddWindow(_rootVisualContainer);
            WarehouseWindow.Instance.HideWindow(true);
        }
        if (PrivateStoreWindow.Instance != null)
        {
            PrivateStoreWindow.Instance.AddWindow(_rootVisualContainer);
            PrivateStoreWindow.Instance.HideWindow(true);
        }
        if (ClanWindow.Instance != null)
        {
            ClanWindow.Instance.AddWindow(_rootVisualContainer);
            ClanWindow.Instance.HideWindow(true);
        }
        if (RankingWindow.Instance != null)
        {
            RankingWindow.Instance.AddWindow(_rootVisualContainer);
            RankingWindow.Instance.HideWindow(true);
        }
        if (MacroWindow.Instance != null)
        {
            MacroWindow.Instance.AddWindow(_rootVisualContainer);
            MacroWindow.Instance.HideWindow(true);
        }
    }

    public void EnableMouse()
    {
        if (!_mouseEnabled)
        {
            _mouseEnabled = true;
            NativeFunctions.SetCursorPos(_lastMousePosition.X, _lastMousePosition.Y);
        }
    }

    public void DisableMouse()
    {
        if (_mouseEnabled)
        {
            NativeFunctions.GetCursorPos(out _lastMousePosition);
            _mouseEnabled = false;
        }
        else
        {
            NativeFunctions.SetCursorPos(_lastMousePosition.X, _lastMousePosition.Y);
        }
    }

    public void CheckUIInputs()
    {
        if (InputManager.Instance.OpenCharacerStatus)
        {
            if (CharacterInfoWindow.Instance != null)
            {
                CharacterInfoWindow.Instance.ToggleHideWindow();
            }
        }

        if (InputManager.Instance.OpenInventory)
        {
            if (InventoryWindow.Instance != null)
            {
                InventoryWindow.Instance.ToggleHideWindow();
            }
        }

        if (InputManager.Instance.OpenSystemMenu)
        {
            if (SystemMenuWindow.Instance != null)
            {
                SystemMenuWindow.Instance.ToggleHideWindow();
            }
        }

        if (InputManager.Instance.OpenActions)
        {
            if (ActionWindow.Instance != null)
            {
                ActionWindow.Instance.ToggleHideWindow();
            }
        }

        if (InputManager.Instance.CloseWindow)
        {
            if (PlayerStateMachine.Instance?.State == PlayerState.SKILL) // Prioritize skill cast cancel
            {
                return;
            }

            if (ChatWindow.Instance != null && ChatWindow.Instance.ChatOpened)
            {
                ChatWindow.Instance.CloseChat(false);
                return;
            }

            if (_openedWindows != null && _openedWindows.Count > 0)
            {
                _openedWindows[_openedWindows.Count - 1].HideWindow(false);
            }
            else
            {
                SystemMenuWindow.Instance.ToggleHideWindow();
            }
        }

        if (InputManager.Instance.OpenSkills)
        {
            if (SkillWindow.Instance != null)
            {
                SkillWindow.Instance.ToggleHideWindow();
            }
        }

        if (InputManager.Instance.OpenMap)
        {
            if (MapWindow.Instance != null)
            {
                MapWindow.Instance.RequestToggleMap();
            }
        }

        if (InputManager.Instance.OpenQuest)
        {
            if (QuestWindow.Instance != null)
            {
                QuestWindow.Instance.ToggleHideWindow();
            }
        }

        if (InputManager.Instance.OpenClan)
        {
            if (ClanWindow.Instance != null)
            {
                ClanWindow.Instance.ToggleClanWindow();
            }
        }

        if (InputManager.Instance.OpenMacro)
        {
            if (MacroWindow.Instance != null)
            {
                MacroWindow.Instance.ToggleHideWindow();
            }
        }
    }

    public void WindowOpened(L2PopupWindow popupWindow)
    {
        _openedWindows.Add(popupWindow);
    }

    public void WindowClosed(L2PopupWindow popupWindow)
    {
        _openedWindows.Remove(popupWindow);
    }

    public void OnGUI()
    {
        if (_mouseEnabled)
        {
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            UnityEngine.Cursor.visible = false;
            UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        }
    }
}
