using System;
using System.Collections;
using System.Collections.Generic;
using EnumTypes;
using StructTypes;
using UnityEngine;

public class PlayerAiManager : MonoBehaviour
{
    public bool isPlayerAiMode = true; // 현재 AI 모드인지 아닌지 확인하는 bool
    public bool isMoveBack = false; // 도망 상태 확인하는 bool
    public PlayerManager playerManager; // 플레이어를 조종하기위해 PlayerManager 클래스 참조
    private List<Vector3> rocksPos = new List<Vector3>(); // 돌들의 위치를 저장하는 리스트
    public Vector3 mapCenterPos; // 맵의 중앙 위치 
    public float rockSize = 2.0f; // 돌의 크기 (예시로 설정, 실제 돌 크기에 맞게 조정)
    float safeDistance = 3f;
    private Action Action;
    // SkillSlot 배열에 사용할 스킬을 나열
    SkillSlot[] skillSlots = new SkillSlot[]
    {
        SkillSlot.Skill_A,
        SkillSlot.Skill_B,
        SkillSlot.Skill_C
    };

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        BossBT.AttackStartCallback += () => StartCoroutine(MoveBackwardsBoss()); // 보스 공격 콜백 코루틴 플레이어 도망 시작
        BossBT.AttackEndCallback += () => isMoveBack = false;
        BossBT.SpecialAttackEndCallback += () => StartCoroutine(MoveBehindRock()); // 보스 전멸기 시작 코루틴 플레이어 돌뒤로 도망
        Action += () => StartCoroutine(MoveBackwardsBoss());
    }

    #region 위치 파악

    private void UpdateRockPositions() // 돌 위치 파악
    {
        rocksPos.Clear();
        GameObject[] rocks = GameObject.FindGameObjectsWithTag("Rock");

        foreach (GameObject rock in rocks)
        {
            rocksPos.Add(rock.transform.position);
        }
    }

    // 가장 가까운 돌을 찾는 함수
    private Vector3 FindClosestRock()
    {
        Vector3 closestRock = Vector3.zero;
        float closestDistance = float.MaxValue;

        // 돌들의 위치 중 가장 가까운 돌을 찾음
        foreach (Vector3 rockPos in rocksPos)
        {
            float distanceToRock = Vector3.Distance(rockPos, transform.position); // 돌과 플레이어의 거리
            if (distanceToRock < closestDistance) // 가장 가까운 돌을 찾음
            {
                closestDistance = distanceToRock;
                closestRock = rockPos;
            }
        }

        return closestRock; // 가장 가까운 돌의 위치 반환
    }

    #endregion
    // 돌 뒤로 도망갈 위치를 계산하는 함수
    private Vector3 GetPositionBehindRock(Vector3 rockPos)
    {
        // 돌의 뒤쪽으로 위치를 계산 (플레이어와 돌의 상대적인 위치)
        Vector3 directionToRock = (transform.position - rockPos).normalized; // 돌과 플레이어의 상대 방향
        Vector3 positionBehindRock = rockPos + directionToRock * rockSize; // 돌 뒤로 이동 (돌의 크기만큼 떨어진 곳)

        return positionBehindRock; // 계산된 위치 반환
    }
    #region 이동 처리

    private void AiMove(float _x, float _z) // 플레이어의 이동 처리 함수
    {
        if (playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Idle)
        {
            JoystickInputData data = new JoystickInputData
            {
                x = Mathf.Clamp(_x, -1f, 1f),
                z = Mathf.Clamp(_z, -1f, 1f)
            };

            Vector3 direction = new Vector3(data.x, 0, data.z);
            if (direction.magnitude > 1)
            {
                direction.Normalize();
                data.x = direction.x;
                data.z = direction.z;
            }

            playerManager.MovementManager.MoveByJoystick(data);
        }
    }

    #endregion

    #region 스킬 사용 및 이동 처리

    private void UseSkillWithApproach(SkillSlot skillType)
    {
        Vector3 bossPos = GameManager.Instance.GetBossTransform().position;
        Vector3 playerPos = transform.position;
        float distanceToBoss = Vector3.Distance(playerPos, bossPos);

        RangeSkill rangeSkill = playerManager.SkillManager.GetSkillData(skillType) as RangeSkill;

        if (rangeSkill == null) // 근접 스킬
        {
            HandleMeleeSkill(skillType, distanceToBoss, bossPos);
        }
        else // 범위 스킬
        {
            HandleRangeSkill(skillType, distanceToBoss, rangeSkill.attackRange, bossPos);
        }
    }

    private void HandleMeleeSkill(SkillSlot skillType, float distanceToBoss, Vector3 bossPos)
    {
        if (distanceToBoss <= safeDistance)
        {
            UseMeleeSkill(skillType, bossPos);
        }
        else
        {
            MoveTowardsBoss(bossPos); // 근접 스킬을 사용하기 위해 보스에게 접근
        }
    }

    private void HandleRangeSkill(SkillSlot skillType, float distanceToBoss, float attackRange, Vector3 bossPos)
    {
        if (distanceToBoss <= attackRange)
        {
            UseRangeSkill(skillType, bossPos);
        }
        else
        {
            MoveTowardsBoss(bossPos); // 범위 스킬을 사용하려면 보스에게 접근
        }
    }

    private void UseMeleeSkill(SkillSlot skillType, Vector3 bossPos)
    {
        playerManager.InputManager.lastSkillUsePoint = new Vector3(bossPos.x, 0, bossPos.z);
        playerManager.SkillManager.TryUseSkill(skillType, bossPos);
        Debug.Log("근접스킬");
    }

    private void UseRangeSkill(SkillSlot skillType, Vector3 bossPos)
    {
        playerManager.InputManager.lastSkillUsePoint = new Vector3(bossPos.x, 0, bossPos.z);
        playerManager.SkillManager.TryUseSkill(skillType, bossPos);
        Debug.Log("범위스킬");
    }

    private void MoveTowardsBoss(Vector3 bossPos)
    {
        Vector3 directionToTarget = bossPos - transform.position;
        AiMove(directionToTarget.x, directionToTarget.z);
    }
    private bool isCoroutineRunning = false; // 코루틴 실행 여부를 추적

    private IEnumerator MoveBackwardsBoss()
    {
        // 이미 코루틴이 실행 중이면 다시 실행하지 않도록 방지
        if (isCoroutineRunning)
        {
            yield break; // 이미 실행 중인 코루틴이 있으면 종료
        }

        isCoroutineRunning = true; // 코루틴 시작

        while (isMoveBack)
        {
            // 보스의 위치와 자신의 위치를 비교하여 이동 방향을 계산
            Vector3 bossPos = GameManager.Instance.GetBossTransform().position;
            Vector3 directionToTarget = transform.position - bossPos;

            // 이동 처리 함수 호출
            AiMove(directionToTarget.x, directionToTarget.z);

            yield return null;
        }

        isCoroutineRunning = false; // 코루틴 종료
        yield break;
    }
    private IEnumerator MoveBehindRock()
    {
        // 이미 코루틴이 실행 중이면 종료 시키고 이 코루틴을 시작하지 않음
        if (isCoroutineRunning)
        {
            yield break; // 이미 실행 중이면 현재 코루틴을 종료하고 새로 시작하지 않음
        }

        isCoroutineRunning = true; // 코루틴 시작

        // 돌 위치 리스트가 비어 있으면 업데이트
        UpdateRockPositions();

        // 돌이 없으면 (돌을 찾지 못한 경우) 바로 종료
        if (rocksPos.Count == 0)
        {
            isCoroutineRunning = false;
            yield break;
        }

        // 가장 가까운 돌 찾기
        Vector3 closestRock = FindClosestRock();

        // 가장 가까운 돌을 기준으로 목표 위치 계산
        Vector3 targetPos = GetPositionBehindRock(closestRock);

        // 목표 위치까지 이동
        while (isMoveBack)
        {
            // 현재 위치에서 목표 위치로 이동하는 방향 계산
            Vector3 directionToTarget = targetPos - transform.position;
            AiMove(directionToTarget.x, directionToTarget.z); // 이동 처리

            // 목표 위치에 도달했는지 체크
            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            {
                break; // 목표 위치에 도달하면 종료
            }

            yield return null; // 매 프레임 대기
        }

        isCoroutineRunning = false; // 코루틴 종료
        yield break;
    }


    #endregion

    #region AI 주기적인 행동

    private void Update()
    {
        Vector3 bossPos = GameManager.Instance.GetBossTransform().position;
        Vector3 playerPos = transform.position;
        float distanceToBoss = Vector3.Distance(playerPos, bossPos);
        if (isMoveBack == false)// 도망중이 아니라면
        {
            // 랜덤으로 하나의 스킬 선택
            int randomIndex = UnityEngine.Random.Range(0, skillSlots.Length);
            // 선택된 스킬로 스킬을 사용하고, 적절히 이동
            UseSkillWithApproach(skillSlots[randomIndex]);
        }
        else
        {
            StartCoroutine(MoveBackwardsBoss());
        }
    }

    #endregion
}
