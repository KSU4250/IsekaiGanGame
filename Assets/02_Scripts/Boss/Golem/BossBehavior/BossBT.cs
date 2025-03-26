using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BossBT : NetworkBehaviour
{
    public static event Action AttackStartCallback;
    public static event Action AttackEndCallback;
    public static event Action SpecialAttackStartCallback;
    public static event Action SpecialAttackEndCallback;

    public delegate void BTDelegate();
    public BTDelegate behaviorEndCallback;
    public BTDelegate phase2BehaviorStartCallback;

    [SerializeField] public BossState curState;
    [SerializeField] private Animator anim;
    [SerializeField] private NavMeshAgent nvAgent;
    [SerializeField] private BossStateManager bossStateManager;
    [SerializeField] private BossSkillManager bossSkillManager;
    [SerializeField] private BossAttackManager bossAttackManager;
    [SerializeField] private Vector3 center;
    [SerializeField] private BossPhase2Cam phase2Cam;
    [SerializeField] private BossPhaseSet phase2Set;

    private bool isCoroutineRunning = false;
    private bool isStun = false;
    private bool isDie = false;
    private bool isTriggerWall = false;
    private BossState previousBehavior;
    private float patternDelay = 2f;
    public Coroutine curCoroutine;

    public float PatternDelay { get { return patternDelay; } set { patternDelay = value; } }

    private void Start()
    {
        bossStateManager.bossWallTriggerCallback += ChangeMove;
    }

    private void Update()
    {
        if (IsServer)
        {
            // 죽었을때
            if (curState == BossState.Die && !isDie)
            {
                StopCoroutine(curCoroutine);
                StartCoroutine(curState.ToString());

                isDie = true;
            }

            // 스턴 걸렸을때
            if (curState == BossState.Stun && !isStun)
            {
                StopCoroutine(curCoroutine);
                StartCoroutine(curState.ToString());
                isStun = true;
            }

            // 아무것도 아닐때
            if (!isCoroutineRunning)
            {
                curCoroutine = StartCoroutine(curState.ToString());
            }
        }
    }

    #region [BossBehavior]
    private IEnumerator Idle()
    {
        yield return null;
    }

    private IEnumerator Chase()
    {
        isCoroutineRunning = true;
        previousBehavior = curState;

        // 따라 다니기 시작
        anim.SetBool("ChaseFlag", true);

        float elapseTime = 0f;

        // 상태가 바뀌고 or 1초마다 콜백 
        while (true)
        {
            nvAgent.SetDestination(bossStateManager.AggroPlayer.transform.position);
            elapseTime += Time.deltaTime;

            if (curState != BossState.Chase)
            {
                break;
            }

            if (elapseTime >= patternDelay)
            {
                elapseTime = 0f;
                behaviorEndCallback?.Invoke();
            }

            yield return null;
        }

        // 따라 다니기 멈춤
        anim.SetBool("ChaseFlag", false);
        nvAgent.ResetPath();

        isCoroutineRunning = false;
    }

    private IEnumerator Attack1()
    {

        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 쿨타임 실행, 보스 공격력 설정
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // 애니메이션 끝
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                SetAnimBool(curState, false);
                break;
            }

            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack2()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 쿨타임 실행, 보스 공격력 설정
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // 애니메이션 끝
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                SetAnimBool(curState, false);
                break;
            }

            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false; 
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack3()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 쿨타임 실행, 보스 공격력 설정
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // 애니메이션 끝
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                SetAnimBool(curState, false);
                break;
            }

            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack4()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 쿨타임 실행, 보스 공격력 설정
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // Attack4 준비모션에서는 안따라다니게 설정
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(curState.ToString()))
            {
                break;
            }
            yield return null;
        }

        while (true)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName(curState.ToString()))
            {
                break;
            }
            yield return null;
        }

        // attack4의 지속시간을 가져옴.
        BSD_Duration attack4 = bossSkillManager.Skills
            .Where(skill => skill.SkillData.SkillName == "Attack4")
            .Select(skill => skill.SkillData as BSD_Duration)
            .FirstOrDefault(bsd => bsd != null);

        float duration = attack4.Duration;
        float elapseTime = 0f;
        float elapseTime2 = 0f;

        // 애니메이션 4-1을 지속시간동안 실행
        while (true)
        {
            // 어그로 플레이어 따라다님
            if (bossStateManager.AggroPlayer == null)
            {
                Debug.Log("어그로 플레이어 null - Attack4");
            }
            else
            {
                nvAgent.SetDestination(bossStateManager.AggroPlayer.transform.position);
            }

            float t = elapseTime / duration;
            // 이동속도 설정
            nvAgent.speed = Mathf.Lerp(1f, 10f, t * t);

            elapseTime += Time.deltaTime;
            if (elapseTime >= duration)
            {
                SetAnimBool(curState, false);
                nvAgent.ResetPath();
                nvAgent.speed = 3f;
                break;
            }
            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false; 
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack5()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        AttackStartCallback?.Invoke();

        // 공격 5 끝
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5Flag", false);
                break;
            }

            yield return null;
        }

        AttackEndCallback?.Invoke();

        float elapseTime = 0f;

        ulong randomNum = bossStateManager.RandomPlayer();
        GameObject target = bossStateManager.AlivePlayers.FirstOrDefault(p => p != null && p.GetComponent<NetworkObject>().OwnerClientId == randomNum);

        // 잠시 다른 플레이어 쳐다보다가
        while (true)
        {
            nvAgent.SetDestination(target.transform.position);
            elapseTime += Time.deltaTime;

            if (elapseTime >= 1f)
            {
                anim.SetBool("Attack5-1Flag", true);
                nvAgent.ResetPath();
                elapseTime = 0f;
                break;
            }

            yield return null;
        }

        AttackStartCallback?.Invoke();

        // 공격 5-1
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5-1") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5-1Flag", false);
                break;
            }

            yield return null;
        }

        AttackEndCallback?.Invoke();

        randomNum = bossStateManager.RandomPlayer();
        target = bossStateManager.AlivePlayers.FirstOrDefault(p => p != null && p.GetComponent<NetworkObject>().OwnerClientId == randomNum);

        // 또다른 플레이어 쳐다보다가
        while (true)
        {
            nvAgent.SetDestination(target.transform.position);
            elapseTime += Time.deltaTime;

            if (elapseTime >= 1f)
            {
                anim.SetBool("Attack5-2Flag", true);
                nvAgent.ResetPath();
                elapseTime = 0f;
                break;
            }

            yield return null;
        }


        AttackStartCallback?.Invoke();

        // 공격 5-2
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5-2") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5-2Flag", false);
                break;
            }

            yield return null;
        }

        AttackEndCallback?.Invoke();

        randomNum = bossStateManager.RandomPlayer();
        target = bossStateManager.AlivePlayers.FirstOrDefault(p => p != null && p.GetComponent<NetworkObject>().OwnerClientId == randomNum);

        // 또다른 플레이어 쳐다보다가 
        while (true)
        {
            nvAgent.SetDestination(target.transform.position);
            elapseTime += Time.deltaTime;

            if (elapseTime >= 1f)
            {
                anim.SetBool("Attack5-3Flag", true);
                nvAgent.ResetPath();
                elapseTime = 0f;
                break;
            }

            yield return null;
        }

        AttackStartCallback?.Invoke();

        // 공격 5-3
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5-3") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5-3Flag", false);
                break;
            }

            yield return null;
        }

        AttackEndCallback?.Invoke();

        // 가운데로 이동해서 특수패턴(전멸기)
        // 현재 위치에서 가운데 위치로 Lerp하게 이동(15~50프레임)하면서, 점프 애니메이션 실행하면 될듯
        SpecialAttackStartCallback?.Invoke();

        Vector3 originPos = bossStateManager.Boss.transform.position;

        while (true)
        {

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5Jump"))
            {
                elapseTime += Time.deltaTime;

                if (elapseTime >= 0.5f && elapseTime <= 1.6f)
                {
                    float t = Mathf.InverseLerp(0.5f, 1.6f, elapseTime);
                    bossStateManager.Boss.transform.position = Vector3.Lerp(originPos, center, t);
                }
            }

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5Jump") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5-4Flag", true);
                break;
            }

            yield return null;
        }

        // Roar(전멸기 패턴)
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5Roar") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5-4Flag", false);
                break;
            }

            yield return null;
        }

        SpecialAttackEndCallback?.Invoke();

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack6()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 애니메이션 끝
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                SetAnimBool(curState, false);
                break;
            }

            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false; 
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack7()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 쿨타임 실행
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // Chase -> Attack7 넘어갔는지 check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(curState.ToString()))
            {
                break;
            }
            yield return null;
        }

        // Attack7이 끝났는지 Check
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                Attack7BeforeClientRpc();

                // 몇초있다가 (총 공격 delay에서 내려찍는데 까지 걸리는 시간을 뻄)
                yield return new WaitForSeconds(bossAttackManager.Delay - 1.1f);

                Attack7AfterClientRpc();

                // 애니메이션 재생 및 skin, collider 활성화
                anim.SetBool("Attack7-1Flag", true);
                break;
            }
            yield return null;
        }

        // Attack 7-1이 끝났는지 check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack7-1") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                SetAnimBool(curState, false);
                anim.SetBool("Attack7-1Flag", false);
                break;
            }
            yield return null;
        }


        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false; 
        AttackEndCallback?.Invoke();

        yield return null;
    }

    private IEnumerator Attack8()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 쿨타임 실행
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // Chase -> Attack8 넘어갔는지 check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(curState.ToString()))
            {
                break;
            }
            yield return null;
        }

        // Attack8이 끝났는지 Check
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                anim.SetBool("Attack8-1Flag", true);
                break;
            }
            yield return null;
        }

        // attack8의 지속시간을 가져옴.
        BSD_Duration attack8 = bossSkillManager.Skills
            .Where(skill => skill.SkillData.SkillName == "Attack8")
            .Select(skill => skill.SkillData as BSD_Duration)
            .FirstOrDefault(bsd => bsd != null);

        float duration = attack8.Duration;
        float elapseTime = 0f;

        // Attack 8-1이 끝났는지 check
        while (true)
        {
            if (isTriggerWall)
            {
                if (bossStateManager.AggroPlayer == null)
                {
                    bossStateManager.Boss.transform.LookAt(bossStateManager.AlivePlayers.FirstOrDefault(p => p != null && p.GetComponent<NetworkObject>().OwnerClientId == bossStateManager.RandomPlayer()).transform.position);
                }
                else
                {
                    bossStateManager.Boss.transform.LookAt(bossStateManager.AggroPlayer.transform.position);
                }

                isTriggerWall = false;
                Debug.Log("방향전환 호출됨");
            }

            elapseTime += Time.deltaTime;
            if (elapseTime >= duration)
            {
                SetAnimBool(curState, false);
                anim.SetBool("Attack8-1Flag", false);
                break;
            }
            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();

        yield return null;
    }

    private IEnumerator Attack9()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 쿨타임 실행
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // Chase -> Attack9 넘어갔는지 check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(curState.ToString()))
            {
                break;
            }
            yield return null;
        }

        // Attack9이 끝났는지 Check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(curState.ToString()) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack9-1Flag", true);
                break;
            }
            yield return null;
        }

        // Attack 9-1이 끝났는지 check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack9-1") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                SetAnimBool(curState, false);
                anim.SetBool("Attack9-1Flag", false);
                break;
            }
            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false; 
        AttackEndCallback?.Invoke();

        yield return null;
    }

    private IEnumerator Stun()
    {
        isCoroutineRunning = true;

        // 기존 작업들을 초기화 하는 코드
        // 1. flag 초기화
        // 2. 이동속도 초기화
        // 3. 보스 y값이 이상할수도 있으니, y값 초기화
        // 4. 스킨과 collider 정상화
        ResetAnimBool();
        nvAgent.speed = 3f;
        bossStateManager.Boss.transform.position = new Vector3(bossStateManager.Boss.transform.position.x, 0f, bossStateManager.Boss.transform.position.z);
        nvAgent.ResetPath();

        // 동기화 해야하는거
        StunSyncClientRpc();

        // 스턴 애니메이션 강제 재생
        anim.Play("Stun");

        // 몇초후
        yield return new WaitForSeconds(3f);


        StunSync2ClientRpc();

        if (previousBehavior != BossState.Attack8)
        {
            // 상태를 스턴걸리기 전 상태로
            curState = previousBehavior;
        }
        else
        {
            curState = BossState.Chase;

            // 패턴이 끝났음을 콜백
            behaviorEndCallback?.Invoke();
        }

        isCoroutineRunning = false;
        isStun = false;
    }

    private IEnumerator Phase2()
    {
        isCoroutineRunning = true;
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 가운데로 이동해서 쿠와왕
        // 현재 위치에서 가운데 위치로 Lerp하게 이동(15~50프레임)하면서, 점프 애니메이션 실행하면 될듯
        Vector3 originPos = bossStateManager.Boss.transform.position;

        float elapseTime = 0f;
        bool once = true;

        Quaternion targetRotation = Quaternion.Euler(0, -180, 0);

        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Phase2"))
            {

                if (once)
                {
                    once = false;
                    // 카메라 움직이고, 브금 변경 콜백
                    Phase2StartClientRpc();
                }

                // 보스 가운대로 설정하고
                elapseTime += Time.deltaTime;
                if (elapseTime >= 0.5f && elapseTime <= 1.6f)
                {
                    float t = Mathf.InverseLerp(0.5f, 1.6f, elapseTime);
                    bossStateManager.Boss.transform.position = Vector3.Lerp(originPos, center, t);

                    bossStateManager.Boss.transform.rotation = Quaternion.Slerp(bossStateManager.Boss.transform.rotation, targetRotation, t);
                }
            }

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Phase2") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Phase2Flag", false);
                anim.SetBool("Phase2-1Flag", true);
                break;
            }

            yield return null;
        }

        once = true;

        // 쿠와왕
        while (true)
        {
            // 쿠와왕 하는 타이밍
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Phase2-1") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.2f && once)
            {
                // 카메라 흔들기
                once = false;
                Phase2RoarClientRpc();
            }

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Phase2-1") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                // 카메라 원상복귀
                Phase2EndClientRpc();

                anim.SetBool("Phase2-1Flag", false);
                break;
            }

            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
    }

    private IEnumerator Die()
    {
        anim.Play("Die");

        yield return null;
    }

    private IEnumerator TimeOut()
    {
        isCoroutineRunning = true;
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 가운데로 이동해서 특수패턴(전멸기)
        // 현재 위치에서 가운데 위치로 Lerp하게 이동(15~50프레임)하면서, 점프 애니메이션 실행하면 될듯
        float elapseTime = 0f;
        Vector3 originPos = bossStateManager.Boss.transform.position;

        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("TimeOut"))
            {
                elapseTime += Time.deltaTime;

                if (elapseTime >= 0.5f && elapseTime <= 1.6f)
                {
                    float t = Mathf.InverseLerp(0.5f, 1.6f, elapseTime);
                    bossStateManager.Boss.transform.position = Vector3.Lerp(originPos, center, t);
                }
            }

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("TimeOut") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("TimeOut2Flag", true);
                break;
            }

            yield return null;
        }

        // Roar(전멸기 패턴)
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("TimeOut2") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("TimeOut2Flag", false);
                anim.SetBool("TimeOutFlag", false);
                break;
            }

            yield return null;
        }


        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
    }
    #endregion

    #region [Function]
    // 애니메이션 bool값 설정
    private void SetAnimBool(BossState _state, bool _isActive)
    {
        anim.SetBool(_state.ToString() + "Flag", _isActive);
    }

    // 애니메이션 끝났는지 check하는 조건문
    private bool CheckEndAnim(BossState _state)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(_state.ToString()) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // 애니메이션 모든 bool값 초기화
    private void ResetAnimBool()
    {
        AnimatorControllerParameter[] parameters = anim.parameters;

        foreach (var parameter in parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(parameter.name, false);
            }
        }
    }

    private void ChangeMove()
    {
        if (anim.GetBool("Attack8Flag"))
        {
            isTriggerWall = true;
        }
    }
    #endregion

    #region [ClientRpc]

    [ClientRpc]
    private void Attack7BeforeClientRpc()
    {
        // boss의 skin을 사라지게 하기, collider도 잠시 비활성화
        bossStateManager.BossSkin.SetActive(false);
        bossStateManager.HitCollider.enabled = false;

        // boss의 Tr을 공격위치로 옮기기
        bossStateManager.Boss.transform.position = bossAttackManager.CircleSkillPos[0].transform.position;
    }

    [ClientRpc]
    private void Attack7AfterClientRpc()
    {
        bossStateManager.BossSkin.SetActive(true);
        bossStateManager.HitCollider.enabled = true; ;
    }

    // 스턴 상태 동기화
    [ClientRpc]
    private void StunSyncClientRpc()
    {
        bossStateManager.BossSkin.SetActive(true);
        bossStateManager.HitCollider.enabled = true;
        bossStateManager.Boss.tag = "Untagged";
        SetAnimBool(BossState.Stun, true);
    }

    [ClientRpc]
    private void StunSync2ClientRpc()
    {
        SetAnimBool(BossState.Stun, false);
    }

    // 카메라 움직이고, 브금 변경 콜백
    [ClientRpc]
    private void Phase2StartClientRpc()
    {
        StartCoroutine(phase2Cam.MoveCam());
        phase2BehaviorStartCallback?.Invoke();
    }

    // 쿠와왕할때 동기화 시킬것들
    [ClientRpc]
    private void Phase2RoarClientRpc()
    {
        // 카메라 흔들기
        StartCoroutine(phase2Cam.ShakeCam());

        // Map Material 변경(여기서 Fire네모, 나무에 Fire생성하는 작업까지), 보스 Mat 변경, 보스 Fire이펙트, 맵장판 파티클 생성하는것도 설정, 보스 2페이즈로 세팅하는것까지 
        phase2Set.BossPhase2Set();
    }

    // 페이즈2 끝나고
    [ClientRpc]
    private void Phase2EndClientRpc()
    {
        // 카메라 원상복귀
        StartCoroutine(phase2Cam.ReturnCam());
    }
    #endregion

}