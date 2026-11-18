using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public abstract class AbstractEffectWindow : L2Window
{
    protected VisualTreeAsset _buffSlot;
    protected VisualElement[] _buffRowContainers;
    protected EffectType _effectType;
    private List<BuffSlot> _effectSlots;
    private List<BuffSlot> _specialEffectSlots;
    private Coroutine _effectCoroutine;

    protected enum EffectType
    {
        Buff, Debuff
    }

    protected void Init(VisualElement root)
    {
        InitWindow(root);
        _windowEle.style.left = 182;

        VisualElement dragArea = GetElementByClass("drag-area");
        if (dragArea != null)
        {
            DragManipulator drag = new DragManipulator(dragArea, _windowEle, this);
            dragArea.AddManipulator(drag);
        }

        MouseOverDetectionManipulator mouseOverDetectionManipulator = new MouseOverDetectionManipulator(_windowEle);
        _windowEle.AddManipulator(mouseOverDetectionManipulator);

        _effectSlots = new List<BuffSlot>();
        _specialEffectSlots = new List<BuffSlot>();

        _effectCoroutine = StartCoroutine(UpdateBuffTimers());
    }

    protected void HideWindowIfNeeded()
    {
        if ((_effectSlots == null || _effectSlots.Count == 0) &&
            (_specialEffectSlots == null || _specialEffectSlots.Count == 0))
        {
            if (!_isWindowHidden)
            {
                HideWindow(true);
            }
        }
    }

    private void CleanupAllNormalEffects()
    {
        if (_buffRowContainers != null && _buffRowContainers.Length > 0 && _buffRowContainers[0] != null)
        {
            _buffRowContainers[0].Clear();
        }
        _effectSlots?.Clear();
    }

    private void CleanupAllSpecialEffects()
    {
        if (_buffRowContainers != null && _buffRowContainers.Length > 1 && _buffRowContainers[1] != null)
        {
            _buffRowContainers[1].Clear();
        }
        _specialEffectSlots?.Clear();
    }

    private void CleanupAllToggleEffects()
    {
        if (_buffRowContainers != null && _buffRowContainers.Length > 2 && _buffRowContainers[2] != null)
        {
            _buffRowContainers[2].Clear();
        }
    }

    public void CleanupAllEffects()
    {
        CleanupAllNormalEffects();
        CleanupAllSpecialEffects();
        CleanupAllToggleEffects();
    }

    public void SetEffects(BuffEffect[] buffEffects)
    {
        CleanupAllEffects();

        if (buffEffects == null || buffEffects.Length == 0)
        {
            HideWindowIfNeeded();
            return;
        }

        foreach (BuffEffect buffEffect in buffEffects)
        {
            if (buffEffect == null) continue;

            Skillgrp skillgrp = SkillgrpTable.Instance.GetSkill(buffEffect.SkillId, buffEffect.SkillLvl);
            if (skillgrp == null) continue;

            bool isDebuff = skillgrp.Debuff == 1 || skillgrp.IconType == SkillType.Debuff;

            if (_effectType == EffectType.Buff && !isDebuff)
            {
                BuffType bType = Buff.ResolveBuffType(skillgrp);
                AddEffect(buffEffect.SkillId, buffEffect.SkillLvl, buffEffect.Duration, bType);
            }
            else if (_effectType == EffectType.Debuff && isDebuff)
            {
                AddEffect(buffEffect.SkillId, buffEffect.SkillLvl, buffEffect.Duration, BuffType.Debuff);
            }
        }

        HideWindowIfNeeded();
    }

    public void AddEffect(int effectId, int effectLevel, int duration, BuffType type)
    {
        Debug.Log($"Received buff: {effectId}, level: {effectLevel}, duration: {duration}, type: {type}");

        int containerIndex = 0;
        List<BuffSlot> targetSlotList = _effectSlots;

        if (type == BuffType.Special)
        {
            containerIndex = 1;
            targetSlotList = _specialEffectSlots;
        }
        else if (type == BuffType.Toggle)
        {
            containerIndex = 2;
            targetSlotList = _effectSlots;
        }

        TemplateContainer template = _buffSlot.Instantiate();
        VisualElement slotVisual = template.Q<VisualElement>("BgContainer") ?? (template.childCount > 0 ? template[0] : template);

        BuffSlot buffSlot = new BuffSlot(slotVisual);

        if (_buffRowContainers != null && containerIndex < _buffRowContainers.Length && _buffRowContainers[containerIndex] != null)
        {
            _buffRowContainers[containerIndex].Add(buffSlot.SlotElement);
        }

        targetSlotList.Add(buffSlot);
        buffSlot.AssignEffect(effectId, effectLevel, duration, type);

        if (_isWindowHidden)
        {
            ShowWindow();
        }
    }

    private IEnumerator UpdateBuffTimers()
    {
        while (true)
        {
            if (_isWindowHidden)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            bool removedAny = false;

            // Tick & expire standard/toggle effects
            if (_effectSlots != null)
            {
                for (int i = _effectSlots.Count - 1; i >= 0; i--)
                {
                    BuffSlot slot = _effectSlots[i];
                    if (slot == null || slot.Buff == null)
                    {
                        _effectSlots.RemoveAt(i);
                        removedAny = true;
                        continue;
                    }

                    if (slot.Buff.Duration > 0)
                    {
                        float remaining = slot.GetRemainingDuration();
                        if (remaining <= 0f)
                        {
                            if (slot.SlotElement != null && slot.SlotElement.parent != null)
                            {
                                slot.SlotElement.parent.Remove(slot.SlotElement);
                            }
                            _effectSlots.RemoveAt(i);
                            removedAny = true;
                            continue;
                        }
                    }

                    slot.UpdateRemainingDuration();
                }
            }

            // Tick & expire special effects
            if (_specialEffectSlots != null)
            {
                for (int i = _specialEffectSlots.Count - 1; i >= 0; i--)
                {
                    BuffSlot slot = _specialEffectSlots[i];
                    if (slot == null || slot.Buff == null)
                    {
                        _specialEffectSlots.RemoveAt(i);
                        removedAny = true;
                        continue;
                    }

                    if (slot.Buff.Duration > 0)
                    {
                        float remaining = slot.GetRemainingDuration();
                        if (remaining <= 0f)
                        {
                            if (slot.SlotElement != null && slot.SlotElement.parent != null)
                            {
                                slot.SlotElement.parent.Remove(slot.SlotElement);
                            }
                            _specialEffectSlots.RemoveAt(i);
                            removedAny = true;
                            continue;
                        }
                    }

                    slot.UpdateRemainingDuration();
                }
            }

            if (removedAny)
            {
                HideWindowIfNeeded();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void UpdateBuffs()
    {
        if (_effectSlots != null)
        {
            foreach (var slot in _effectSlots)
            {
                slot?.UpdateRemainingDuration();
            }
        }
        if (_specialEffectSlots != null)
        {
            foreach (var slot in _specialEffectSlots)
            {
                slot?.UpdateRemainingDuration();
            }
        }
        HideWindowIfNeeded();
    }

    private void OnDestroy()
    {
        if (_effectCoroutine != null)
        {
            StopCoroutine(_effectCoroutine);
        }
    }

    public virtual void SetEtcEffects(PlayerBuffStatus status)
    {
        CleanupAllSpecialEffects();
    }
}
