using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class DropListWindow : L2PopupWindow
{
    private static DropListWindow _instance;
    public static DropListWindow Instance => _instance;

    private Label _mobNameHeader;
    private VisualElement _dropItemsContainer;
    private Label _emptyDropLabel;

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
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/DropListWindow/DropListWindow");
    }

    protected override void InitWindow(VisualElement root)
    {
        base.InitWindow(root);

        StyleSheet uss = Resources.Load<StyleSheet>("Data/UI/_Elements/Game/DropListWindow/DropListWindow");
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
            windowName.text = "Drop List";
        }

        _mobNameHeader = GetLabelById("MobNameHeader");
        _dropItemsContainer = GetElementById("DropItemsContainer");
        _emptyDropLabel = GetLabelById("EmptyDropLabel");
    }

    public void SetDropList(string mobName, int[] itemIds)
    {
        if (_mobNameHeader != null)
        {
            _mobNameHeader.text = $"Drops: {mobName}";
        }

        if (_dropItemsContainer == null)
        {
            ShowWindow();
            return;
        }

        _dropItemsContainer.Clear();

        if (itemIds == null || itemIds.Length == 0)
        {
            if (_emptyDropLabel != null)
            {
                _emptyDropLabel.style.display = DisplayStyle.Flex;
            }
            ShowWindow();
            return;
        }

        if (_emptyDropLabel != null)
        {
            _emptyDropLabel.style.display = DisplayStyle.None;
        }

        foreach (int itemId in itemIds)
        {
            int currentItemId = itemId;
            VisualElement row = new VisualElement();
            row.AddToClassList("droplist-item-row");

            VisualElement icon = new VisualElement();
            icon.AddToClassList("droplist-item-icon");

            Texture2D iconTex = IconTable.Instance.GetIcon(currentItemId);
            if (iconTex != null)
            {
                icon.style.backgroundImage = new StyleBackground(iconTex);
            }

            string itemName = ItemNameTable.Instance.GetItemName(currentItemId)?.Name;
            if (string.IsNullOrEmpty(itemName))
            {
                itemName = $"Item #{currentItemId}";
            }

            Label nameLabel = new Label(itemName);
            nameLabel.AddToClassList("droplist-item-name");

            row.Add(icon);
            row.Add(nameLabel);

            row.RegisterCallback<PointerEnterEvent>(evt =>
            {
                L2ToolTip.Instance?.UpdateTooltip(L2Slot.SlotType.Item, new ItemTooltipData(currentItemId, itemName), row);
            });

            row.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                L2ToolTip.Instance?.HideWindow();
            });

            _dropItemsContainer.Add(row);
        }

        ShowWindow();
    }
}
