using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ViewerHud : MonoBehaviour
{
    public ViewerEffectPlayer Player;
    public Rect HudRect = new Rect(12, 12, 460, 330);

    string _filter = "e_u511_fail";
    Vector2 _scroll;
    string _selectedName = "";
    readonly List<string> _filtered = new List<string>();
    string _statusText = "";

    void OnEnable()
    {
        ViewerStatus.Instance.Changed += RefreshStatus;
        RefreshStatus();
    }

    void OnDisable()
    {
        ViewerStatus.Instance.Changed -= RefreshStatus;
    }

    void RefreshStatus()
    {
        var s = ViewerStatus.Instance;
        _statusText = s.Stage + (string.IsNullOrEmpty(s.Detail) ? "" : " — " + s.Detail);
    }

    public bool ContainsScreenPoint(Vector2 screenPoint)
    {
        Vector2 gui = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
        return HudRect.Contains(gui);
    }

    public bool ContainsMouse()
    {
        Vector2 pos = Input.mousePosition;
        return ContainsScreenPoint(pos);
    }

    void OnGUI()
    {
        GUI.depth = 0;
        GUILayout.BeginArea(HudRect, GUI.skin.window);
        GUILayout.Label("Viewer Effects");
        GUILayout.Label("Статус: " + _statusText);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Поиск", GUILayout.Width(50));
        string next = GUILayout.TextField(_filter ?? "", GUILayout.MinWidth(200));
        GUILayout.EndHorizontal();
        if (next != _filter)
        {
            _filter = next;
            RebuildFilter();
        }

        if (_filtered.Count == 0 && UcEffectCatalog.Names.Count > 0)
            RebuildFilter();

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(170));
        int shown = Mathf.Min(_filtered.Count, 80);
        for (int i = 0; i < shown; i++)
        {
            bool on = _selectedName == _filtered[i];
            if (GUILayout.Toggle(on, _filtered[i], "Button") && !on && !ViewerStatus.Instance.Busy)
            {
                _selectedName = _filtered[i];
                Select(_selectedName);
            }
        }
        if (_filtered.Count > shown)
            GUILayout.Label("… ещё " + (_filtered.Count - shown));
        GUILayout.EndScrollView();
        GUILayout.Label("Эффектов: " + UcEffectCatalog.Names.Count + "  показано: " + _filtered.Count);
        GUILayout.Label("ПКМ drag — поворот, колёсико — ближе/дальше, ЛКМ — Play.");
        if (Player != null && Player.IsReady && GUILayout.Button("Проиграть в центре"))
            Player.PlayAt(Vector3.zero);
        GUILayout.EndArea();
    }

    void RebuildFilter()
    {
        string keep = _selectedName;
        _filtered.Clear();
        string f = _filter?.Trim() ?? "";
        foreach (string name in UcEffectCatalog.Names)
        {
            if (f.Length == 0 || name.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0)
                _filtered.Add(name);
        }
        bool still = false;
        for (int i = 0; i < _filtered.Count; i++)
        {
            if (_filtered[i] == keep)
            {
                still = true;
                break;
            }
        }
        _selectedName = still ? keep : "";
    }

    public void Select(string name)
    {
        if (Player == null || ViewerStatus.Instance.Busy)
            return;
        _selectedName = name ?? "";
        if (!string.IsNullOrEmpty(_selectedName) &&
            (_filter == null || _selectedName.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) < 0))
            _filter = _selectedName;
        RebuildFilter();
        Player.StopAllCoroutines();
        Player.StartCoroutine(Player.Prepare(_selectedName));
    }
}
