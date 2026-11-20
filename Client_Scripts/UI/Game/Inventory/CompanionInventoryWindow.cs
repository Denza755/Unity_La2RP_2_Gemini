using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CompanionInventoryWindow : L2PopupWindow
{
    private static CompanionInventoryWindow _instance;
    public static CompanionInventoryWindow Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("CompanionInventoryWindow");
                _instance = go.AddComponent<CompanionInventoryWindow>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private int _companionObjectId;
    public int CompanionObjectId => _companionObjectId;
    private string _companionName = "Companion";
    private int _adena = 0;
    private readonly List<ItemInstance> _items = new();

    private Label _windowTitle;
    private Label _inventoryCountLabel;
    private Label _adenaCountLabel;
    private VisualElement _gearContainer;
    private VisualElement _tabContainer;

    private readonly Dictionary<Paperdoll, VisualElement> _gearAnchors = new();
    private readonly Dictionary<Paperdoll, CompanionSlot> _gearSlots = new();
    private readonly List<CompanionSlot> _invSlots = new();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
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
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/InventoryWindow/InventoryWindow");
    }

    protected override void InitWindow(VisualElement root)
    {
        base.InitWindow(root);

        _windowTitle = (Label)GetElementById("windows-name-label");
        if (_windowTitle != null)
        {
            _windowTitle.text = "[Companion] " + _companionName;
        }

        var dragArea = GetElementByClass("drag-area");
        if (dragArea != null)
        {
            DragManipulator drag = new DragManipulator(dragArea, _windowEle, this);
            dragArea.AddManipulator(drag);
            RegisterClickWindowEvent(_windowEle, dragArea);
        }

        RegisterCloseWindowEvent("btn-close-frame");

        var expandBtn = GetElementByClass("expand-btn");
        if (expandBtn != null) expandBtn.style.display = DisplayStyle.None;

        var minBtn = GetElementByClass("minimize-btn");
        if (minBtn != null) minBtn.style.display = DisplayStyle.None;

        _inventoryCountLabel = GetLabelById("InventoryCount");
        _adenaCountLabel = GetLabelById("AdenaCount");

        InitGearAnchors();
        InitInventoryGrid();
    }

    protected override IEnumerator BuildWindow(VisualElement root)
    {
        InitWindow(root);

        yield return new WaitForEndOfFrame();

        CenterWindow();
        HideWindow(true);

        if (L2GameUI.Instance != null)
        {
            L2GameUI.Instance.WindowLoadComplete();
        }
    }

    private void InitGearAnchors()
    {
        _gearAnchors.Clear();
        _gearAnchors[Paperdoll.HEAD] = _windowEle.Q<VisualElement>("Helmet");
        _gearAnchors[Paperdoll.GLOVES] = _windowEle.Q<VisualElement>("Gloves");
        _gearAnchors[Paperdoll.CHEST] = _windowEle.Q<VisualElement>("Torso");
        _gearAnchors[Paperdoll.FEET] = _windowEle.Q<VisualElement>("Boots");
        _gearAnchors[Paperdoll.LEGS] = _windowEle.Q<VisualElement>("Legs");
        _gearAnchors[Paperdoll.RHAND] = _windowEle.Q<VisualElement>("Rhand");
        _gearAnchors[Paperdoll.LHAND] = _windowEle.Q<VisualElement>("Lhand");
        _gearAnchors[Paperdoll.NECK] = _windowEle.Q<VisualElement>("Neck");
        _gearAnchors[Paperdoll.REAR] = _windowEle.Q<VisualElement>("Rear");
        _gearAnchors[Paperdoll.LEAR] = _windowEle.Q<VisualElement>("Lear");
        _gearAnchors[Paperdoll.RFINGER] = _windowEle.Q<VisualElement>("Rring");
        _gearAnchors[Paperdoll.LFINGER] = _windowEle.Q<VisualElement>("Lring");
    }

    private void InitInventoryGrid()
    {
        var tabView = _windowEle.Q<VisualElement>("L2TabView");
        if (tabView == null)
        {
            return;
        }

        var header = tabView.Q<VisualElement>("Header");
        if (header != null)
        {
            header.style.display = DisplayStyle.None; // Companion uses single main inventory tab
        }

        var content = tabView.Q<VisualElement>("Content");
        if (content == null)
        {
            return;
        }

        content.Clear();
        var scrollView = new ScrollView(ScrollViewMode.Vertical);
        scrollView.style.flexGrow = 1;

        var grid = new VisualElement();
        grid.style.flexDirection = FlexDirection.Row;
        grid.style.flexWrap = Wrap.Wrap;
        grid.style.alignContent = Align.FlexStart;

        _invSlots.Clear();
        var slotTemplate = L2SlotManager.Instance.InventorySlotTemplate;

        for (int i = 0; i < 80; i++)
        {
            var slotElem = slotTemplate.Instantiate()[0];
            grid.Add(slotElem);
            var slot = new CompanionSlot(i, slotElem, L2Slot.SlotType.CompanionInventory, false);
            _invSlots.Add(slot);
        }

        scrollView.Add(grid);
        content.Add(scrollView);
    }

    public void SetCompanionInventory(int companionObjectId, string companionName, int adena, ItemInstance[] items)
    {
        _companionObjectId = companionObjectId;
        _companionName = string.IsNullOrEmpty(companionName) ? "Companion" : companionName;
        _adena = adena;

        _items.Clear();
        if (items != null)
        {
            _items.AddRange(items);
        }

        EnsureWindowCreated();

        if (_windowTitle != null)
        {
            _windowTitle.text = "[Companion] " + _companionName;
        }

        if (_adenaCountLabel != null)
        {
            _adenaCountLabel.text = $"{_adena:n0}";
        }

        // Open player inventory and companion inventory side-by-side
        if (InventoryWindow.Instance != null)
        {
            InventoryWindow.Instance.ShowWindow();
        }

        ShowWindow();

        PositionSideBySide();
        RefreshDisplay();
    }

    private void EnsureWindowCreated()
    {
        if (_windowEle == null)
        {
            var root = L2GameUI.Instance != null ? L2GameUI.Instance.RootVisualContainer : null;
            if (root != null)
            {
                AddWindow(root);
            }
        }
    }

    private void PositionSideBySide()
    {
        if (_windowEle == null)
        {
            return;
        }

        // Position companion window immediately to the right of player's inventory window
        if (InventoryWindow.Instance != null && InventoryWindow.Instance.WindowEle != null)
        {
            var playerWin = InventoryWindow.Instance.WindowEle;
            float left = playerWin.resolvedStyle.left;
            float top = playerWin.resolvedStyle.top;
            float width = playerWin.resolvedStyle.width;

            if (width > 50)
            {
                _windowEle.style.position = UnityEngine.UIElements.Position.Absolute;
                _windowEle.style.left = left + width + 10f;
                _windowEle.style.top = top;
                return;
            }
        }

        CenterWindow();
    }

    public void RefreshDisplay()
    {
        // 1. Gear puppet slots
        foreach (var kvp in _gearSlots)
        {
            kvp.Value?.ClearManipulators();
        }
        _gearSlots.Clear();

        var slotTemplate = L2SlotManager.Instance.InventorySlotTemplate;
        foreach (var kvp in _gearAnchors)
        {
            if (kvp.Value == null) continue;
            kvp.Value.Clear();

            var slotElem = slotTemplate.Instantiate()[0];
            kvp.Value.Add(slotElem);
            var slot = new CompanionSlot((int)kvp.Key, slotElem, L2Slot.SlotType.CompanionGear, true);
            _gearSlots[kvp.Key] = slot;
        }

        // 2. Clear unequipped slots
        for (int i = 0; i < _invSlots.Count; i++)
        {
            _invSlots[i].AssignEmpty();
        }

        int unequippedIndex = 0;
        int equippedCount = 0;

        foreach (var item in _items)
        {
            if (item == null) continue;

            if (item.Equipped)
            {
                equippedCount++;
                AssignGearItem(item);
            }
            else
            {
                if (unequippedIndex < _invSlots.Count)
                {
                    _invSlots[unequippedIndex].AssignItem(item);
                    unequippedIndex++;
                }
            }
        }

        if (_inventoryCountLabel != null)
        {
            _inventoryCountLabel.text = $"({unequippedIndex}/80)";
        }
    }

    private void AssignGearItem(ItemInstance item)
    {
        if (item.Type2 == ItemType2.TYPE2_WEAPON && item.ItemData?.Itemgrp is Weapongrp bowgrp && bowgrp.WeaponType == WeaponType.bow)
        {
            if (_gearSlots.TryGetValue(Paperdoll.RHAND, out var rSlot)) rSlot.AssignItem(item);
        }
        else if (item.Type2 == ItemType2.TYPE2_WEAPON && item.ItemData?.Itemgrp is Weapongrp wg &&
            (wg.WeaponType == WeaponType.bigblunt || wg.WeaponType == WeaponType.bigword ||
             wg.WeaponType == WeaponType.dual || wg.WeaponType == WeaponType.pole || wg.WeaponType == WeaponType.fist))
        {
            if (_gearSlots.TryGetValue(Paperdoll.RHAND, out var rSlot)) rSlot.AssignItem(item);
            if (_gearSlots.TryGetValue(Paperdoll.LHAND, out var lSlot)) lSlot.AssignItem(item);
        }
        else if (item.Slot == (int)Paperdoll.CHEST && item.BodyPart == ItemSlot.SLOT_FULL_ARMOR)
        {
            if (_gearSlots.TryGetValue(Paperdoll.CHEST, out var cSlot)) cSlot.AssignItem(item);
            if (_gearSlots.TryGetValue(Paperdoll.LEGS, out var legSlot)) legSlot.AssignItem(item);
        }
        else
        {
            Paperdoll slot = (Paperdoll)item.Slot;
            if (slot != Paperdoll.NULL && _gearSlots.TryGetValue(slot, out var targetSlot))
            {
                targetSlot.AssignItem(item);
            }
        }
    }

    public void TransferItemFromPlayer(int itemObjectId, int count)
    {
        GameClient.Instance?.ClientPacketHandler?.SendPacket(new RequestCompanionItemActionPacket(0, _companionObjectId, itemObjectId, count > 0 ? count : 1));
    }

    public void TransferItemToPlayer(int itemObjectId, int count)
    {
        GameClient.Instance?.ClientPacketHandler?.SendPacket(new RequestCompanionItemActionPacket(1, _companionObjectId, itemObjectId, count > 0 ? count : 1));
    }

    public void EquipCompanionItem(int itemObjectId)
    {
        GameClient.Instance?.ClientPacketHandler?.SendPacket(new RequestCompanionItemActionPacket(2, _companionObjectId, itemObjectId, 1));
    }
}
