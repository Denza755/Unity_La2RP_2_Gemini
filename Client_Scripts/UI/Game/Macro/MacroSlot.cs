using UnityEngine;
using UnityEngine.UIElements;

public class MacroSlot : L2DraggableSlot
{
    public static readonly string[] MacroIconNames = new string[]
    {
        "action003",                      // 0: Sword / Attack
        "skill0000",                      // 1: Magic hand
        "action007",                      // 2: Target
        "action008",                      // 3: Pickup
        "action001",                      // 4: Sit/Stand
        "action005",                      // 5: Trade
        "action010",                      // 6: Assist
        "etc_scroll_of_resurrection_i00", // 7: Resurrection
        "etc_potion_red_i00",             // 8: HP Potion
        "etc_potion_blue_i00",            // 9: MP Potion
        "etc_spellbook_i00",              // 10: Spellbook
        "action018"                       // 11: Private store
    };

    public Macro MacroData { get; private set; }
    private Label _acronymLabel;

    public MacroSlot(VisualElement slotElement, int position, SlotType slotType)
        : base(position, slotElement, slotType, true, false)
    {
        _slotElement = slotElement;
        _position = position;

        EnsureAcronymLabel();
    }

    private void EnsureAcronymLabel()
    {
        if (_slotElement == null) return;

        _acronymLabel = _slotElement.Q<Label>("MacroAcronym");
        if (_acronymLabel == null)
        {
            _acronymLabel = new Label();
            _acronymLabel.name = "MacroAcronym";
            _acronymLabel.style.position = UnityEngine.UIElements.Position.Absolute;
            _acronymLabel.style.left = 0;
            _acronymLabel.style.right = 0;
            _acronymLabel.style.bottom = 1;
            _acronymLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _acronymLabel.style.fontSize = 9;
            _acronymLabel.style.color = new Color(1f, 0.85f, 0.2f, 1f);
            _acronymLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _acronymLabel.pickingMode = PickingMode.Ignore;
            _slotElement.Add(_acronymLabel);
        }
    }

    public void AssignMacro(Macro macro)
    {
        MacroData = macro;
        if (macro == null)
        {
            _empty = true;
            _id = -1;
            if (_slotBg != null) _slotBg.style.backgroundImage = null;
            if (_acronymLabel != null) _acronymLabel.style.display = DisplayStyle.None;
            if (_slotDragManipulator != null) _slotDragManipulator.enabled = false;
            return;
        }

        _empty = false;
        _id = macro.Id;
        _name = macro.Name;
        _description = macro.Description;

        _slotElement.RemoveFromClassList("empty");
        if (_slotDragManipulator != null) _slotDragManipulator.enabled = true;

        EnsureAcronymLabel();
        if (_acronymLabel != null)
        {
            _acronymLabel.text = macro.Acronym;
            _acronymLabel.style.display = string.IsNullOrEmpty(macro.Acronym) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        string iconName = GetMacroIconName(macro.Icon);
        _icon = iconName;
        Texture2D tex = IconTable.Instance.LoadTextureByName(iconName);
        if (_slotBg != null)
        {
            _slotBg.style.backgroundImage = tex;
        }

        string tooltip = $"<b>{macro.Name}</b>";
        if (!string.IsNullOrEmpty(macro.Acronym))
        {
            tooltip += $" [{macro.Acronym}]";
        }
        if (!string.IsNullOrEmpty(macro.Description))
        {
            tooltip += $"\n{macro.Description}";
        }
        if (macro.Commands != null && macro.Commands.Count > 0)
        {
            tooltip += "\n---";
            for (int i = 0; i < macro.Commands.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(macro.Commands[i].Command))
                {
                    tooltip += $"\n{macro.Commands[i].Command}";
                }
            }
        }

        if (_tooltipManipulator != null)
        {
            _tooltipManipulator.SetValue(tooltip);
        }
        else
        {
            _tooltipManipulator = new TooltipManipulator(_slotElement, _slotType, tooltip);
            _slotElement.AddManipulator(_tooltipManipulator);
        }
    }

    public static string GetMacroIconName(int iconIndex)
    {
        if (iconIndex >= 0 && iconIndex < MacroIconNames.Length)
        {
            return MacroIconNames[iconIndex];
        }
        return MacroIconNames[0];
    }

    protected override void HandleLeftClick()
    {
        if (MacroData != null)
        {
            MacroExecutor.Instance.ExecuteMacro(MacroData);
        }
    }

    protected override void HandleRightClick()
    {
        if (MacroData != null && MacroWindow.Instance != null)
        {
            MacroWindow.Instance.OpenEditMacro(MacroData);
        }
    }

    protected override void HandleMiddleClick()
    {
    }
}
