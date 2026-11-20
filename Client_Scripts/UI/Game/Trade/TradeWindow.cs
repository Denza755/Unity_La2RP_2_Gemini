using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TradeWindow : L2PopupWindow
{
    public class TradeItemData
    {
        public int ObjectId;
        public int ItemId;
        public int Count;
        public int Enchant;

        public TradeItemData(int objectId, int itemId, int count, int enchant)
        {
            ObjectId = objectId;
            ItemId = itemId;
            Count = count;
            Enchant = enchant;
        }
    }

    private static TradeWindow _instance;
    public static TradeWindow Instance => _instance;

    public int PartnerObjectId { get; private set; }

    private VisualElement _myInventoryGrid;
    private VisualElement _myTradeGrid;
    private VisualElement _partnerTradeGrid;

    private Label _myStatusLabel;
    private Label _partnerStatusLabel;
    private Label _partnerSectionLabel;

    private Button _okButton;
    private Button _cancelButton;

    private readonly List<ItemInstance> _myInventoryItems = new();
    private readonly List<TradeItemData> _myTradeItems = new();
    private readonly List<TradeItemData> _partnerTradeItems = new();

    private bool _myConfirmed = false;
    private bool _partnerConfirmed = false;

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
        _instance = null;
    }

    protected override void LoadAssets()
    {
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/TradeWindow/TradeWindow");
    }

    protected override void InitWindow(VisualElement root)
    {
        base.InitWindow(root);

        StyleSheet uss = Resources.Load<StyleSheet>("Data/UI/_Elements/Game/TradeWindow/TradeWindow");
        if (uss != null && !_windowEle.styleSheets.Contains(uss))
        {
            _windowEle.styleSheets.Add(uss);
        }

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
            windowName.text = "Trade";
        }

        _myInventoryGrid = GetElementById("MyInventoryGrid");
        _myTradeGrid = GetElementById("MyTradeGrid");
        _partnerTradeGrid = GetElementById("PartnerTradeGrid");

        _myStatusLabel = GetLabelById("MyStatusLabel");
        _partnerStatusLabel = GetLabelById("PartnerStatusLabel");
        _partnerSectionLabel = GetLabelById("PartnerSectionLabel");

        _okButton = _windowEle.Q<Button>("TradeOkButton");
        _cancelButton = _windowEle.Q<Button>("TradeCancelButton");

        if (_okButton != null)
        {
            _okButton.clicked += OnClickOk;
        }

        if (_cancelButton != null)
        {
            _cancelButton.clicked += OnClickCancel;
        }
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

    public void StartTrade(int partnerObjectId, ItemInstance[] items)
    {
        PartnerObjectId = partnerObjectId;
        _myConfirmed = false;
        _partnerConfirmed = false;

        _myInventoryItems.Clear();
        if (items != null)
        {
            _myInventoryItems.AddRange(items);
        }

        _myTradeItems.Clear();
        _partnerTradeItems.Clear();

        UpdateStatusLabels();
        RefreshInventoryGrid();
        RefreshMyTradeGrid();
        RefreshPartnerTradeGrid();

        string partnerName = WorldSpawner.Instance?.GetEntity(partnerObjectId)?.Identity?.Name;
        if (string.IsNullOrEmpty(partnerName))
        {
            partnerName = "Partner";
        }
        if (_partnerSectionLabel != null)
        {
            _partnerSectionLabel.text = $"{partnerName}'s Offer";
        }

        ShowWindow();
    }

    private void UpdateStatusLabels()
    {
        if (_myStatusLabel != null)
        {
            _myStatusLabel.text = _myConfirmed ? "CONFIRMED" : "Pending";
            if (_myConfirmed) _myStatusLabel.AddToClassList("trade-status-ok");
            else _myStatusLabel.RemoveFromClassList("trade-status-ok");
        }

        if (_partnerStatusLabel != null)
        {
            _partnerStatusLabel.text = _partnerConfirmed ? "CONFIRMED" : "Pending";
            if (_partnerConfirmed) _partnerStatusLabel.AddToClassList("trade-status-ok");
            else _partnerStatusLabel.RemoveFromClassList("trade-status-ok");
        }
    }

    private void RefreshInventoryGrid()
    {
        if (_myInventoryGrid == null) return;
        _myInventoryGrid.Clear();

        foreach (var item in _myInventoryItems)
        {
            var currentItem = item;
            VisualElement slot = CreateSlotElement(currentItem.ItemId, currentItem.Count, currentItem.EnchantLevel);

            slot.RegisterCallback<ClickEvent>(evt =>
            {
                if (_myConfirmed) return;
                OnItemClicked(currentItem);
            });

            _myInventoryGrid.Add(slot);
        }
    }

    private void RefreshMyTradeGrid()
    {
        if (_myTradeGrid == null) return;
        _myTradeGrid.Clear();

        foreach (var item in _myTradeItems)
        {
            VisualElement slot = CreateSlotElement(item.ItemId, item.Count, item.Enchant);
            _myTradeGrid.Add(slot);
        }
    }

    private void RefreshPartnerTradeGrid()
    {
        if (_partnerTradeGrid == null) return;
        _partnerTradeGrid.Clear();

        foreach (var item in _partnerTradeItems)
        {
            VisualElement slot = CreateSlotElement(item.ItemId, item.Count, item.Enchant);
            _partnerTradeGrid.Add(slot);
        }
    }

    private VisualElement CreateSlotElement(int itemId, int count, int enchant)
    {
        VisualElement slot = new VisualElement();
        slot.AddToClassList("trade-slot-elem");

        VisualElement icon = new VisualElement();
        icon.AddToClassList("trade-slot-icon");
        Texture2D iconTex = IconTable.Instance.GetIcon(itemId);
        if (iconTex != null)
        {
            icon.style.backgroundImage = new StyleBackground(iconTex);
        }
        slot.Add(icon);

        if (count > 1)
        {
            Label countLabel = new Label(FormatCount(count));
            countLabel.AddToClassList("trade-slot-count");
            slot.Add(countLabel);
        }

        string itemName = ItemNameTable.Instance.GetItemName(itemId)?.Name ?? $"Item #{itemId}";
        if (enchant > 0)
        {
            itemName = $"+{enchant} {itemName}";
        }

        slot.RegisterCallback<PointerEnterEvent>(evt =>
        {
            L2ToolTip.Instance?.UpdateTooltip(L2Slot.SlotType.Inventory, new ItemTooltipData(itemId, itemName), slot);
        });

        slot.RegisterCallback<PointerLeaveEvent>(evt =>
        {
            L2ToolTip.Instance?.HideWindow(false);
        });

        return slot;
    }

    private string FormatCount(int count)
    {
        if (count >= 1000000) return (count / 1000000f).ToString("0.#") + "M";
        if (count >= 1000) return (count / 1000f).ToString("0.#") + "K";
        return count.ToString();
    }

    private void OnItemClicked(ItemInstance item)
    {
        if (item.Count > 1)
        {
            L2InputAmountWindow.Instance?.ShowWindow("Amount to trade:", item.Count, (amount) =>
            {
                SendAddItem(item.ObjectId, amount);
            }, null);
        }
        else
        {
            SendAddItem(item.ObjectId, 1);
        }
    }

    private void SendAddItem(int objectId, int count)
    {
        GameClient.Instance?.ClientPacketHandler?.SendPacket(new AddTradeItemPacket(0, objectId, count));
    }

    public void AddMyItem(int objectId, int itemId, int count, int enchant)
    {
        TradeItemData existing = _myTradeItems.Find(x => x.ObjectId == objectId);
        if (existing != null)
        {
            existing.Count += count;
        }
        else
        {
            _myTradeItems.Add(new TradeItemData(objectId, itemId, count, enchant));
        }
        RefreshMyTradeGrid();
    }

    public void AddPartnerItem(int objectId, int itemId, int count, int enchant)
    {
        TradeItemData existing = _partnerTradeItems.Find(x => x.ObjectId == objectId);
        if (existing != null)
        {
            existing.Count += count;
        }
        else
        {
            _partnerTradeItems.Add(new TradeItemData(objectId, itemId, count, enchant));
        }
        RefreshPartnerTradeGrid();
    }

    public void ConfirmOwn()
    {
        _myConfirmed = true;
        UpdateStatusLabels();
    }

    public void ConfirmOther()
    {
        _partnerConfirmed = true;
        UpdateStatusLabels();
    }

    public void OnTradeDone(int status)
    {
        HideWindow(false);
    }

    private void OnClickOk()
    {
        if (_myConfirmed) return;
        _myConfirmed = true;
        UpdateStatusLabels();
        GameClient.Instance?.ClientPacketHandler?.SendPacket(new TradeDonePacket(1));
    }

    private void OnClickCancel()
    {
        GameClient.Instance?.ClientPacketHandler?.SendPacket(new TradeDonePacket(0));
        HideWindow(false);
    }

    public override void HideWindow(bool silent)
    {
        base.HideWindow(silent);
    }
}
