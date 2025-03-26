using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using EnumTypes;
using StructTypes;

public class PlayerSkillManager
{
    private PlayerManager playerManager = null;

    private List<PlayerSkillBase> skillDatas = null;

    private Animator animator = null;

    /// <summary>
    /// 애니메이터의 각 트리거 아이디를 미리 캐싱해놓고, 이를 스킬 타입에 따라 저장해놓음.
    /// </summary>
    private Dictionary<SkillSlot, int> animatorIdMap = null;

    private Dictionary<SkillSlot, PlayerSkillBase> skillDataMap = null;
    private Dictionary<SkillSlot, float> currentCoolTimeMap = null;

    #region Public Functions

    /// <summary>
    /// 플레이어 스킬 매니저를 초기화한다.
    /// </summary>
    /// <param name="_skilldataList">스킬 데이터 리스트</param>
    public void Init(PlayerManager _mng)
    {
        playerManager = _mng;
        skillDatas = _mng.PlayerData.skills;

        animator = _mng.GetComponent<Animator>();

        animatorIdMap = new Dictionary<SkillSlot, int>();
        skillDataMap = new Dictionary<SkillSlot, PlayerSkillBase>();
        currentCoolTimeMap = new Dictionary<SkillSlot, float>();

        foreach (var data in skillDatas)
        {
            skillDataMap.Add(data.skillSlot, data);
            currentCoolTimeMap.Add(data.skillSlot, 0f);
        }

        // 우선 하드코딩으로 애니메이터 id를 캐싱함. 확장성 생각하면 나중에 수정할 것!
        {
            animatorIdMap.Add(SkillSlot.Skill_A, Animator.StringToHash("Skill_A"));
            animatorIdMap.Add(SkillSlot.Skill_B, Animator.StringToHash("Skill_B"));
            animatorIdMap.Add(SkillSlot.Skill_C, Animator.StringToHash("Skill_C"));
            animatorIdMap.Add(SkillSlot.BasicAttack, Animator.StringToHash("BasicAttack"));
            animatorIdMap.Add(SkillSlot.Dash, Animator.StringToHash("Dash"));
            animatorIdMap.Add(SkillSlot.Revive, Animator.StringToHash("ReviveOther"));
        }

    }

    /// <summary>
    /// 실제로 스킬을 사용한다.
    /// </summary>
    /// <param name="_skillIdx">사용할 스킬의 스킬 리스트 상 인덱스</param>
    public void UseSkill(SkillSlot _slot)
    {
        // 사용하려는 변수들의 유효성 체크.
        if (skillDatas == null || currentCoolTimeMap == null || !skillDataMap.ContainsKey(_slot))
        {
            Debug.LogWarning("Skill is not valid!");
            return;
        }

        if(!IsSkillUsable(_slot))
        {
            Debug.LogWarning("Skill is not usable at this time!");
            return;
        }

        // 쿨타임 적용
        currentCoolTimeMap[_slot] = skillDataMap[_slot].coolTime;

        // 스킬 애니메이션 시작
        if (animatorIdMap.TryGetValue(_slot, out int animId))
        {
            if(GameManager.Instance.IsLocalGame)
            {
                animator.SetTrigger(animId);
            }
            else if (playerManager.PlayerNetworkManager.IsClientPlayer())
            {
                playerManager.PlayerNetworkManager.SetNetworkAnimatorTrigger(animId);
            }
        }
    }

    public float GetCoolTime(SkillSlot _type)
    {
        return currentCoolTimeMap[_type];
    }

    /// <summary>
    /// 모든 스킬들의 쿨타임을 변화량만큼 뺀다.
    /// </summary>
    /// <param name="_deltaTime">변화량</param>
    public void DecreaseCoolTimes(float _delta)
    {
        foreach (var key in currentCoolTimeMap.Keys.ToList())
        {
            if(currentCoolTimeMap[key] > 0f)
                currentCoolTimeMap[key] = Mathf.Clamp(currentCoolTimeMap[key] - _delta, 0f, 10000f);
        }
    }


    /// <summary>
    /// 현재 지정한 스킬 타입의 스킬이 사용 가능한 지 체크한다.
    /// </summary>
    /// <param name="_skillIdx">체크할 스킬의 타입</param>
    /// <returns></returns>
    public bool IsSkillUsable(SkillSlot _type)
    {
        if(!skillDataMap.ContainsKey(_type))
        {
            Debug.LogFormat("{0} Skill is not usable in this character!", _type);
            return false;
        }

        // 쿨타임 체크
        if (currentCoolTimeMap[_type] > 0f)
        {
            Debug.LogFormat("{0} Skill is coolTime!", _type);
            return false;
        }

        return true;
    }

    public void SkillAction(SkillSlot _type)
    {
        skillDataMap[_type].UseSkill(playerManager);
    }

    public PlayerSkillBase GetSkillData(SkillSlot _type)
    {
        return skillDataMap[_type];
    }

    /// <summary>
    /// 스킬 발동을 시도한다.
    /// </summary>
    /// <param name="_skillIdx"></param>
    public bool TryUseSkill(SkillSlot _type, SkillPointData _point)
    {
        // 스킬 발동에 성공했다면
        if (IsSkillUsable(_type))
        {
            // 캐릭터를 포인트로 지정한 방향을 보도록 한다.
            if (_point.type == SkillPointType.Position)
            {
                Vector3 tempPos = _point.skillUsedPosition;
                tempPos.y = 0f;
                tempPos.Normalize();
                playerManager.transform.rotation = Quaternion.LookRotation(tempPos);
            }
            else
            {
                playerManager.transform.rotation = _point.skillUsedRotation;
            }

            UseSkill(_type);

            // UI에 쿨타임을 적용한다.
            if (playerManager.BattleUIManager != null)
                playerManager.BattleUIManager.ApplyCooltime(_type, GetCoolTime(_type));

            return true;
        }

        return false;
    }

    public void TryUseSkill(SkillSlot _type, Vector3 _position)
    {
        // 스킬 발동에 성공했다면
        if (IsSkillUsable(_type))
        {
            Vector3 direction = _position - playerManager.transform.position;
            direction.y = 0f;
            playerManager.transform.rotation = Quaternion.LookRotation(direction);

            UseSkill(_type);

            // UI에 쿨타임을 적용한다.
            if (playerManager.BattleUIManager != null)
                playerManager.BattleUIManager.ApplyCooltime(_type, GetCoolTime(_type));
        }
    }

    #endregion
}
