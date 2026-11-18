using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SkillgrpTable
{
    private static SkillgrpTable _instance;

    public static SkillgrpTable Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SkillgrpTable();
            }

            return _instance;
        }
    }

    private Dictionary<int, Dictionary<int, Skillgrp>> _skills;

    public void Initialize()
    {
        _skills = new Dictionary<int, Dictionary<int, Skillgrp>>();
        ReadActions();
    }

    public void ClearTable()
    {
        _skills.Clear();
        _skills = null;
        _instance = null;
    }

    public Skillgrp GetSkill(int id, int level)
    {
        _skills.TryGetValue(id, out Dictionary<int, Skillgrp> skillLevel);

        if (skillLevel == null) return null;

        skillLevel.TryGetValue(level, out Skillgrp skillgrp);

        return skillgrp;
    }

    public Dictionary<int, Skillgrp> GetSkillgrp(int id)
    {
        _skills.TryGetValue(id, out Dictionary<int, Skillgrp> skillLevel);

        return skillLevel;
    }

    public Dictionary<int, Dictionary<int, Skillgrp>> Skills { get { return _skills; } }

    private void ReadActions()
    {

        string dataPath = Path.Combine(Application.streamingAssetsPath, "Data/Meta/Skillgrp_Classic.txt");
        if (!File.Exists(dataPath))
        {
            Debug.LogWarning("File not found: " + dataPath);
            return;
        }

        using (StreamReader reader = new StreamReader(dataPath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Skillgrp skillGrp = new Skillgrp();

                string[] keyvals = line.Split('\t');

                for (int i = 0; i < keyvals.Length; i++)
                {
                    if (!keyvals[i].Contains("="))
                    {
                        continue;
                    }

                    string[] keyval = keyvals[i].Split("=");
                    string key = keyval[0];
                    string value = keyval[1];

                    switch (key)
                    {
                        case "skill_id":
                            skillGrp.Id = ParseIntSafe(value);
                            break;
                        case "skill_level":
                            skillGrp.Level = ParseIntSafe(value);
                            break;
                        case "skill_sublevel":
                            skillGrp.SubLevel = ParseIntSafe(value);
                            break;
                        case "icon_type":
                            skillGrp.IconType = Enum.TryParse<SkillType>(value, true, out var it) ? it : SkillType.Physical;
                            break;
                        case "MagicType":
                            skillGrp.MagicType = ParseIntSafe(value);
                            break;
                        case "operate_type":
                            skillGrp.OperateType = (SkillOperateType)ParseIntSafe(value);
                            break;
                        case "mp_consume":
                            skillGrp.MpConsume = ParseIntSafe(value);
                            break;
                        case "cast_range":
                            skillGrp.CastRange = ParseIntSafe(value);
                            break;
                        case "cast_style":
                            skillGrp.CastStyle = ParseIntSafe(value);
                            break;
                        case "hit_time":
                            skillGrp.HitTime = float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var ht) ? ht : 0f;
                            break;
                        case "cool_time":
                            skillGrp.CoolTime = float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var ct) ? ct : 0f;
                            break;
                        case "reuse_delay":
                            skillGrp.ReuseDelay = float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var rd) ? rd : 0f;
                            break;
                        case "effect_point":
                            skillGrp.EffectPoint = ParseIntSafe(value);
                            break;
                        case "is_magic":
                            skillGrp.IsMagic = Enum.TryParse<SkillMagicType>(value, true, out var im) ? im : SkillMagicType.None;
                            break;
                        case "origin_skill":
                            skillGrp.IsDouble = ParseIntSafe(value);
                            break;
                        case "animation":
                            string val = DatUtils.CleanupString(value).Replace("}", "").Replace("{", "");
                            switch (val.ToUpper())
                            {
                                case "A":
                                    skillGrp.CastAnimation = SkillCastAnimation.CastShort;
                                    skillGrp.ThrowAnimation = SkillThrowAnimation.MagicNoTarget;
                                    break;
                                case "B":
                                    skillGrp.CastAnimation = SkillCastAnimation.CastShort;
                                    skillGrp.ThrowAnimation = SkillThrowAnimation.MagicShot;
                                    break;
                                case "C":
                                    skillGrp.CastAnimation = SkillCastAnimation.CastShort;
                                    skillGrp.ThrowAnimation = SkillThrowAnimation.MagicThrow;
                                    break;
                                case "D":
                                    skillGrp.CastAnimation = SkillCastAnimation.CastMid;
                                    skillGrp.ThrowAnimation = SkillThrowAnimation.MagicNoTarget;
                                    break;
                                case "E":
                                    skillGrp.CastAnimation = SkillCastAnimation.CastMid;
                                    skillGrp.ThrowAnimation = SkillThrowAnimation.MagicShot;
                                    break;
                                case "F":
                                    skillGrp.CastAnimation = SkillCastAnimation.CastMid;
                                    skillGrp.ThrowAnimation = SkillThrowAnimation.MagicThrow;
                                    break;
                                case "G":
                                    skillGrp.CastAnimation = SkillCastAnimation.CastLong;
                                    skillGrp.ThrowAnimation = SkillThrowAnimation.MagicNoTarget;
                                    break;
                                case "K":
                                    skillGrp.CastAnimation = SkillCastAnimation.CastLong;
                                    skillGrp.ThrowAnimation = SkillThrowAnimation.MagicShot;
                                    break;
                                case "L":
                                    skillGrp.CastAnimation = SkillCastAnimation.CastLong;
                                    skillGrp.ThrowAnimation = SkillThrowAnimation.MagicThrow;
                                    break;
                                case "I":
                                    skillGrp.CastAnimation = SkillCastAnimation.NoCast;
                                    skillGrp.ThrowAnimation = SkillThrowAnimation.MagicShot;
                                    break;
                                case "J":
                                    skillGrp.CastAnimation = SkillCastAnimation.NoCast;
                                    skillGrp.ThrowAnimation = SkillThrowAnimation.MagicNoTarget;
                                    break;
                                case "S":
                                    skillGrp.CastAnimation = SkillCastAnimation.SpAtk01;
                                    break;
                                case "T":
                                    skillGrp.CastAnimation = SkillCastAnimation.SpAtk02;
                                    break;
                                case "U":
                                    skillGrp.CastAnimation = SkillCastAnimation.Spatk03;
                                    break;
                                case "V":
                                    skillGrp.CastAnimation = SkillCastAnimation.SpAtk04;
                                    break;
                                case "W":
                                    skillGrp.CastAnimation = SkillCastAnimation.Spatk03;
                                    break;
                                case "Y":
                                    skillGrp.CastAnimation = SkillCastAnimation.SpAtk02;
                                    break;
                                case "X":
                                    skillGrp.CastAnimation = SkillCastAnimation.WarriorBuff01;
                                    break;
                                default:
                                    if (val.StartsWith("MIX", StringComparison.OrdinalIgnoreCase))
                                    {
                                        skillGrp.CastAnimation = SkillCastAnimation.SpAtk01;
                                    }
                                    else
                                    {
                                        skillGrp.CastAnimation = SkillCastAnimation.None;
                                    }
                                    break;
                            }
                            break;
                        case "skill_visual_effect":
                            string visualEffect = DatUtils.CleanupString(value); ;
                            if (int.TryParse(visualEffect, out int effectId))
                            {
                                skillGrp.SkillVisualEffect = effectId;
                            }
                            else
                            {
                                skillGrp.SkillVisualEffect = -1;
                            }
                            break;
                        case "icon":
                            skillGrp.Icon = DatUtils.CleanupString(value);
                            break;
                        case "icon_panel":
                            skillGrp.IconPanel = DatUtils.CleanupString(value);
                            break;
                        case "debuff":
                            skillGrp.Debuff = ParseIntSafe(value);
                            break;
                        case "resist_cast":
                            skillGrp.ResistCast = ParseIntSafe(value);
                            break;
                        case "enchant_skill_level":
                            skillGrp.EnchantSkillLevel = ParseIntSafe(value);
                            break;
                        case "enchant_icon":
                            skillGrp.EnchantIcon = DatUtils.CleanupString(value);
                            break;
                        case "hp_consume":
                            skillGrp.HpConsume = ParseIntSafe(value);
                            break;
                        case "rumble_self":
                            skillGrp.RumbleSelf = ParseIntSafe(value);
                            break;
                        case "rumble_target":
                            skillGrp.RumbleTarget = ParseIntSafe(value);
                            break;
                    }
                }

                if (!SkillTable.Instance.ShouldLoadSkill(skillGrp.Id))
                {
                    continue;
                }

                TryAdd(skillGrp);
            }

            Debug.Log($"Successfully imported {_skills.Count} skills(s)");
        }

    }

    private void TryAdd(Skillgrp skillGrp)
    {
        if (_skills.ContainsKey(skillGrp.Id))
        {
            Dictionary<int, Skillgrp> dataGrp = _skills[skillGrp.Id];
            dataGrp.TryAdd(skillGrp.Level, skillGrp);
        }
        else
        {
            Dictionary<int, Skillgrp> dataGrp = CreateDict();
            AddDict(dataGrp, skillGrp);
            _skills.TryAdd(skillGrp.Id, dataGrp);
        }
    }

    private Dictionary<int, Skillgrp> CreateDict()
    {
        return new Dictionary<int, Skillgrp>();
    }

    private void AddDict(Dictionary<int, Skillgrp> dataGrp, Skillgrp skillGrp)
    {
        dataGrp.TryAdd(skillGrp.Level, skillGrp);
    }

    private static int ParseIntSafe(string value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intVal))
            return intVal;
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatVal))
            return (int)Math.Round(floatVal);
        return 0;
    }
}
