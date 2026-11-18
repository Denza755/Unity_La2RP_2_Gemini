using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

public class BuffSlot : L2Slot
{
    public Buff Buff { get; private set; }
    private Label _durationLabel;

    public BuffSlot(VisualElement slotElement) : base(slotElement, 0, SlotType.Effect)
    {
        _durationLabel = slotElement != null ? slotElement.Q<Label>("Duration") : null;
    }

    public void AssignEffect(int effectId, int effectLevel, int duration, BuffType buffType)
    {
        Id = effectId;
        Skillgrp skillgrp = SkillgrpTable.Instance.GetSkill(effectId, effectLevel);
        SkillNameData skillNameData = SkillNameTable.Instance.GetName(effectId, effectLevel);

        string skillName = skillNameData != null ? skillNameData.Name : $"Skill {effectId}";
        string desc = skillNameData != null ? (skillNameData.Desc ?? "") : "";
        string[] descParams = skillNameData != null ? skillNameData.DescParams : null;
        string description = ComposeDescription(desc, descParams);
        string icon = skillgrp != null ? skillgrp.Icon : "";

        Buff = new Buff(skillName, description, icon, effectLevel, duration, buffType, Time.unscaledTime);

        if (_slotBg != null && !string.IsNullOrEmpty(icon))
        {
            StyleBackground background = new StyleBackground(IconTable.Instance.LoadTextureByName(icon));
            _slotBg.style.backgroundImage = background;
        }

        AddTooltip();
    }

    private string ComposeDescription(string desc, string[] descParams)
    {
        if (string.IsNullOrEmpty(desc) || descParams == null)
        {
            return desc ?? "";
        }

        for (var i = 0; i < descParams.Length; i++)
        {
            desc = desc.Replace($"$s{i + 1}", descParams[i]);
        }
        return desc;
    }

    public void UpdateRemainingDuration()
    {
        if (Buff == null || Buff.Duration <= 0)
        {
            HideRemainingDuration();
            return;
        }

        float remainingDuration = GetRemainingDuration();

        if (remainingDuration <= 0f)
        {
            HideRemainingDuration();
            return;
        }

        if (remainingDuration < 60f)
        {
            TogglePulse();
            ShowRemainingDuration(remainingDuration);
        }
        else
        {
            HideRemainingDuration();
        }
    }

    public float GetRemainingDuration()
    {
        if (Buff == null) return 0f;
        return Buff.StartTime + Buff.Duration - Time.unscaledTime;
    }

    private void AddTooltip()
    {
        if (Buff != null)
        {
            _tooltipManipulator?.SetValue(Buff);
        }
    }

    private void TogglePulse()
    {
        if (_slotElement == null) return;

        if (_slotElement.ClassListContains("fade-low"))
        {
            _slotElement.RemoveFromClassList("fade-low");
            _slotElement.AddToClassList("fade-high");
        }
        else
        {
            _slotElement.RemoveFromClassList("fade-high");
            _slotElement.AddToClassList("fade-low");
        }
    }

    private void ShowRemainingDuration(float duration)
    {
        if (_durationLabel == null) return;
        if (duration < 0f) duration = 0f;
        _durationLabel.text = Mathf.Ceil(duration).ToString(CultureInfo.InvariantCulture);
        if (_durationLabel.style.display != DisplayStyle.Flex) _durationLabel.style.display = DisplayStyle.Flex;
    }

    private void HideRemainingDuration()
    {
        if (_durationLabel != null && _durationLabel.style.display != DisplayStyle.None)
        {
            _durationLabel.style.display = DisplayStyle.None;
        }
    }
}
