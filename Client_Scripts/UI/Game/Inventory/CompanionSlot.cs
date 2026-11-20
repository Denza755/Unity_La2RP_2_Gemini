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

    protected override void HandleRightClick()
    {
        UseItem();
    }
}
