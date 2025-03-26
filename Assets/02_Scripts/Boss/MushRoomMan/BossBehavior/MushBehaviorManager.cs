using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// 보스가 어떤 행동을 할지 결정
public class MushBehaviorManager : NetworkBehaviour
{
    // 참조할것들
    private MushStateManager mushStateManager;
    private BossSkillManager mushSkillManager;
    private MushBT mushBT;

    // 보스BehaviourManager 데이터
    private List<BossSkill> tmpList = new List<BossSkill>();
    private WaitForSeconds delay1f = new WaitForSeconds(1f);
    private bool attack3Trigger = false;

    // 초기화
    private void Awake()
    {
        mushStateManager = FindAnyObjectByType<MushStateManager>();
        mushSkillManager = FindAnyObjectByType<BossSkillManager>();
        mushBT = FindAnyObjectByType<MushBT>();
    }

    private void Start()
    {
        if (IsServer)
        {
            mushBT.behaviorEndCallback += () => StartCoroutine(BossPerformAction());
            mushStateManager.bossDieCallback += SetDie;
            mushStateManager.bossChangeStateCallback += SetChase;
            mushStateManager.bossHp25Callback += SetAttack3;
        }
    }

    #region [Behavior]

    // 특정조건에서 랜덤한 행동을 하나 선택하는 함수
    // 사거리와 쿨타임 계산후 랜덤한 상태를 enum값으로 리턴
    private MushState GetRandomAction()
    {
        tmpList.Clear();

        float dis = GetDisWithoutY();

        tmpList = mushSkillManager.IsSkillInRange(dis, mushSkillManager.RandomSkills);
        tmpList = mushSkillManager.IsSkillCooldown(tmpList);

        int randomIndex = UnityEngine.Random.Range(0, tmpList.Count);

        if (tmpList.Count == 0)
        {
            return MushState.Chase;
        }

        return (MushState)Enum.Parse(typeof(MushState), tmpList[randomIndex].SkillData.SkillName);
    }

    // 보스가 특정 행동을 하도록 설정하는 함수
    private void SetBossBehavior(MushState _state)
    {
        mushBT.CurState = _state;
    }

    // 보스가 특정행동 하도록 최종실행시키는 함수
    private IEnumerator BossPerformAction()
    {
        // 패턴 후 딜레이
        yield return delay1f;

        if (attack3Trigger)
        {
            attack3Trigger = false;
            SetBossBehavior(MushState.Attack3);
            yield break;
        }

        // 각종 조건들에 따라 다르게 실행 
        SetBossBehavior(GetRandomAction());
    }

    // 보스가 죽었을떄
    private void SetDie()
    {
        SetBossBehavior(MushState.Die);
    }

    private void SetChase()
    {
        SetBossBehavior(MushState.Chase);
    }

    private void SetAttack3()
    {
        attack3Trigger = true;
    }

    #endregion

    #region [Function]

    // 플레이어와 보스 거리 계산
    private float GetDisWithoutY()
    {
        Vector2 bossPos2D = new Vector2(mushStateManager.Boss.transform.position.x, mushStateManager.Boss.transform.position.z);
        Vector2 playerPos2D = new Vector2(mushStateManager.AggroPlayer.transform.position.x, mushStateManager.AggroPlayer.transform.position.z);

        return Vector2.Distance(bossPos2D, playerPos2D);
    }

    #endregion
}
