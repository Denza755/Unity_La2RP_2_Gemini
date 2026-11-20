using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MacroWindow : L2PopupWindow
{
    private static MacroWindow _instance;
    public static MacroWindow Instance => _instance;

    private VisualElement _mainView;
    private VisualElement _editView;
    private VisualElement _macroItemsContainer;
    private Label _emptyMacroLabel;

    private Button _addMacroBtn;
    private Button _editMacroBtn;
    private Button _deleteMacroBtn;

    // Edit view elements
    private Label _editTitleLabel;
    private TextField _nameInput;
    private TextField _acronymInput;
    private TextField _descInput;
    private VisualElement _iconPickerContainer;
    private VisualElement _commandsContainer;
    private List<TextField> _cmdInputs = new List<TextField>();
    private Button _saveMacroBtn;
    private Button _cancelEditBtn;

    private List<Button> _iconPickerButtons = new List<Button>();
    private int _selectedIconIndex = 0;
    private Macro _selectedMacro = null;
    private int _editingMacroId = 0;

    private readonly List<MacroSlot> _activeSlots = new List<MacroSlot>();

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
    }

    private void OnDestroy()
    {
        if (PlayerMacros.Instance != null)
        {
            PlayerMacros.Instance.OnMacrosChanged -= RefreshMacroList;
        }
        _instance = null;
    }

    protected override void LoadAssets()
    {
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/MacroWindow/MacroWindow");
    }

    protected override void InitWindow(VisualElement root)
    {
        base.InitWindow(root);

        VisualElement dragArea = GetElementByClass("drag-area");
        if (dragArea != null)
        {
            DragManipulator drag = new DragManipulator(dragArea, _windowEle, this);
            dragArea.AddManipulator(drag);
        }

        RegisterCloseWindowEvent("btn-close-frame");
        RegisterClickWindowEvent(_windowEle, dragArea);

        Label windowName = (Label)GetElementById("windows-name-label");
        if (windowName != null)
        {
            windowName.text = "Macros";
        }

        _mainView = GetElementById("MainView");
        _editView = GetElementById("EditView");
        _macroItemsContainer = GetElementById("MacroItemsContainer");
        _emptyMacroLabel = (Label)GetElementById("EmptyMacroLabel");

        _addMacroBtn = (Button)GetElementById("AddMacroBtn");
        _editMacroBtn = (Button)GetElementById("EditMacroBtn");
        _deleteMacroBtn = (Button)GetElementById("DeleteMacroBtn");

        _editTitleLabel = (Label)GetElementById("EditTitleLabel");
        _nameInput = (TextField)GetElementById("MacroNameInput");
        _acronymInput = (TextField)GetElementById("MacroAcronymInput");
        _descInput = (TextField)GetElementById("MacroDescInput");
        _iconPickerContainer = GetElementById("IconPickerContainer");
        _commandsContainer = GetElementById("CommandLinesContainer");
        _saveMacroBtn = (Button)GetElementById("SaveMacroBtn");
        _cancelEditBtn = (Button)GetElementById("CancelEditBtn");

        BuildIconPicker();
        BuildCommandInputs();

        if (_addMacroBtn != null)
        {
            _addMacroBtn.AddManipulator(new ButtonClickSoundManipulator(_addMacroBtn));
            _addMacroBtn.clicked += OpenNewMacro;
        }

        if (_editMacroBtn != null)
        {
            _editMacroBtn.AddManipulator(new ButtonClickSoundManipulator(_editMacroBtn));
            _editMacroBtn.clicked += () =>
            {
                if (_selectedMacro != null) OpenEditMacro(_selectedMacro);
            };
        }

        if (_deleteMacroBtn != null)
        {
            _deleteMacroBtn.AddManipulator(new ButtonClickSoundManipulator(_deleteMacroBtn));
            _deleteMacroBtn.clicked += HandleDeleteMacro;
        }

        if (_saveMacroBtn != null)
        {
            _saveMacroBtn.AddManipulator(new ButtonClickSoundManipulator(_saveMacroBtn));
            _saveMacroBtn.clicked += HandleSaveMacro;
        }

        if (_cancelEditBtn != null)
        {
            _cancelEditBtn.AddManipulator(new ButtonClickSoundManipulator(_cancelEditBtn));
            _cancelEditBtn.clicked += CloseEditView;
        }

        if (PlayerMacros.Instance != null)
        {
            PlayerMacros.Instance.OnMacrosChanged += RefreshMacroList;
        }
    }

    protected override IEnumerator BuildWindow(VisualElement root)
    {
        InitWindow(root);

        yield return new WaitForEndOfFrame();

        // Center window on screen
        _windowEle.style.left = new Length(50, LengthUnit.Percent);
        _windowEle.style.top = new Length(50, LengthUnit.Percent);
        _windowEle.style.translate = new StyleTranslate(new Translate(new Length(-50, LengthUnit.Percent), new Length(-50, LengthUnit.Percent)));

        RefreshMacroList();
        HideWindow(true);

        L2GameUI.Instance.WindowLoadComplete();
    }

    private void BuildIconPicker()
    {
        if (_iconPickerContainer == null) return;
        _iconPickerContainer.Clear();
        _iconPickerButtons.Clear();

        for (int i = 0; i < MacroSlot.MacroIconNames.Length; i++)
        {
            int index = i;
            Button btn = new Button();
            btn.AddToClassList("macro-picker-icon-btn");

            string iconName = MacroSlot.GetMacroIconName(index);
            Texture2D tex = IconTable.Instance.LoadTextureByName(iconName);
            if (tex != null)
            {
                btn.style.backgroundImage = tex;
            }

            btn.clicked += () => SelectIcon(index);
            _iconPickerContainer.Add(btn);
            _iconPickerButtons.Add(btn);
        }

        SelectIcon(0);
    }

    private void SelectIcon(int index)
    {
        _selectedIconIndex = index;
        for (int i = 0; i < _iconPickerButtons.Count; i++)
        {
            if (i == index)
            {
                _iconPickerButtons[i].AddToClassList("selected");
            }
            else
            {
                _iconPickerButtons[i].RemoveFromClassList("selected");
            }
        }
    }

    private void BuildCommandInputs()
    {
        if (_commandsContainer == null) return;
        _commandsContainer.Clear();
        _cmdInputs.Clear();

        for (int i = 0; i < 12; i++)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("macro-cmd-line-row");

            Label num = new Label((i + 1).ToString());
            num.AddToClassList("macro-cmd-num");
            row.Add(num);

            TextField input = new TextField();
            input.AddToClassList("macro-cmd-input");
            row.Add(input);

            _commandsContainer.Add(row);
            _cmdInputs.Add(input);
        }
    }

    public void RefreshMacroList()
    {
        if (_macroItemsContainer == null) return;

        foreach (var slot in _activeSlots)
        {
            slot.ClearManipulators();
        }
        _activeSlots.Clear();
        _macroItemsContainer.Clear();

        List<Macro> macros = PlayerMacros.Instance != null ? PlayerMacros.Instance.GetAllMacros() : new List<Macro>();

        if (macros.Count == 0)
        {
            if (_emptyMacroLabel != null) _emptyMacroLabel.style.display = DisplayStyle.Flex;
            _selectedMacro = null;
            return;
        }

        if (_emptyMacroLabel != null) _emptyMacroLabel.style.display = DisplayStyle.None;

        for (int i = 0; i < macros.Count; i++)
        {
            Macro macro = macros[i];
            VisualElement row = new VisualElement();
            row.AddToClassList("macro-item-row");

            if (_selectedMacro != null && _selectedMacro.Id == macro.Id)
            {
                row.AddToClassList("selected");
            }

            // Slot button for drag & drop and execution
            VisualElement slotWrapper = new VisualElement();
            slotWrapper.AddToClassList("macro-item-slot-wrapper");

            Button slotBtn = new Button();
            slotBtn.AddToClassList("l2-slot");
            slotBtn.AddToClassList("macro-slot");

            VisualElement bgContainer = new VisualElement();
            bgContainer.pickingMode = PickingMode.Ignore;
            bgContainer.AddToClassList("slot-bg-container");

            VisualElement slotBg = new VisualElement();
            slotBg.name = "SlotBg";
            slotBg.pickingMode = PickingMode.Ignore;
            slotBg.AddToClassList("slot-bg");

            bgContainer.Add(slotBg);
            slotBtn.Add(bgContainer);
            slotWrapper.Add(slotBtn);
            row.Add(slotWrapper);

            MacroSlot macroSlot = new MacroSlot(slotBtn, i, L2Slot.SlotType.Macro);
            macroSlot.AssignMacro(macro);
            _activeSlots.Add(macroSlot);

            // Text Info
            VisualElement info = new VisualElement();
            info.AddToClassList("macro-item-info");

            Label nameLabel = new Label(macro.Name);
            nameLabel.AddToClassList("macro-item-name");
            info.Add(nameLabel);

            if (!string.IsNullOrEmpty(macro.Description))
            {
                Label descLabel = new Label(macro.Description);
                descLabel.AddToClassList("macro-item-desc");
                info.Add(descLabel);
            }
            row.Add(info);

            row.RegisterCallback<ClickEvent>((evt) =>
            {
                SelectMacroRow(macro, row);
            });

            _macroItemsContainer.Add(row);
        }
    }

    private void SelectMacroRow(Macro macro, VisualElement selectedRow)
    {
        _selectedMacro = macro;

        foreach (var child in _macroItemsContainer.Children())
        {
            child.RemoveFromClassList("selected");
        }
        selectedRow?.AddToClassList("selected");
    }

    public void OpenNewMacro()
    {
        _editingMacroId = 0;
        if (_editTitleLabel != null) _editTitleLabel.text = "Create Macro";

        if (_nameInput != null) _nameInput.value = "";
        if (_acronymInput != null) _acronymInput.value = "";
        if (_descInput != null) _descInput.value = "";

        SelectIcon(0);

        for (int i = 0; i < _cmdInputs.Count; i++)
        {
            _cmdInputs[i].value = "";
        }

        if (_mainView != null) _mainView.style.display = DisplayStyle.None;
        if (_editView != null) _editView.style.display = DisplayStyle.Flex;
    }

    public void OpenEditMacro(Macro macro)
    {
        if (macro == null) return;

        _editingMacroId = macro.Id;
        if (_editTitleLabel != null) _editTitleLabel.text = $"Edit Macro: {macro.Name}";

        if (_nameInput != null) _nameInput.value = macro.Name;
        if (_acronymInput != null) _acronymInput.value = macro.Acronym;
        if (_descInput != null) _descInput.value = macro.Description;

        SelectIcon(macro.Icon);

        for (int i = 0; i < _cmdInputs.Count; i++)
        {
            if (macro.Commands != null && i < macro.Commands.Count)
            {
                _cmdInputs[i].value = macro.Commands[i]?.Command ?? "";
            }
            else
            {
                _cmdInputs[i].value = "";
            }
        }

        if (_mainView != null) _mainView.style.display = DisplayStyle.None;
        if (_editView != null) _editView.style.display = DisplayStyle.Flex;
    }

    public void CloseEditView()
    {
        if (_editView != null) _editView.style.display = DisplayStyle.None;
        if (_mainView != null) _mainView.style.display = DisplayStyle.Flex;
    }

    private void HandleSaveMacro()
    {
        string name = _nameInput?.value?.Trim() ?? "";
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Macro name cannot be empty.");
            return;
        }

        string acronym = _acronymInput?.value?.Trim() ?? "";
        if (acronym.Length > 4) acronym = acronym.Substring(0, 4);

        string desc = _descInput?.value?.Trim() ?? "";
        if (desc.Length > 32) desc = desc.Substring(0, 32);

        List<MacroCmd> commands = new List<MacroCmd>();
        for (int i = 0; i < _cmdInputs.Count; i++)
        {
            string cmdText = _cmdInputs[i]?.value?.Trim() ?? "";
            if (!string.IsNullOrEmpty(cmdText))
            {
                commands.Add(new MacroCmd(commands.Count + 1, 3, 0, 0, cmdText));
            }
        }

        Macro macro = new Macro(_editingMacroId, _selectedIconIndex, name, desc, acronym, commands);
        PlayerMacros.Instance?.RequestMakeMacro(macro);

        CloseEditView();
    }

    private void HandleDeleteMacro()
    {
        if (_selectedMacro == null) return;

        PlayerMacros.Instance?.RequestDeleteMacro(_selectedMacro.Id);
        _selectedMacro = null;
    }

    public override void ShowWindow()
    {
        base.ShowWindow();
        AudioManager.Instance?.PlayUISound("window_open");
        RefreshMacroList();
        L2GameUI.Instance?.WindowOpened(this);
    }

    public override void HideWindow(bool silent)
    {
        base.HideWindow(silent);
        CloseEditView();

        if (!silent)
        {
            AudioManager.Instance?.PlayUISound("window_close");
        }

        L2GameUI.Instance?.WindowClosed(this);
    }
}
