using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class MacroExecutor : MonoBehaviour
{
    private static MacroExecutor _instance;
    public static MacroExecutor Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("MacroExecutor");
                _instance = go.AddComponent<MacroExecutor>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private Coroutine _currentExecution;
    private Macro _runningMacro;

    public bool IsRunning => _currentExecution != null;
    public Macro RunningMacro => _runningMacro;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ExecuteMacro(int macroId)
    {
        Macro macro = PlayerMacros.Instance.GetMacro(macroId);
        if (macro != null)
        {
            ExecuteMacro(macro);
        }
        else
        {
            Debug.LogWarning($"Macro with ID {macroId} not found.");
        }
    }

    public void ExecuteMacro(Macro macro)
    {
        if (macro == null) return;

        StopExecution();
        _runningMacro = macro;
        _currentExecution = StartCoroutine(RunMacroRoutine(macro));
    }

    public void StopExecution()
    {
        if (_currentExecution != null)
        {
            StopCoroutine(_currentExecution);
            _currentExecution = null;
        }
        _runningMacro = null;
    }

    private IEnumerator RunMacroRoutine(Macro macro)
    {
        Debug.Log($"[MacroExecutor] Running macro: {macro.Name} (ID: {macro.Id})");

        if (macro.Commands != null)
        {
            for (int i = 0; i < macro.Commands.Count; i++)
            {
                MacroCmd cmd = macro.Commands[i];
                if (cmd == null || string.IsNullOrWhiteSpace(cmd.Command))
                {
                    continue;
                }

                string line = cmd.Command.Trim();
                yield return ExecuteLine(line);
            }
        }

        _currentExecution = null;
        _runningMacro = null;
    }

    private IEnumerator ExecuteLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) yield break;

        // Command: /delay <sec>
        if (line.StartsWith("/delay", StringComparison.OrdinalIgnoreCase))
        {
            string arg = line.Substring(6).Trim();
            if (float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out float delay) && delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
            yield break;
        }

        // Command: /target <name>
        if (line.StartsWith("/target", StringComparison.OrdinalIgnoreCase))
        {
            string targetName = line.Substring(7).Trim();
            ExecuteTarget(targetName);
            yield return new WaitForSeconds(0.1f);
            yield break;
        }

        // Command: /useskillforce <name or id>
        if (line.StartsWith("/useskillforce", StringComparison.OrdinalIgnoreCase))
        {
            string skillArg = line.Substring(14).Trim();
            ExecuteUseSkill(skillArg, true);
            yield return new WaitForSeconds(0.1f);
            yield break;
        }

        // Command: /useskill <name or id>
        if (line.StartsWith("/useskill", StringComparison.OrdinalIgnoreCase))
        {
            string skillArg = line.Substring(9).Trim();
            ExecuteUseSkill(skillArg, false);
            yield return new WaitForSeconds(0.1f);
            yield break;
        }

        // Command: /attack
        if (line.Equals("/attack", StringComparison.OrdinalIgnoreCase))
        {
            PlayerActions.Instance?.UseAction(ActionType.Attack);
            yield return new WaitForSeconds(0.1f);
            yield break;
        }

        // Command: /sit
        if (line.Equals("/sit", StringComparison.OrdinalIgnoreCase))
        {
            if (PlayerEntity.Instance != null && !PlayerEntity.Instance.WaitTypeSitting)
            {
                PlayerActions.Instance?.UseAction(ActionType.SitStand);
            }
            yield return new WaitForSeconds(0.1f);
            yield break;
        }

        // Command: /stand
        if (line.Equals("/stand", StringComparison.OrdinalIgnoreCase))
        {
            if (PlayerEntity.Instance != null && PlayerEntity.Instance.WaitTypeSitting)
            {
                PlayerActions.Instance?.UseAction(ActionType.SitStand);
            }
            yield return new WaitForSeconds(0.1f);
            yield break;
        }

        // Command: /pickup
        if (line.Equals("/pickup", StringComparison.OrdinalIgnoreCase))
        {
            PlayerActions.Instance?.UseAction(ActionType.Pickup);
            yield return new WaitForSeconds(0.1f);
            yield break;
        }

        // Chat / Voiced command
        if (ChatWindow.Instance != null)
        {
            ChatWindow.Instance.SendChatMessage(line);
        }
        else
        {
            GameClient.Instance?.ClientPacketHandler?.SendMessage(line, L2MessageType.ALL, null);
        }

        yield return new WaitForSeconds(0.05f);
    }

    private void ExecuteTarget(string targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName)) return;

        if (targetName.Equals("%self", StringComparison.OrdinalIgnoreCase) ||
            targetName.Equals("self", StringComparison.OrdinalIgnoreCase))
        {
            if (PlayerEntity.Instance != null)
            {
                TargetManager.Instance?.SetTarget(PlayerEntity.Instance);
            }
            return;
        }

        if (targetName.Equals("%target", StringComparison.OrdinalIgnoreCase))
        {
            // Keep current target
            return;
        }

        // Search near entities
        Entity found = null;
        Collider[] colliders = Physics.OverlapSphere(
            PlayerEntity.Instance != null ? PlayerEntity.Instance.Position : Vector3.zero,
            30f);

        for (int i = 0; i < colliders.Length; i++)
        {
            Entity e = colliders[i].GetComponentInParent<Entity>();
            if (e != null && e.Identity != null && !string.IsNullOrEmpty(e.Identity.Name))
            {
                if (e.Identity.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    found = e;
                    break;
                }
                if (found == null && e.Identity.Name.IndexOf(targetName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found = e;
                }
            }
        }

        if (found != null)
        {
            TargetManager.Instance?.SetTarget(found);
        }
    }

    private void ExecuteUseSkill(string skillArg, bool force)
    {
        if (string.IsNullOrWhiteSpace(skillArg)) return;

        int targetSkillId = -1;
        if (int.TryParse(skillArg, out int parsedId))
        {
            targetSkillId = parsedId;
        }
        else if (PlayerSkill.Instance != null && PlayerSkill.Instance.Skills != null)
        {
            foreach (SkillInfo si in PlayerSkill.Instance.Skills)
            {
                SkillNameData nameData = SkillNameTable.Instance?.GetName(si.Id, 1);
                if (nameData != null && !string.IsNullOrEmpty(nameData.Name))
                {
                    if (nameData.Name.Equals(skillArg, StringComparison.OrdinalIgnoreCase))
                    {
                        targetSkillId = si.Id;
                        break;
                    }
                }
            }
        }

        if (targetSkillId > 0 && PlayerSkill.Instance != null)
        {
            PlayerSkill.Instance.UseSkill(targetSkillId);
        }
    }
}
