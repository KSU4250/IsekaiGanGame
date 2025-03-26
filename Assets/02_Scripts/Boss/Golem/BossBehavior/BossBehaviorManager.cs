using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BossBehaviorManager : NetworkBehaviour
{
    [SerializeField] private BossStateManager bossStateManager;
    [SerializeField] private BossSkillManager bossSkillManager;
    [SerializeField] private BossBT bossBT;

    private List<BossSkill> tmpList = new List<BossSkill>();
    private WaitForSeconds delay1f = new WaitForSeconds(1f);
    private bool hp10Trigger = false;
    private bool hpHalfTrigger = false;
    private int hp10Cnt = 0;
    private bool TimeOutTrigger = false;


    private void Start()
    {
        if (IsServer)
        {
            bossBT.behaviorEndCallback += () => StartCoroutine(BossPerformAction());
            bossStateManager.bossHp10Callback += SetHP10;
            bossStateManager.bossHpHalfCallback += SetHPHalf;
            bossStateManager.bossStunCallback += SetStun;
            bossStateManager.bossDieCallback += SetDie;
            bossStateManager.bossTimeOutCallback += SetTimeOut;
        }
    }

    // 특정조건에서 랜덤한 행동을 하나 선택하는 함수
    // 사거리와 쿨타임 계산후 랜덤한 상태를 enum값으로 리턴
    private BossState GetRandomAction()
    {
        Debug.Log("GetRandomAction실행됨");

        tmpList.Clear();

        float dis = bossStateManager.GetDisWithoutY();

        tmpList = bossSkillManager.IsSkillInRange(dis, bossSkillManager.RandomSkills);
        tmpList = bossSkillManager.IsSkillCooldown(tmpList);
        tmpList = bossSkillManager.CheckBackAttack(tmpList, bossStateManager.AlivePlayers, bossStateManager.Boss);

        int randomIndex = UnityEngine.Random.Range(0, tmpList.Count);

        if (tmpList.Count == 0)
        {
            return BossState.Chase;
        }

        return (BossState)Enum.Parse(typeof(BossState), tmpList[randomIndex].SkillData.SkillName);
    }

    // 보스가 특정 행동을 하도록 설정하는 함수
    private void SetBossBehavior(BossState _state)
    {
        bossBT.curState = _state;
    }

    // 보스가 특정행동 하도록 최종실행시키는 함수
    private IEnumerator BossPerformAction()
    {
        Debug.Log("BossPerformAction실행됨");

        // 패턴 후 딜레이
        yield return delay1f;

        if (TimeOutTrigger)
        {
            TimeOutTrigger = false;
            SetBossBehavior(BossState.TimeOut);
            yield break;
        }

        // 피가 10퍼 깍였을때 쓰는 패턴
        if (hp10Trigger)
        {
            hp10Trigger = false;
            hp10Cnt++;
            SetBossBehavior(BossState.Attack6);
            yield break;
        }

        // 돌 4번 던진후 쓰는 패턴
        if (hp10Cnt == 4)
        {
            hp10Cnt = 0;
            SetBossBehavior(BossState.Attack5);
            yield break;
        }

        // 피가 50퍼 깍였을때 쓰는 패턴
        if (hpHalfTrigger)
        {
            hpHalfTrigger = false;
            SetBossBehavior(BossState.Phase2);
            yield break;
        }

        // 각종 조건들에 따라 다르게 실행 
        SetBossBehavior(GetRandomAction());
    }

    // hp10퍼 까였을때 실행되는 패턴설정
    private void SetHP10()
    {
        hp10Trigger = true;
    }

    // hp50퍼 까였을때 실행되는 패턴설정
    private void SetHPHalf()
    {
        hpHalfTrigger = true;
    }

    // 타임아웃됬을때 호출되는 함수
    private void SetTimeOut()
    {
        TimeOutTrigger = true;
    }

    // 보스가 스턴일때
    private void SetStun()
    {
        SetBossBehavior(BossState.Stun);
    }

    // 보스가 죽었을떄
    private void SetDie()
    {
        SetBossBehavior(BossState.Die);
    }
}
