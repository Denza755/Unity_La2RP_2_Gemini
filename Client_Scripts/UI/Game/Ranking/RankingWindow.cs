using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RankingWindow : L2PopupWindow
{
    private static RankingWindow _instance;
    public static RankingWindow Instance => _instance;

    private int _currentCategory = 0; // 0=Level/Exp, 1=PvP, 2=PK, 3=Online

    private Button _tabLevel;
    private Button _tabPvp;
    private Button _tabPk;
    private Button _tabOnline;
    private Label _valHeaderLabel;
    private VisualElement _rowsContainer;
    private Button _refreshBtn;
    private Button _closeBtn;

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
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/RankingWindow/RankingWindow");
    }

    protected override void InitWindow(VisualElement root)
    {
        base.InitWindow(root);

        StyleSheet style = Resources.Load<StyleSheet>("Data/UI/_Elements/Game/RankingWindow/RankingWindow");
        if (style != null && !_windowEle.styleSheets.Contains(style))
        {
            _windowEle.styleSheets.Add(style);
        }

        var windowName = (Label)GetElementById("windows-name-label");
        if (windowName != null)
        {
            windowName.text = "Rankings";
        }

        var dragArea = GetElementByClass("drag-area");
        if (dragArea != null)
        {
            DragManipulator drag = new DragManipulator(dragArea, _windowEle, this);
            dragArea.AddManipulator(drag);
            RegisterClickWindowEvent(_windowEle, dragArea);
        }

        RegisterCloseWindowEvent("btn-close-frame");

        _tabLevel = _windowEle.Q<Button>("TabLevel");
        _tabPvp = _windowEle.Q<Button>("TabPvp");
        _tabPk = _windowEle.Q<Button>("TabPk");
        _tabOnline = _windowEle.Q<Button>("TabOnline");
        _valHeaderLabel = _windowEle.Q<Label>("ValueHeaderLabel");
        _rowsContainer = _windowEle.Q<VisualElement>("RankingRowsContainer");
        _refreshBtn = _windowEle.Q<Button>("RankingRefreshBtn");
        _closeBtn = _windowEle.Q<Button>("RankingCloseBtn");

        SetupTabButton(_tabLevel, 0);
        SetupTabButton(_tabPvp, 1);
        SetupTabButton(_tabPk, 2);
        SetupTabButton(_tabOnline, 3);

        if (_refreshBtn != null)
        {
            _refreshBtn.AddManipulator(new ButtonClickSoundManipulator(_refreshBtn));
            _refreshBtn.RegisterCallback<ClickEvent>(evt => RequestRanking(_currentCategory));
        }

        if (_closeBtn != null)
        {
            _closeBtn.AddManipulator(new ButtonClickSoundManipulator(_closeBtn));
            _closeBtn.RegisterCallback<ClickEvent>(evt => HideWindow(false));
        }
    }

    private void SetupTabButton(Button btn, int category)
    {
        if (btn == null) return;
        btn.AddManipulator(new ButtonClickSoundManipulator(btn));
        btn.RegisterCallback<ClickEvent>(evt => SwitchCategory(category));
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

    public void ToggleRankingWindow()
    {
        if (!_isWindowHidden)
        {
            HideWindow(false);
        }
        else
        {
            ShowWindow();
            SwitchCategory(_currentCategory);
        }
    }

    public void SwitchCategory(int category)
    {
        _currentCategory = category;

        _tabLevel?.RemoveFromClassList("rk-tab-active");
        _tabPvp?.RemoveFromClassList("rk-tab-active");
        _tabPk?.RemoveFromClassList("rk-tab-active");
        _tabOnline?.RemoveFromClassList("rk-tab-active");

        switch (category)
        {
            case 0:
                _tabLevel?.AddToClassList("rk-tab-active");
                if (_valHeaderLabel != null) _valHeaderLabel.text = "Exp";
                break;
            case 1:
                _tabPvp?.AddToClassList("rk-tab-active");
                if (_valHeaderLabel != null) _valHeaderLabel.text = "PvP Kills";
                break;
            case 2:
                _tabPk?.AddToClassList("rk-tab-active");
                if (_valHeaderLabel != null) _valHeaderLabel.text = "PK Kills";
                break;
            case 3:
                _tabOnline?.AddToClassList("rk-tab-active");
                if (_valHeaderLabel != null) _valHeaderLabel.text = "Play Time";
                break;
        }

        RequestRanking(category);
    }

    public void RequestRanking(int category)
    {
        GameClient.Instance?.ClientPacketHandler?.SendPacket(new RequestRankingPacket(category));
    }

    public void UpdateRankingData(RankingListPacket packet)
    {
        if (_isWindowHidden)
        {
            ShowWindow();
        }

        if (_rowsContainer == null) return;
        _rowsContainer.Clear();

        for (int i = 0; i < packet.Entries.Count; i++)
        {
            var entry = packet.Entries[i];
            VisualElement row = new VisualElement();
            row.AddToClassList("rk-row");
            if (i % 2 == 1)
            {
                row.AddToClassList("rk-row-alt");
            }

            // Rank
            Label rankLbl = new Label(entry.Rank.ToString());
            rankLbl.AddToClassList("rk-cell-text");
            rankLbl.AddToClassList("rk-col-rank");
            if (entry.Rank == 1) rankLbl.AddToClassList("rk-rank-gold");
            else if (entry.Rank == 2) rankLbl.AddToClassList("rk-rank-silver");
            else if (entry.Rank == 3) rankLbl.AddToClassList("rk-rank-bronze");
            row.Add(rankLbl);

            // Name + Online Dot
            VisualElement nameBox = new VisualElement();
            nameBox.AddToClassList("rk-name-box");
            nameBox.AddToClassList("rk-col-name");

            VisualElement dot = new VisualElement();
            dot.AddToClassList(entry.IsOnline ? "rk-online-dot" : "rk-offline-dot");
            nameBox.Add(dot);

            Label nameLbl = new Label(entry.Name);
            nameLbl.AddToClassList("rk-cell-text");
            nameBox.Add(nameLbl);
            row.Add(nameBox);

            // Class
            string className = Enum.IsDefined(typeof(CharacterClass), (byte)entry.ClassId)
                ? ((CharacterClass)entry.ClassId).ToString()
                : $"Class {entry.ClassId}";
            Label classLbl = new Label(className);
            classLbl.AddToClassList("rk-cell-text");
            classLbl.AddToClassList("rk-col-class");
            row.Add(classLbl);

            // Level
            Label lvLbl = new Label(entry.Level.ToString());
            lvLbl.AddToClassList("rk-cell-text");
            lvLbl.AddToClassList("rk-col-lv");
            row.Add(lvLbl);

            // Value
            string valText;
            if (packet.Category == 3)
            {
                // Seconds -> Days/Hours/Minutes
                long totalSec = entry.Value;
                long days = totalSec / 86400;
                long hours = (totalSec % 86400) / 3600;
                long mins = (totalSec % 3600) / 60;
                if (days > 0) valText = $"{days}d {hours}h";
                else valText = $"{hours}h {mins}m";
            }
            else
            {
                valText = entry.Value.ToString("N0");
            }

            Label valLbl = new Label(valText);
            valLbl.AddToClassList("rk-cell-text");
            valLbl.AddToClassList("rk-col-val");
            row.Add(valLbl);

            _rowsContainer.Add(row);
        }
    }
}
