using UnityEngine;
using UnityEngine.UIElements;

public class CompanionSlot : InventorySlot
{
    private readonly bool _isGear;

    public CompanionSlot(int position, VisualElement slotElement, L2SlotContainer slotContainer, SlotType slotType, bool isGear = false)
        : base(position, slotElement, slotContainer, slotType)
    {
        _isGear = isGear;
    }

    public CompanionSlot(int position, VisualElement slotElement, SlotType slotType, bool isGear = false)
        : base(position, slotElement, slotType)
    {
        _isGear = isGear;
    }

    public override void UseItem()
    {
        if (!_empty && CompanionInventoryWindow.Instance != null)
        {
            CompanionInventoryWindow.Instance.EquipCompanionItem(ObjectId);
        }
    }

    public void AssignEmpty()
    {
        _empty = true;
        _id = 0;
        _name = "";
        _description = "";
        _icon = "";
        _objectId = -1;
        _assignedItem = null;

        if (_slotElement != null)
        {
            _slotElement.AddToClassList("empty");
        }

        if (_slotBg != null)
        {
            _slotBg.style.backgroundImage = null;
        }

        if (_tooltipManipulator != null)
        {
            _tooltipManipulator.SetValue("");
        }

        if (_slotDragManipulator != null)
        {
            _slotDragManipulator.enabled = false;
        }
    }

    public void ResetSlot() => AssignEmpty();

    protected override void HandleRightClick()
    {
        UseItem();
    }
}
