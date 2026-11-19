using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestWindow : L2PopupWindow
{
    public enum QuestCategory
    {
        Once,
        Repeat,
        Epic,
        Transfer,
        Special
    }

    public class QuestItemInfo
    {
        public int QuestId;
        public string Name;
        public QuestCategory Category;
        public string Description;
        public bool IsCompleted;

        public QuestItemInfo(int id, string name, QuestCategory category, string description = "")
        {
            QuestId = id;
            Name = name;
            Category = category;
            Description = description;
            IsCompleted = false;
        }
    }

    private static QuestWindow _instance;
    public static QuestWindow Instance => _instance;

    private readonly List<QuestItemInfo> _quests = new();
    private QuestCategory _currentCategory = QuestCategory.Once;

    private Button _tabOnce;
    private Button _tabRepeat;
    private Button _tabEpic;
    private Button _tabTransfer;
    private Button _tabSpecial;

    private Label _questCountLabel;
    private Label _emptyQuestsLabel;
    private ScrollView _questScrollView;
    private VisualElement _questListItemsContainer;

    private VisualElement _autoNotifyOption;
    private VisualElement _autoNotifyCheck;
    private VisualElement _trackingOption;
    private VisualElement _trackingCheck;
    private Button _detailedInfoButton;

    private bool _autoNotifyEnabled = false;
    private bool _trackingEnabled = true;
    private QuestItemInfo _selectedQuest;

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
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/QuestWindow/QuestWindow");
    }

    protected override void InitWindow(VisualElement root)
    {
        base.InitWindow(root);

        StyleSheet uss = Resources.Load<StyleSheet>("Data/UI/_Elements/Game/QuestWindow/QuestWindow");
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
            windowName.text = "Quest";
        }
    }

    protected override IEnumerator BuildWindow(VisualElement root)
    {
        InitWindow(root);

        yield return new WaitForEndOfFrame();

        _tabOnce = GetElementById("TabOnce") as Button;
        _tabRepeat = GetElementById("TabRepeat") as Button;
        _tabEpic = GetElementById("TabEpic") as Button;
        _tabTransfer = GetElementById("TabTransfer") as Button;
        _tabSpecial = GetElementById("TabSpecial") as Button;

        _questCountLabel = GetLabelById("QuestCountLabel");
        _emptyQuestsLabel = GetLabelById("EmptyQuestsLabel");
        _questScrollView = GetElementById("QuestScrollView") as ScrollView;
        _questListItemsContainer = GetElementById("QuestListItemsContainer");

        _autoNotifyOption = GetElementById("AutoNotifyOption");
        _autoNotifyCheck = GetElementById("AutoNotifyCheck");
        _trackingOption = GetElementById("TrackingOption");
        _trackingCheck = GetElementById("TrackingCheck");
        _detailedInfoButton = GetElementById("DetailedInfoButton") as Button;

        SetupTabs();
        SetupCheckboxes();
        SetupButtons();

        RefreshQuestView();
        HideWindow(true);
    }

    private void SetupTabs()
    {
        if (_tabOnce != null)
        {
            _tabOnce.clicked += () => SelectCategory(QuestCategory.Once);
        }
        if (_tabRepeat != null)
        {
            _tabRepeat.clicked += () => SelectCategory(QuestCategory.Repeat);
        }
        if (_tabEpic != null)
        {
            _tabEpic.clicked += () => SelectCategory(QuestCategory.Epic);
        }
        if (_tabTransfer != null)
        {
            _tabTransfer.clicked += () => SelectCategory(QuestCategory.Transfer);
        }
        if (_tabSpecial != null)
        {
            _tabSpecial.clicked += () => SelectCategory(QuestCategory.Special);
        }
    }

    private void SetupCheckboxes()
    {
        if (_autoNotifyOption != null)
        {
            _autoNotifyOption.RegisterCallback<ClickEvent>(evt =>
            {
                _autoNotifyEnabled = !_autoNotifyEnabled;
                if (_autoNotifyCheck != null)
                {
                    if (_autoNotifyEnabled)
                    {
                        _autoNotifyCheck.AddToClassList("checked");
                    }
                    else
                    {
                        _autoNotifyCheck.RemoveFromClassList("checked");
                    }
                }
                AudioManager.Instance?.PlayUISound("click");
            });
        }

        if (_trackingOption != null)
        {
            _trackingOption.RegisterCallback<ClickEvent>(evt =>
            {
                _trackingEnabled = !_trackingEnabled;
                if (_trackingCheck != null)
                {
                    if (_trackingEnabled)
                    {
                        _trackingCheck.AddToClassList("checked");
                    }
                    else
                    {
                        _trackingCheck.RemoveFromClassList("checked");
                    }
                }
                AudioManager.Instance?.PlayUISound("click");
            });
        }
    }

    private void SetupButtons()
    {
        if (_detailedInfoButton != null)
        {
            _detailedInfoButton.clicked += () =>
            {
                AudioManager.Instance?.PlayUISound("click");
                if (_selectedQuest != null && !string.IsNullOrEmpty(_selectedQuest.Description))
                {
                    Debug.Log($"[QuestWindow] Detailed info for quest: {_selectedQuest.Name}: {_selectedQuest.Description}");
                }
            };
        }
    }

    public void SelectCategory(QuestCategory category)
    {
        _currentCategory = category;
        AudioManager.Instance?.PlayUISound("tab");

        UpdateTabVisuals();
        RefreshQuestView();
    }

    private void UpdateTabVisuals()
    {
        _tabOnce?.RemoveFromClassList("active");
        _tabRepeat?.RemoveFromClassList("active");
        _tabEpic?.RemoveFromClassList("active");
        _tabTransfer?.RemoveFromClassList("active");
        _tabSpecial?.RemoveFromClassList("active");

        switch (_currentCategory)
        {
            case QuestCategory.Once:
                _tabOnce?.AddToClassList("active");
                break;
            case QuestCategory.Repeat:
                _tabRepeat?.AddToClassList("active");
                break;
            case QuestCategory.Epic:
                _tabEpic?.AddToClassList("active");
                break;
            case QuestCategory.Transfer:
                _tabTransfer?.AddToClassList("active");
                break;
            case QuestCategory.Special:
                _tabSpecial?.AddToClassList("active");
                break;
        }
    }

    public void SetQuests(List<QuestItemInfo> quests)
    {
        _quests.Clear();
        if (quests != null)
        {
            _quests.AddRange(quests);
        }
        RefreshQuestView();
    }

    public void AddQuest(QuestItemInfo quest)
    {
        if (quest == null) return;
        _quests.RemoveAll(q => q.QuestId == quest.QuestId);
        _quests.Add(quest);
        RefreshQuestView();
    }

    public void RemoveQuest(int questId)
    {
        _quests.RemoveAll(q => q.QuestId == questId);
        if (_selectedQuest != null && _selectedQuest.QuestId == questId)
        {
            _selectedQuest = null;
        }
        RefreshQuestView();
    }

    public void RefreshQuestView()
    {
        if (_questCountLabel != null)
        {
            int activeCount = _quests.Count;
            _questCountLabel.text = $"({activeCount}/40)";
        }

        if (_questListItemsContainer == null) return;

        _questListItemsContainer.Clear();

        List<QuestItemInfo> filtered = _quests.FindAll(q => q.Category == _currentCategory);

        if (filtered.Count == 0)
        {
            if (_emptyQuestsLabel != null)
            {
                _emptyQuestsLabel.style.display = DisplayStyle.Flex;
                _emptyQuestsLabel.text = "No active quests";
            }
        }
        else
        {
            if (_emptyQuestsLabel != null)
            {
                _emptyQuestsLabel.style.display = DisplayStyle.None;
            }

            foreach (var q in filtered)
            {
                VisualElement itemRow = new VisualElement();
                itemRow.AddToClassList("quest-list-item");
                if (_selectedQuest == q)
                {
                    itemRow.AddToClassList("selected");
                }

                Label titleLabel = new Label(q.Name);
                titleLabel.AddToClassList("quest-item-title");
                itemRow.Add(titleLabel);

                var capturedQuest = q;
                itemRow.RegisterCallback<ClickEvent>(evt =>
                {
                    _selectedQuest = capturedQuest;
                    RefreshQuestView();
                    AudioManager.Instance?.PlayUISound("click");
                });

                _questListItemsContainer.Add(itemRow);
            }
        }
    }
}
