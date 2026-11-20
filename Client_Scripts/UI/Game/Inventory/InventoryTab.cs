using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class InventoryTab : L2Tab
{
    private int _itemCount = 0;

    public List<ItemType1> _filteredCategories;
    private L2SlotContainer _slotContainer;
    [SerializeField] private bool _mainTab;

    public override void Initialize(L2TabView tabView, VisualElement tabContainer, VisualElement tabHeader)
    {
        base.Initialize(tabView, tabContainer, tabHeader);

        // _contentContainer = tabContainer.Q<VisualElement>("Content");
        _slotContainer = new L2SlotContainer();
        _slotContainer.Initialize(tabContainer, 9, PlayerInventory.Instance.InventorySize);
    }

    public void UpdateItemList(List<ItemInstance> items)
    {
        L2Slot.SlotType slotType = _mainTab ? slotType = L2Slot.SlotType.Inventory : L2Slot.SlotType.InventoryBis;
        _slotContainer.UpdateSlots(PlayerInventory.Instance.InventorySize, InventoryWindow.Instance.Expanded ? 12 : 9, slotType);

        // _slotContainer.AssignItemsToSlots(items);

        // Assign items to slots
        _itemCount = 0;
        items.ForEach(item =>
        {
            if (item.Location == ItemLocation.Inventory)
            {
                bool isQuestItem = item.Type2 == ItemType2.TYPE2_QUEST;

                // Main tab excludes quest items
                if (_mainTab && isQuestItem)
                {
                    return;
                }

                if (_filteredCategories == null || _filteredCategories.Count == 0)
                {
                    if (_mainTab)
                    {
                        if (item.Slot >= 0 && item.Slot < _slotContainer.Slots.Length)
                        {
                            ((InventorySlot)_slotContainer.Slots[item.Slot]).AssignItem(item);
                        }
                        else
                        {
                            Debug.LogWarning($"InventoryTab: item {item.ItemId} has invalid slot {item.Slot} (available slots: {_slotContainer.Slots.Length}).");
                        }
                    }
                    else if (isQuestItem)
                    {
                        if (_itemCount < _slotContainer.Slots.Length)
                        {
                            ((InventorySlot)_slotContainer.Slots[_itemCount++]).AssignItem(item);
                        }
                    }
                }
                else if (_filteredCategories.Contains(item.Type1) || (isQuestItem && _filteredCategories.Contains(ItemType1.TYPE1_ITEM_QUESTITEM_ADENA)))
                {
                    if (_itemCount < _slotContainer.Slots.Length)
                    {
                        ((InventorySlot)_slotContainer.Slots[_itemCount++]).AssignItem(item);
                    }
                }
            }
        });
    }

    public override void SelectSlot(int slotPosition)
    {
        _slotContainer.SelectSlot(slotPosition);
    }

    protected override void OnTabHeaderClicked()
    {
        base.OnTabHeaderClicked();
    }
}
