using System.Collections.Generic;
using UnityEngine;

using EnumTypes;

public class SkillButtonsManager : MonoBehaviour
{
    private UIBattleUIManager battleUIManager = null;

    private Dictionary<SkillSlot, ButtonSetting> skillButtonMap = null;

    public void OnSkillJoystickDown(SkillSlot _slot)
    {
        if (battleUIManager == null)
        {
            Debug.LogWarning("Battle UI Manager is Null!");
            return;
        }

        battleUIManager.OnSkillJoystickDown(_slot);
    }

    public void OnSkillJoystickUp(SkillSlot _slot)
    {
        if (battleUIManager == null)
        {
            Debug.LogWarning("Battle UI Manager is Null!");
            return;
        }

        battleUIManager.OnSkillJoystickUp(_slot);
    }

    public void OnSkillButtonUp(SkillSlot _slot)
    {
        battleUIManager.OnSkillButtonUp(_slot);
    }

    public void ApplyCooltime(SkillSlot _slot, float _time)
    {
        skillButtonMap[_slot].SetCooltime(_time);
    }

    public void SendSkillDirection(SkillSlot _slot, float _horizontal, float _vertical)
    {
        if (battleUIManager == null)
        {
            Debug.LogWarning("Battle UI Manager is Null!");
            return;
        }

        battleUIManager.SendSkillDirectionToSkillUI(_slot, _horizontal, _vertical);
    }

    public void SetSkillButtonEnabled(SkillSlot _slot, bool _enabled)
    {
        skillButtonMap[_slot].gameObject.SetActive(_enabled);
    }

    public void DisableButtonInteractable()
    {
        foreach (var skillBtn in skillButtonMap)
        {
            skillBtn.Value.DisableSkillButton();
        }
    }

    private void Awake()
    {
        battleUIManager = GetComponentInParent<UIBattleUIManager>();
        skillButtonMap = new Dictionary<SkillSlot, ButtonSetting>();

        ButtonSetting[] skillButtons = GetComponentsInChildren<ButtonSetting>(true);

        foreach(var skillButton in skillButtons)
        {
            skillButtonMap.Add(skillButton.ButtonSkillType, skillButton);
        }
    }
}
