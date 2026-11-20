using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ClanWindow : L2PopupWindow
{
    private static ClanWindow _instance;
    public static ClanWindow Instance => _instance;

    private Label _clanNameLabel;
    private Label _clanLevelLabel;
    private Label _clanLeaderLabel;
    private Label _clanMembersCountLabel;
    private VisualElement _membersContainer;
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
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/ClanWindow/ClanWindow");
    }

    protected override void InitWindow(VisualElement root)
    {
        base.InitWindow(root);

        StyleSheet style = Resources.Load<StyleSheet>("Data/UI/_Elements/Game/ClanWindow/ClanWindow");
        if (style != null && !_windowEle.styleSheets.Contains(style))
        {
            _windowEle.styleSheets.Add(style);
        }

        var windowName = (Label)GetElementById("windows-name-label");
        if (windowName != null)
        {
            windowName.text = "Clan";
        }

        var dragArea = GetElementByClass("drag-area");
        if (dragArea != null)
        {
            DragManipulator drag = new DragManipulator(dragArea, _windowEle, this);
            dragArea.AddManipulator(drag);
            RegisterClickWindowEvent(_windowEle, dragArea);
        }

        RegisterCloseWindowEvent("btn-close-frame");

        _clanNameLabel = _windowEle.Q<Label>("ClanNameLabel");
        _clanLevelLabel = _windowEle.Q<Label>("ClanLevelLabel");
        _clanLeaderLabel = _windowEle.Q<Label>("ClanLeaderLabel");
        _clanMembersCountLabel = _windowEle.Q<Label>("ClanMembersCountLabel");
        _membersContainer = _windowEle.Q<VisualElement>("ClanMembersContainer");
        _refreshBtn = _windowEle.Q<Button>("ClanRefreshBtn");
        _closeBtn = _windowEle.Q<Button>("ClanCloseBtn");

        if (_refreshBtn != null)
        {
            _refreshBtn.AddManipulator(new ButtonClickSoundManipulator(_refreshBtn));
            _refreshBtn.RegisterCallback<ClickEvent>(evt => RequestPledgeList());
        }

        if (_closeBtn != null)
        {
            _closeBtn.AddManipulator(new ButtonClickSoundManipulator(_closeBtn));
            _closeBtn.RegisterCallback<ClickEvent>(evt => HideWindow(false));
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

    public void ToggleClanWindow()
    {
        if (IsVisible())
        {
            HideWindow(false);
        }
        else
        {
            ShowWindow(false);
            RequestPledgeList();
        }
    }

    public void RequestPledgeList()
    {
        GameClient.Instance.SendMessage(new RequestPledgeMemberListPacket());
    }

    public void UpdateClanData(PledgeShowMemberListAllPacket packet)
    {
        if (!IsVisible())
        {
            ShowWindow(false);
        }

        if (_clanNameLabel != null)
        {
            _clanNameLabel.text = string.IsNullOrEmpty(packet.PledgeName) ? "Clan: None" : $"Clan: {packet.PledgeName}";
        }

        if (_clanLevelLabel != null)
        {
            _clanLevelLabel.text = $"Level: {packet.ClanLevel}";
        }

        if (_clanLeaderLabel != null)
        {
            _clanLeaderLabel.text = string.IsNullOrEmpty(packet.LeaderName) ? "Leader: None" : $"Leader: {packet.LeaderName}";
        }

        if (_clanMembersCountLabel != null)
        {
            _clanMembersCountLabel.text = $"Members: {packet.Members.Count}";
        }

        if (_membersContainer == null) return;
        _membersContainer.Clear();

        for (int i = 0; i < packet.Members.Count; i++)
        {
            var m = packet.Members[i];
            VisualElement row = new VisualElement();
            row.AddToClassList("clan-member-row");
            if (i % 2 == 1)
            {
                row.AddToClassList("clan-member-row-alt");
            }

            Label nameLbl = new Label(m.Name);
            nameLbl.AddToClassList("clan-cell-text");
            nameLbl.AddToClassList("clan-col-name");
            row.Add(nameLbl);

            Label lvLbl = new Label(m.Level.ToString());
            lvLbl.AddToClassList("clan-cell-text");
            lvLbl.AddToClassList("clan-col-lv");
            row.Add(lvLbl);

            string className = Enum.IsDefined(typeof(CharacterClass), (byte)m.ClassId)
                ? ((CharacterClass)m.ClassId).ToString()
                : $"Class {m.ClassId}";
            Label classLbl = new Label(className);
            classLbl.AddToClassList("clan-cell-text");
            classLbl.AddToClassList("clan-col-class");
            row.Add(classLbl);

            Label statusLbl = new Label(m.IsOnline ? "Online" : "Offline");
            statusLbl.AddToClassList("clan-cell-text");
            statusLbl.AddToClassList("clan-col-status");
            statusLbl.AddToClassList(m.IsOnline ? "clan-status-online" : "clan-status-offline");
            row.Add(statusLbl);

            _membersContainer.Add(row);
        }
    }
}
