using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PrivateStoreWindow : L2PopupWindow
{
    private static PrivateStoreWindow _instance;
    public static PrivateStoreWindow Instance => _instance;

    public enum StoreMode
    {
        BuyFromStore, // Interacting with a player/bot selling items
        SellToStore   // Interacting with a player/bot buying items
    }

    public class CartEntry
    {
        public int ObjectId;
        public int ItemId;
        public int Count;
        public int Price;
        public short Enchant;
    }

    private StoreMode _currentMode = StoreMode.BuyFromStore;
    private int _storePlayerId;
    private int _playerAdena;

    private Label _titleLabel;
    private Label _leftHeader;
    private Label _rightHeader;
    private VisualElement _availableGrid;
    private VisualElement _selectedGrid;
    private Label _adenaLabel;
    private Label _totalLabel;
    private Button _actionBtn;
    private Button _cancelBtn;

    // Available items in left panel
    private List<PrivateStoreListSellPacket.StoreItem> _sellStoreItems = new();
    private List<PrivateStoreListBuyPacket.BuyItem> _buyStoreItems = new();

    // Selected items in cart (right panel)
    private readonly List<CartEntry> _cartItems = new();

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
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/PrivateStoreWindow/PrivateStoreWindow");
    }

    protected override void InitWindow(VisualElement root)
    {
        base.InitWindow(root);

        StyleSheet style = Resources.Load<StyleSheet>("Data/UI/_Elements/Game/PrivateStoreWindow/PrivateStoreWindow");
        if (style != null && !_windowEle.styleSheets.Contains(style))
        {
            _windowEle.styleSheets.Add(style);
        }

        var windowName = (Label)GetElementById("windows-name-label");
        if (windowName != null)
        {
            windowName.text = "Private Store";
        }

        var dragArea = GetElementByClass("drag-area");
        if (dragArea != null)
        {
            DragManipulator drag = new DragManipulator(dragArea, _windowEle, this);
            dragArea.AddManipulator(drag);
            RegisterClickWindowEvent(_windowEle, dragArea);
        }

        RegisterCloseWindowEvent("btn-close-frame");

        _titleLabel = _windowEle.Q<Label>("StoreTitleLabel");
        _leftHeader = _windowEle.Q<Label>("StoreLeftHeader");
        _rightHeader = _windowEle.Q<Label>("StoreRightHeader");
        _availableGrid = _windowEle.Q<VisualElement>("StoreAvailableGrid");
        _selectedGrid = _windowEle.Q<VisualElement>("StoreSelectedGrid");
        _adenaLabel = _windowEle.Q<Label>("StoreAdenaLabel");
        _totalLabel = _windowEle.Q<Label>("StoreTotalLabel");
        _actionBtn = _windowEle.Q<Button>("StoreActionBtn");
        _cancelBtn = _windowEle.Q<Button>("StoreCancelBtn");

        if (_actionBtn != null)
        {
            _actionBtn.AddManipulator(new ButtonClickSoundManipulator(_actionBtn));
            _actionBtn.RegisterCallback<ClickEvent>(evt => OnActionClicked());
        }

        if (_cancelBtn != null)
        {
            _cancelBtn.AddManipulator(new ButtonClickSoundManipulator(_cancelBtn));
            _cancelBtn.RegisterCallback<ClickEvent>(evt => HideWindow(false));
        }
    }

    protected override IEnumerator BuildWindow(VisualElement root)
    {
        InitWindow(root);
        yield return new WaitForEndOfFrame();

        _windowEle.style.left = new Length(50, LengthUnit.Percent);
        _windowEle.style.top = new Length(50, LengthUnit.Percent);
        _windowEle.style.translate = new StyleTranslate(new Translate(new Length(-50, LengthUnit.Percent), new Length(-50, LengthUnit.Percent)));

        L2GameUI.Instance.WindowLoadComplete();
    }

    public void OpenSellStore(PrivateStoreListSellPacket packet)
    {
        _currentMode = StoreMode.BuyFromStore;
        _storePlayerId = packet.StorePlayerId;
        _playerAdena = packet.PlayerAdena;
        _sellStoreItems = packet.Items;
        _cartItems.Clear();

        ShowWindow(false);

        if (_titleLabel != null) _titleLabel.text = "Buy From Private Store";
        if (_leftHeader != null) _leftHeader.text = "Store Items (Click to Buy)";
        if (_rightHeader != null) _rightHeader.text = "Items to Purchase";
        if (_actionBtn != null) _actionBtn.text = "Buy";

        RefreshAvailableGrid();
        RefreshCartGrid();
    }

    public void OpenBuyStore(PrivateStoreListBuyPacket packet)
    {
        _currentMode = StoreMode.SellToStore;
        _storePlayerId = packet.StorePlayerId;
        _playerAdena = packet.PlayerAdena;
        _buyStoreItems = packet.Items;
        _cartItems.Clear();

        ShowWindow(false);

        if (_titleLabel != null) _titleLabel.text = "Sell to Private Store";
        if (_leftHeader != null) _leftHeader.text = "Buyer Wanted Items";
        if (_rightHeader != null) _rightHeader.text = "Items to Sell";
        if (_actionBtn != null) _actionBtn.text = "Sell";

        RefreshAvailableGrid();
        RefreshCartGrid();
    }

    private void RefreshAvailableGrid()
    {
        if (_availableGrid == null) return;
        _availableGrid.Clear();

        if (_currentMode == StoreMode.BuyFromStore)
        {
            foreach (var item in _sellStoreItems)
            {
                int remaining = item.Count;
                var inCart = _cartItems.Find(c => c.ObjectId == item.ObjectId);
                if (inCart != null) remaining -= inCart.Count;
                if (remaining <= 0) continue;

                VisualElement slot = CreateSlotElement(item.ItemId, item.Enchant, remaining, item.Price, () =>
                {
                    PromptAddToCart(item.ObjectId, item.ItemId, item.Enchant, remaining, item.Price);
                });
                _availableGrid.Add(slot);
            }
        }
        else
        {
            foreach (var item in _buyStoreItems)
            {
                int remaining = item.Count;
                var inCart = _cartItems.Find(c => c.ObjectId == item.ObjectId);
                if (inCart != null) remaining -= inCart.Count;
                if (remaining <= 0) continue;

                VisualElement slot = CreateSlotElement(item.ItemId, item.Enchant, remaining, item.Price, () =>
                {
                    PromptAddToCart(item.ObjectId, item.ItemId, (short)item.Enchant, remaining, item.Price);
                });
                _availableGrid.Add(slot);
            }
        }
    }

    private void RefreshCartGrid()
    {
        if (_selectedGrid == null) return;
        _selectedGrid.Clear();

        long totalCost = 0;

        foreach (var cart in _cartItems)
        {
            totalCost += (long)cart.Price * cart.Count;

            VisualElement slot = CreateSlotElement(cart.ItemId, cart.Enchant, cart.Count, cart.Price, () =>
            {
                _cartItems.Remove(cart);
                RefreshAvailableGrid();
                RefreshCartGrid();
            });
            _selectedGrid.Add(slot);
        }

        if (_adenaLabel != null)
        {
            _adenaLabel.text = $"Adena: {_playerAdena:N0}";
        }

        if (_totalLabel != null)
        {
            _totalLabel.text = $"Total: {totalCost:N0} Adena";
            if (_currentMode == StoreMode.BuyFromStore && totalCost > _playerAdena)
            {
                _totalLabel.style.color = new Color(1f, 0.3f, 0.3f, 1f);
            }
            else
            {
                _totalLabel.style.color = new Color(0.3f, 1f, 0.3f, 1f);
            }
        }
    }

    private VisualElement CreateSlotElement(int itemId, int enchant, int count, int price, Action onClick)
    {
        VisualElement slot = new VisualElement();
        slot.AddToClassList("ps-slot-elem");

        VisualElement icon = new VisualElement();
        icon.AddToClassList("ps-slot-icon");

        string iconPath = IconTable.Instance.GetIconPath(itemId);
        if (!string.IsNullOrEmpty(iconPath))
        {
            Sprite sp = Resources.Load<Sprite>(iconPath);
            if (sp != null)
            {
                icon.style.backgroundImage = new StyleBackground(sp);
            }
        }
        slot.Add(icon);

        if (count > 1 || enchant > 0)
        {
            Label countLabel = new Label();
            countLabel.AddToClassList("ps-slot-count");
            countLabel.text = enchant > 0 ? $"+{enchant}" : count.ToString();
            slot.Add(countLabel);
        }

        string itemName = ItemNameTable.Instance.GetItemName(itemId);
        slot.RegisterCallback<MouseEnterEvent>(evt =>
        {
            L2ToolTip.Instance?.ShowTooltip($"{itemName} (Price: {price:N0} Adena)\nCount: {count:N0}");
        });
        slot.RegisterCallback<MouseLeaveEvent>(evt =>
        {
            L2ToolTip.Instance?.HideTooltip();
        });

        slot.RegisterCallback<ClickEvent>(evt =>
        {
            onClick?.Invoke();
        });

        return slot;
    }

    private void PromptAddToCart(int objectId, int itemId, short enchant, int maxCount, int price)
    {
        if (maxCount <= 1)
        {
            AddCartItem(objectId, itemId, enchant, 1, price);
            return;
        }

        if (L2InputAmountWindow.Instance != null)
        {
            string itemName = ItemNameTable.Instance.GetItemName(itemId);
            L2InputAmountWindow.Instance.ShowWindow(itemName, maxCount, selectedCount =>
            {
                if (selectedCount > 0 && selectedCount <= maxCount)
                {
                    AddCartItem(objectId, itemId, enchant, selectedCount, price);
                }
            }, null);
        }
        else
        {
            AddCartItem(objectId, itemId, enchant, 1, price);
        }
    }

    private void AddCartItem(int objectId, int itemId, short enchant, int count, int price)
    {
        var existing = _cartItems.Find(c => c.ObjectId == objectId);
        if (existing != null)
        {
            existing.Count += count;
        }
        else
        {
            _cartItems.Add(new CartEntry
            {
                ObjectId = objectId,
                ItemId = itemId,
                Enchant = enchant,
                Count = count,
                Price = price
            });
        }

        RefreshAvailableGrid();
        RefreshCartGrid();
    }

    private void OnActionClicked()
    {
        if (_cartItems.Count == 0) return;

        if (_currentMode == StoreMode.BuyFromStore)
        {
            long totalCost = 0;
            var requests = new List<RequestPrivateStoreBuyPacket.RequestedItem>();
            foreach (var c in _cartItems)
            {
                totalCost += (long)c.Price * c.Count;
                requests.Add(new RequestPrivateStoreBuyPacket.RequestedItem(c.ObjectId, c.Count, c.Price));
            }

            if (totalCost > _playerAdena)
            {
                Debug.LogWarning("Not enough adena to buy from private store.");
                return;
            }

            GameClient.Instance.SendMessage(new RequestPrivateStoreBuyPacket(_storePlayerId, requests));
        }
        else
        {
            var requests = new List<RequestPrivateStoreSellPacket.RequestedItem>();
            foreach (var c in _cartItems)
            {
                requests.Add(new RequestPrivateStoreSellPacket.RequestedItem(c.ObjectId, c.ItemId, c.Enchant, c.Count, c.Price));
            }

            GameClient.Instance.SendMessage(new RequestPrivateStoreSellPacket(_storePlayerId, requests));
        }

        HideWindow(false);
    }
}
