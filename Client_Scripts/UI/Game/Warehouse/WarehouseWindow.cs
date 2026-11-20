using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WarehouseWindow : L2PopupWindow
{
    public enum Mode
    {
        Deposit,
        Withdraw
    }

    public class WhItemEntry
    {
        public int ObjectId;
        public int ItemId;
        public int Count;
        public int Enchant;

        public WhItemEntry(int objectId, int itemId, int count, int enchant)
        {
            ObjectId = objectId;
            ItemId = itemId;
            Count = count;
            Enchant = enchant;
        }
    }

    private static WarehouseWindow _instance;
    public static WarehouseWindow Instance => _instance;

    private Mode _currentMode = Mode.Deposit;
    private int _playerAdena = 0;

    private Label _whModeHeader;
    private Label _leftPanelLabel;
    private Label _whAdenaLabel;
    private Label _whFeeLabel;

    private VisualElement _whAvailableGrid;
    private VisualElement _whSelectedGrid;

    private Button _okBtn;
    private Button _cancelBtn;

    private readonly List<WhItemEntry> _availableItems = new();
    private readonly List<WhItemEntry> _selectedItems = new();

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
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/WarehouseWindow/WarehouseWindow");
    }

    protected override void InitWindow(VisualElement root)
    {
        base.InitWindow(root);

        StyleSheet uss = Resources.Load<StyleSheet>("Data/UI/_Elements/Game/WarehouseWindow/WarehouseWindow");
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
            windowName.text = "Warehouse";
        }

        _whModeHeader = GetLabelById("WhModeHeader");
        _leftPanelLabel = GetLabelById("LeftPanelLabel");
        _whAdenaLabel = GetLabelById("WhAdenaLabel");
        _whFeeLabel = GetLabelById("WhFeeLabel");

        _whAvailableGrid = GetElementById("WhAvailableGrid");
        _whSelectedGrid = GetElementById("WhSelectedGrid");

        _okBtn = _windowEle.Q<Button>("WhOkBtn");
        _cancelBtn = _windowEle.Q<Button>("WhCancelBtn");

        if (_okBtn != null)
        {
            _okBtn.clicked += OnClickOk;
        }

        if (_cancelBtn != null)
        {
            _cancelBtn.clicked += OnClickCancel;
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

    public void ShowDepositList(int playerAdena, ItemInstance[] items)
    {
        _currentMode = Mode.Deposit;
        _playerAdena = playerAdena;

        if (_whModeHeader != null) _whModeHeader.text = "Warehouse Deposit";
        if (_leftPanelLabel != null) _leftPanelLabel.text = "My Inventory";

        PopulateItems(items);
        ShowWindow();
    }

    public void ShowWithdrawList(int playerAdena, ItemInstance[] items)
    {
        _currentMode = Mode.Withdraw;
        _playerAdena = playerAdena;

        if (_whModeHeader != null) _whModeHeader.text = "Warehouse Withdraw";
        if (_leftPanelLabel != null) _leftPanelLabel.text = "Stored Items";

        PopulateItems(items);
        ShowWindow();
    }

    private void PopulateItems(ItemInstance[] items)
    {
        _availableItems.Clear();
        _selectedItems.Clear();

        if (items != null)
        {
            foreach (var item in items)
            {
                if (item != null)
                {
                    _availableItems.Add(new WhItemEntry(item.ObjectId, item.ItemId, item.Count, item.EnchantLevel));
                }
            }
        }

        UpdateInfoBar();
        RefreshAvailableGrid();
        RefreshSelectedGrid();
    }

    private void UpdateInfoBar()
    {
        if (_whAdenaLabel != null)
        {
            _whAdenaLabel.text = $"Adena: {_playerAdena:N0}";
        }

        if (_whFeeLabel != null)
        {
            if (_currentMode == Mode.Deposit)
            {
                int fee = _selectedItems.Count * 30;
                _whFeeLabel.text = $"Fee: {fee:N0} Adena (30 / slot)";
            }
            else
            {
                _whFeeLabel.text = "Fee: 0 Adena";
            }
        }
    }

    private void RefreshAvailableGrid()
    {
        if (_whAvailableGrid == null) return;
        _whAvailableGrid.Clear();

        foreach (var entry in _availableItems)
        {
            var cur = entry;
            VisualElement slot = CreateSlotElement(cur.ItemId, cur.Count, cur.Enchant);
            slot.RegisterCallback<ClickEvent>(evt =>
            {
                if (cur.Count > 1)
                {
                    L2InputAmountWindow.Instance?.ShowWindow("Amount to transfer:", cur.Count, (amount) =>
                    {
                        TransferToSelected(cur, amount);
                    }, null);
                }
                else
                {
                    TransferToSelected(cur, 1);
                }
            });
            _whAvailableGrid.Add(slot);
        }
    }

    private void RefreshSelectedGrid()
    {
        if (_whSelectedGrid == null) return;
        _whSelectedGrid.Clear();

        foreach (var entry in _selectedItems)
        {
            var cur = entry;
            VisualElement slot = CreateSlotElement(cur.ItemId, cur.Count, cur.Enchant);
            slot.RegisterCallback<ClickEvent>(evt =>
            {
                TransferBackToAvailable(cur);
            });
            _whSelectedGrid.Add(slot);
        }
    }

    private void TransferToSelected(WhItemEntry entry, int count)
    {
        if (count <= 0 || entry.Count < count) return;

        WhItemEntry existing = _selectedItems.Find(x => x.ObjectId == entry.ObjectId);
        if (existing != null)
        {
            existing.Count += count;
        }
        else
        {
            _selectedItems.Add(new WhItemEntry(entry.ObjectId, entry.ItemId, count, entry.Enchant));
        }

        entry.Count -= count;
        if (entry.Count <= 0)
        {
            _availableItems.Remove(entry);
        }

        UpdateInfoBar();
        RefreshAvailableGrid();
        RefreshSelectedGrid();
    }

    private void TransferBackToAvailable(WhItemEntry entry)
    {
        WhItemEntry existing = _availableItems.Find(x => x.ObjectId == entry.ObjectId);
        if (existing != null)
        {
            existing.Count += entry.Count;
        }
        else
        {
            _availableItems.Add(new WhItemEntry(entry.ObjectId, entry.ItemId, entry.Count, entry.Enchant));
        }

        _selectedItems.Remove(entry);

        UpdateInfoBar();
        RefreshAvailableGrid();
        RefreshSelectedGrid();
    }

    private VisualElement CreateSlotElement(int itemId, int count, int enchant)
    {
        VisualElement slot = new VisualElement();
        slot.AddToClassList("wh-slot-elem");

        VisualElement icon = new VisualElement();
        icon.AddToClassList("wh-slot-icon");
        Texture2D iconTex = IconTable.Instance.GetIcon(itemId);
        if (iconTex != null)
        {
            icon.style.backgroundImage = new StyleBackground(iconTex);
        }
        slot.Add(icon);

        if (count > 1)
        {
            Label countLabel = new Label(FormatCount(count));
            countLabel.AddToClassList("wh-slot-count");
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

    private void OnClickOk()
    {
        if (_selectedItems.Count == 0)
        {
            HideWindow(false);
            return;
        }

        List<(int objectId, int count)> itemsToSend = new();
        foreach (var item in _selectedItems)
        {
            itemsToSend.Add((item.ObjectId, item.Count));
        }

        if (_currentMode == Mode.Deposit)
        {
            GameClient.Instance?.ClientPacketHandler?.SendPacket(new SendWarehouseDepositListPacket(itemsToSend));
        }
        else
        {
            GameClient.Instance?.ClientPacketHandler?.SendPacket(new SendWarehouseWithdrawListPacket(itemsToSend));
        }

        HideWindow(false);
    }

    private void OnClickCancel()
    {
        HideWindow(false);
    }
}
