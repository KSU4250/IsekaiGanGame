using System.Collections.Generic;
using UnityEngine;

using EnumTypes;
using StructTypes;

/// <summary>
/// 스킬 범위 및 방향 지정 UI 들을 관리하기 위한 매니저 클래스.
/// </summary>
public class SkillUIManager : MonoBehaviour
{
    [SerializeField]
    private List<SkillUIData> skillUIList = null;

    private Dictionary<SkillSlot, SkillUI_Base> skillUIMap = null;

    public Dictionary<SkillSlot, SkillUI_Base> SkillUIMap
    {
        get { return skillUIMap; }
    }

    /// <summary>
    /// 스킬 범위 혹은 스킬 사용 방향의 출력 여부를 설정한다.
    /// </summary>
    /// <param name="_type">스킬 타입</param>
    /// <param name="_enabled">출력 여부</param>
    public void SetSkillUIEnabled(SkillSlot _type, bool _enabled)
    {
        if(skillUIMap.TryGetValue(_type, out var skillUI))
        {
            skillUI.SetEnabled(_enabled);
        }
        else
        {
            Debug.LogFormat("{0} type skill don't need to show skill area!", _type);
        }
    }

    /// <summary>
    /// 스킬 범위 혹은 스킬 사용 방향을 입력받은 값만큼 움직인다.
    /// </summary>
    /// <param name="_type">스킬 타입</param>
    /// <param name="_horizontal">가로 입력값</param>
    /// <param name="_vertical">세로 입력값</param>
    public void SetSkillAimPoint(SkillSlot _type, float _horizontal, float _vertical)
    {
        if (skillUIMap.TryGetValue(_type, out var skillUI))
        {
            skillUI.AimSkill(_horizontal,_vertical);
        }
    }

    /// <summary>
    /// 스킬이 사용될 범위 혹은 스킬 사용 방향을 리턴한다.
    /// </summary>
    /// <param name="_slot">스킬 타입</param>
    /// <returns>스킬 범위 or 스킬 방향(Euler Angle)</returns>
    public SkillPointData GetSkillAimPoint(SkillSlot _slot)
    {
        if (skillUIMap.TryGetValue(_slot, out var skillUI))
        {
            return skillUI.GetSkillAimPoint();
        }
        else
        {
            Debug.LogFormat("{0} type skill don't need to show skill area!", _slot);

            return new SkillPointData();
        }
    }

    /// <summary>
    /// 지정한 스킬 UI 외에 다른 스킬 UI가 활성화되어있는지 확인한다.
    /// </summary>
    /// <param name="_slot">지정 스킬 슬롯</param>
    /// <param name="_otherSkillSlot">다른 활성화중인 스킬 UI의 슬롯</param>
    /// <returns>다른 스킬 UI의 활성화 여부</returns>
    public bool IsOtherSkillUIEnabled(SkillSlot _slot, out SkillSlot _otherSkillSlot)
    {
        foreach(var skillUIPair in skillUIMap)
        {
            if (skillUIPair.Key != _slot && skillUIPair.Value.IsEnabled())
            {
                _otherSkillSlot = skillUIPair.Key;
                return true;
            }
        }

        _otherSkillSlot = SkillSlot.None;

        return false;
    }

    public void SetAllSkillUIEnabled(bool _enabled)
    {
        if(skillUIMap != null)
        {
            foreach (var skillUI in skillUIMap.Values)
            {
                if(skillUI != null)
                    skillUI.SetEnabled(_enabled);
            }
        }
    }

    private void Awake()
    {
        skillUIMap = new Dictionary<SkillSlot, SkillUI_Base>();

        foreach(var skillUIData in skillUIList)
        {
            skillUIMap.Add(skillUIData.skillType, skillUIData.skillUI);
        }
    }
}
