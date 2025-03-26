using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BossAttackManager : NetworkBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private BossStateManager bossStateManager;
    [SerializeField] private BossSkillManager bossSkillManager;

    [Header("부채꼴 공격 관련")]
    [SerializeField] private GameObject fanSkillPos;
    [SerializeField] private GameObject fanAttackCollider;
    [SerializeField] private DecalProjector fanFullRangeDecal;
    [SerializeField] private DecalProjector fanChargingRangeDecal;

    [Header("반원 공격 관련")]
    [SerializeField] private GameObject halfCircleSkillPos;
    [SerializeField] private GameObject halfCircleAttackCollider;
    [SerializeField] private DecalProjector halfCircleRangeDecal;

    [Header("원 공격 관련")]
    [SerializeField] private GameObject[] circleSkillPos;
    [SerializeField] private GameObject[] circleAttackColliders;
    [SerializeField] private DecalProjector[] circleFullRangeDecals;
    [SerializeField] private DecalProjector[] circleChargingRangeDecals;

    [Header("돌진 공격 관련")]
    [SerializeField] private GameObject recDecalPos;
    [SerializeField] private DecalProjector recDecal; 

    [Header("돌 던지기")]
    [SerializeField] private GameObject P_Stone;
    [SerializeField] private Transform rightHand;
    [SerializeField] private GameObject P_Stone2;

    [Header("파티클")]
    [SerializeField] private GameObject attack4;

    // 스킬 관련
    private BossSkillData skill;
    private string skillName;
    private float range;
    private int damage;
    private float delay;
    private float duration;
    private float knockBackDis;

    // 부채꼴 생성 관련
    private float angle;
    private float radius;
    private float thickness;
    private int segmentCount = 50;
    private bool fanFirstTime = false;
    private bool halfCircleFirstTime = false;

    // 공격 콜라이더 관련
    private WaitForSeconds attackColliderTime = new WaitForSeconds(0.3f);

    // 랜덤 타겟
    private GameObject randomTarget;

    // 애니메이션 속도 배속시 delay감소 시켜야함.
    private float animSpd = 1f;

    // 돌 던지기 관련(stun시 돌도 사라지도록 만들어야함)
    private GameObject curStone;
    private Vector3 stoneStartPos;
    private Vector3 stoneStartSize;
    private Quaternion stoneStartRotation;
    private List<GameObject> stones = new List<GameObject>();

    // 스턴시 진행중이였던 코루틴 중지
    private Coroutine curCoroutine;

    // 프로퍼티
    public GameObject[] CircleSkillPos { get { return circleSkillPos; } }
    public float Delay { get { return delay; } }
    public float AnimSpd { set { animSpd = value; } }

    private void Start()
    {
        bossStateManager.bossRandomTargetCallback += SetRandomTarget;
        randomTarget = null;
    }

    private void PerformAttack(string _state)
    {
        Debug.Log(_state + "호출됨");
        switch (_state)
        {
            case "Attack1":
                curCoroutine = StartCoroutine(Attack1());
                break;
            case "Attack2":
                curCoroutine = StartCoroutine(Attack2());
                break;
            case "Attack3":
                curCoroutine = StartCoroutine(Attack3());
                break;
            case "Attack4":
                curCoroutine = StartCoroutine(Attack4());
                break;
            case "Attack5":
                curCoroutine = StartCoroutine(Attack5());
                break;
            case "Attack6":
                curCoroutine = StartCoroutine(Attack6());
                break;
            case "Attack7":
                curCoroutine = StartCoroutine(Attack7());
                break;
            case "Attack8":
                curCoroutine = StartCoroutine(Attack8());
                break;
            case "Attack9":
                curCoroutine = StartCoroutine(Attack9());
                break;
            case "Stun":
                StartCoroutine(Stun());
                break;
            case "SpecialAttack":
                curCoroutine = StartCoroutine(SpecialAttack());
                break;
            case "Die":
                StartCoroutine(Die());
                break;
            case "TimeOut":
                StartCoroutine(TimeOut());
                break;
            default:
                break;
        }
    }

    private void Attack8Particle()
    {
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack8, bossStateManager.Boss.transform.position);
    }

    private void Attack4Particle()
    {
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack4, bossStateManager.Boss.transform.position);
    }

    #region [Attack]
    // 휘두르기
    private IEnumerator Attack1()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack1").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        range = range * 2;

        // 스킬위치 조정
        Vector3 bossPosition = bossStateManager.Boss.transform.position;
        Vector3 forwardOffset = bossStateManager.Boss.transform.forward * 2f;
        fanSkillPos.transform.position = new Vector3(bossPosition.x + forwardOffset.x, 0.3f, bossPosition.z + forwardOffset.z);

        // 스킬 각도를 보스 각도에 맞게 설정
        Vector3 targetDirection = bossStateManager.Boss.transform.forward;
        fanSkillPos.transform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        // 스킬 데미지 설정
        fanAttackCollider.GetComponent<BossAttackCollider>().Damage = damage;
        fanAttackCollider.GetComponent<BossAttackCollider>().SkillName = skillName;
        fanAttackCollider.GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        angle = 90f;
        radius = range / 2;
        thickness = 1f;
        segmentCount = 50;

        if (!fanFirstTime)
        {
            Mesh fanMesh = BuildMesh();
            fanAttackCollider.GetComponent<MeshFilter>().mesh = fanMesh;
            fanAttackCollider.GetComponent<MeshCollider>().sharedMesh = fanMesh;
        }

        // 스킬 표시
        fanFullRangeDecal.size = new Vector3(range, range, 1f);

        // 스킬 차는거 표시
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;
            fanChargingRangeDecal.size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            yield return null;
        }
        yield return null;

        // 끝나면 스킬 범위 안보이게
        fanFullRangeDecal.size = new Vector3(0f, 0f, 0f);
        fanChargingRangeDecal.size = new Vector3(0f, 0f, 0f);

        // attackCollider 활성화
        fanAttackCollider.SetActive(true);

        // 파티클 재생
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack1, new Vector3(fanAttackCollider.transform.position.x + forwardOffset.x * 3f, 0.5f, fanAttackCollider.transform.position.z + forwardOffset.z * 3f));

        yield return attackColliderTime;

        // attackCollider 비활성화
        fanAttackCollider.SetActive(false);
    }

    // 럴커패턴
    private IEnumerator Attack2()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack2").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        int cnt = 0;
        stones.Clear();

        // 스킬위치 조정
        foreach (GameObject skillPos in circleSkillPos)
        {
            if (bossStateManager.AlivePlayers[cnt] == null)
            {
                skillPos.transform.position = new Vector3(-100f, 0f, -100f);
                cnt++;
                continue;
            }

            skillPos.transform.position = new Vector3(bossStateManager.AlivePlayers[cnt].transform.position.x, 0.3f, bossStateManager.AlivePlayers[cnt].transform.position.z);

            cnt++;

            if (cnt == bossStateManager.AlivePlayers.Length) break;
        }

        // 스킬 데미지 설정
        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        foreach (GameObject attackCollider in circleAttackColliders)
        {
            attackCollider.transform.localScale = new Vector3(range, 0.5f, range);
            attackCollider.GetComponent<BossAttackCollider>().Damage = damage;
            attackCollider.GetComponent<BossAttackCollider>().SkillName = skillName;
            attackCollider.GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;
        }

        cnt = 0;
        // 스킬 표시 && 돌생성
        foreach (DecalProjector circleFullRangeDecal in circleFullRangeDecals)
        {
            circleFullRangeDecal.size = new Vector3(range, range, 1f);
            stones.Add(Instantiate(P_Stone2, new Vector3(circleFullRangeDecal.transform.position.x, 10f, circleFullRangeDecal.transform.position.z), Quaternion.identity, null));
        }

        // 스킬 차는거 표시
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            // 스킬 표시
            foreach (DecalProjector circleChargingRangeDecal in circleChargingRangeDecals)
            {
                circleChargingRangeDecal.size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);
            }

            // 돌 떨어짐
            foreach (GameObject stone in stones)
            {
                stone.transform.position = new Vector3(stone.transform.position.x, 2f / (elapseTime / delay), stone.transform.position.z);
            }

            yield return null;
        }
        yield return null;

        // 끝나면 스킬 범위 안보이게
        foreach (DecalProjector circleFullRangeDecal in circleFullRangeDecals)
        {
            circleFullRangeDecal.size = new Vector3(0f, 0f, 0f);
        }

        foreach (DecalProjector circleChargingRangeDecal in circleChargingRangeDecals)
        {
            circleChargingRangeDecal.size = new Vector3(0f, 0f, 0f);
        }

        foreach (GameObject stone in stones)
        {
            Destroy(stone);
        }

        // attackCollider 활성화
        foreach (GameObject circleAttackCollider in circleAttackColliders)
        {
            circleAttackCollider.SetActive(true);

            // 파티클 재생
            ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack2, circleAttackCollider.transform.position);
        }

        yield return attackColliderTime;

        // attackCollider 비활성화
        foreach (GameObject circleAttackCollider in circleAttackColliders)
        {
            circleAttackCollider.SetActive(false);
        }
    }

    // 땅에 발구름
    private IEnumerator Attack3()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack3").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        // 스킬위치 조정
        circleSkillPos[0].transform.position = new Vector3(bossStateManager.Boss.transform.position.x, 0.3f, bossStateManager.Boss.transform.position.z);

        // 스킬 데미지 설정
        circleAttackColliders[0].GetComponent<BossAttackCollider>().Damage = damage;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().SkillName = skillName;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        circleAttackColliders[0].transform.localScale = new Vector3(range, 0.5f, range);

        // 스킬 표시
        circleFullRangeDecals[0].size = new Vector3(range, range, 1f);

        // 스킬 차는거 표시
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            circleChargingRangeDecals[0].size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            yield return null;
        }
        yield return null;

        // 사거리 표시 없에기
        circleFullRangeDecals[0].size = new Vector3(0f, 0f, 0f);
        circleChargingRangeDecals[0].size = new Vector3(0f, 0f, 0f);

        // attackCollider 활성화
        circleAttackColliders[0].SetActive(true);

        // 파티클 재생
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack3, circleAttackColliders[0].transform.position, Quaternion.Euler(-90f, 0f, 0f));

        yield return attackColliderTime;

        // attackCollider 비활성화
        circleAttackColliders[0].SetActive(false);
    }

    // 휠윈드
    private IEnumerator Attack4()
    {
        BSD_Duration skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack4").SkillData as BSD_Duration;

        skillName = skill.SkillName;
        damage = skill.Damage;
        duration = skill.Duration;
        knockBackDis = skill.KnockbackDistance;

        // 스킬 데미지 설정
        GetComponent<BossAttackCollider>().Damage = damage;
        GetComponent<BossAttackCollider>().SkillName = skillName;
        GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;


        // 공격 4-1인지 확인
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4-1"))
            {
                break;
            }
            yield return null;
        }

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        GetComponent<BoxCollider>().enabled = true;
        Vector3 originSize = GetComponent<BoxCollider>().size;
        GetComponent<BoxCollider>().size = new Vector3(3f, 3f, 3f);
        bossStateManager.Boss.tag = "BossAttack";

        // ParticleManager.Instance.PlayParticleSetParent(ParticleManager.Instance.attack4, bossStateManager.Boss.transform.position, bossStateManager.Boss, duration + 1f);

        float elapseTime = 0f;

        // 휠윈드 파티클
        attack4.SetActive(true);


        while (true)
        {
            elapseTime += Time.deltaTime;

            // 너무 붙어있으면 안되서 1초마다 껏다킴
            if (elapseTime >= 1f)
            {
                GetComponent<BoxCollider>().enabled = false;
                GetComponent<BoxCollider>().enabled = true;
                elapseTime = 0f;
            }
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Chase"))
            {
                break;
            }
            yield return null;
        }




        // 공격 끝난후
        GetComponent<BoxCollider>().size = originSize;
        GetComponent<BoxCollider>().enabled = false;
        bossStateManager.Boss.tag = "Untagged";
        // 휠윈드 파티클
        attack4.SetActive(false);

        yield return null;
    }

    // 특수 패턴 돌진기 (기본 거리 6)
    private IEnumerator Attack5()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack5").SkillData;

        skillName = skill.SkillName;
        damage = skill.Damage;
        knockBackDis = skill.KnockbackDistance;

        // 스킬 데미지 설정
        GetComponent<BossAttackCollider>().Damage = damage;
        GetComponent<BossAttackCollider>().SkillName = skillName;
        GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        GetComponent<BoxCollider>().enabled = true;
        Vector3 originSize = GetComponent<BoxCollider>().size;
        GetComponent<BoxCollider>().size = new Vector3(2f, 2f, 2f);
        bossStateManager.Boss.tag = "BossAttack";

        // 공격 끝났는지 Check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Chase2") || anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5Jump"))
            {
                break;
            }

            yield return null;
        }

        // 공격 끝난후
        GetComponent<BoxCollider>().size = originSize;
        GetComponent<BoxCollider>().enabled = false;
        bossStateManager.Boss.tag = "Untagged";

        yield return null;
    }

    // 전멸기
    private IEnumerator SpecialAttack()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "SpecialAttack").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        // 스킬위치 조정
        circleSkillPos[0].transform.position = new Vector3(bossStateManager.Boss.transform.position.x, 0.3f, bossStateManager.Boss.transform.position.z);

        // 스킬 데미지 설정
        circleAttackColliders[0].GetComponent<BossAttackCollider>().Damage = damage;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().SkillName = skillName;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        circleAttackColliders[0].transform.localScale = new Vector3(range, 0.5f, range);

        // 스킬 표시
        circleFullRangeDecals[0].size = new Vector3(range, range, 1f);

        // 스킬 차는거 표시
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            circleChargingRangeDecals[0].size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            yield return null;
        }
        yield return null;

        // 사거리 표시 없에기
        circleFullRangeDecals[0].size = new Vector3(0f, 0f, 0f);
        circleChargingRangeDecals[0].size = new Vector3(0f, 0f, 0f);

        // 플레이어 한테 레이쏴서 돌뒤 플레이어 태그 변경
        CheckPlayerBehindRock();

        yield return new WaitForSeconds(0.1f);

        // attackCollider 활성화
        circleAttackColliders[0].SetActive(true);

        // 파티클 재생
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.SpecialAttack, circleAttackColliders[0].transform.position);

        yield return attackColliderTime;

        // attackCollider 비활성화
        circleAttackColliders[0].SetActive(false);

        // 태그 변경
        ResetTag();

        yield return null;
    }

    // 돌던지기
    private IEnumerator Attack6()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack6").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        // 스킬위치 조정
        circleSkillPos[0].transform.position = new Vector3(randomTarget.transform.position.x, 0.3f, randomTarget.transform.position.z);

        // 스킬 데미지 설정
        circleAttackColliders[0].GetComponent<BossAttackCollider>().Damage = damage;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().SkillName = skillName;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        circleAttackColliders[0].transform.localScale = new Vector3(range, 0.5f, range);

        // 스킬 표시
        circleFullRangeDecals[0].size = new Vector3(range, range, 1f);

        // 돌생성
        if (IsServer)
        {
            GameObject stone = Instantiate(P_Stone, rightHand);
            stone.GetComponent<NetworkObject>().Spawn(true);
            curStone = stone;

            stone.transform.SetParent(null);
            stoneStartPos = stone.transform.position;
            stoneStartSize = stone.transform.localScale;
            stoneStartRotation = stone.transform.rotation;
        }


        // 스킬 차는거 표시
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            circleChargingRangeDecals[0].size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            if (IsServer)
            {
                curStone.transform.position = Vector3.Lerp(stoneStartPos, circleSkillPos[0].transform.position, elapseTime / delay);
                curStone.transform.localScale = Vector3.Lerp(stoneStartSize, new Vector3(2f, 2f, 2f), elapseTime / delay);
                curStone.transform.rotation = Quaternion.Lerp(stoneStartRotation, Quaternion.Euler(0f, 0f, 0f), elapseTime / delay);
            }

            yield return null;
        }
        yield return null;

        // 사거리 표시 없에기
        circleFullRangeDecals[0].size = new Vector3(0f, 0f, 0f);
        circleChargingRangeDecals[0].size = new Vector3(0f, 0f, 0f);

        // attackCollider 활성화
        circleAttackColliders[0].SetActive(true);

        // 파티클 재생
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack6, circleAttackColliders[0].transform.position);


        yield return attackColliderTime;

        // attackCollider 비활성화
        circleAttackColliders[0].SetActive(false);

        curStone = null;
    }

    // 보스 점프
    private IEnumerator Attack7()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack7").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        // 스킬위치 조정
        circleSkillPos[0].transform.position = new Vector3(bossStateManager.AggroPlayer.transform.position.x, 0.3f, bossStateManager.AggroPlayer.transform.position.z);

        // 스킬 데미지 설정
        circleAttackColliders[0].GetComponent<BossAttackCollider>().Damage = damage;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().SkillName = skillName;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        circleAttackColliders[0].transform.localScale = new Vector3(range, 0.5f, range);

        // 스킬 표시
        circleFullRangeDecals[0].size = new Vector3(range, range, 1f);

        // 스킬 차는거 표시
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            circleChargingRangeDecals[0].size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            yield return null;
        }
        yield return null;

        // 사거리 표시 없에기
        circleFullRangeDecals[0].size = new Vector3(0f, 0f, 0f);
        circleChargingRangeDecals[0].size = new Vector3(0f, 0f, 0f);

        // attackCollider 활성화
        circleAttackColliders[0].SetActive(true);

        // 파티클 재생
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack7, circleAttackColliders[0].transform.position);

        yield return attackColliderTime;

        // attackCollider 비활성화
        circleAttackColliders[0].SetActive(false);


    }

    // 돌진
    private IEnumerator Attack8()
    {
        BSD_Duration skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack8").SkillData as BSD_Duration;

        skillName = skill.SkillName;
        damage = skill.Damage;
        duration = skill.Duration;
        knockBackDis = skill.KnockbackDistance;

        // 스킬 데미지 설정
        GetComponent<BossAttackCollider>().Damage = damage;
        GetComponent<BossAttackCollider>().SkillName = skillName;
        GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // 돌진 데칼 설정
        recDecalPos.transform.position = bossStateManager.Boss.transform.position + bossStateManager.Boss.transform.forward * 2f;
        recDecalPos.transform.rotation = bossStateManager.Boss.transform.localRotation;
        recDecal.size = new Vector3(0f, 0f, 0f);
        recDecalPos.SetActive(true);
        StartCoroutine(rushDecalCoroutine(0.5f));

        // Attack 8
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack8") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                break;
            }
            yield return null;
        }

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        GetComponent<BoxCollider>().enabled = true;
        Vector3 originSize = GetComponent<BoxCollider>().size;
        GetComponent<BoxCollider>().size = new Vector3(2f, 2f, 2f);
        bossStateManager.Boss.tag = "BossAttack";

        // Attack 8-1
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Chase"))
            {
                break;
            }
            yield return null;
        }

        // 공격 끝난후
        GetComponent<BoxCollider>().size = originSize;
        GetComponent<BoxCollider>().enabled = false;
        bossStateManager.Boss.tag = "Untagged";

        yield return null;
    }

    // 백어택
    private IEnumerator Attack9()
    {
        Debug.Log("Attack9 호출됨");

        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack9").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        range = range * 2;

        // 스킬위치 조정
        Vector3 bossPosition = bossStateManager.Boss.transform.position;
        Vector3 forwardOffset = bossStateManager.Boss.transform.forward * 2f;
        halfCircleSkillPos.transform.position = new Vector3(bossPosition.x, 0.3f, bossPosition.z);

        // 스킬 각도를 보스 각도에 맞게 설정
        Vector3 targetDirection = bossStateManager.Boss.transform.forward;
        halfCircleSkillPos.transform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        // 스킬 데미지 설정
        halfCircleAttackCollider.GetComponent<BossAttackCollider>().Damage = damage;
        halfCircleAttackCollider.GetComponent<BossAttackCollider>().SkillName = skillName;
        halfCircleAttackCollider.GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        angle = 180f;
        radius = range / 2;
        thickness = 1f;
        segmentCount = 50;

        if (!halfCircleFirstTime)
        {
            Mesh fanMesh = BuildMesh();
            halfCircleAttackCollider.GetComponent<MeshFilter>().mesh = fanMesh;
            halfCircleAttackCollider.GetComponent<MeshCollider>().sharedMesh = fanMesh;
        }

        // 스킬 표시
        halfCircleRangeDecal.size = new Vector3(range, range, 1f);

        yield return new WaitForSeconds(0.2f);

        // 스킬 표시사라짐
        halfCircleRangeDecal.size = new Vector3(0f, 0f, 0f);

        // 스킬 차는거 표시
        float elapseTime = 0f;

        while (elapseTime < delay - 0.2f)
        {
            elapseTime += Time.deltaTime;
            yield return null;
        }
        yield return null;

        // attackCollider 활성화
        halfCircleAttackCollider.SetActive(true);

        // 파티클 재생
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack9, new Vector3(halfCircleAttackCollider.transform.position.x + forwardOffset.x * 0.5f, 0.5f, halfCircleAttackCollider.transform.position.z + forwardOffset.z * 0.5f));

        yield return attackColliderTime;

        // attackCollider 비활성화
        halfCircleAttackCollider.SetActive(false);
    }

    // 스턴 걸렸을때
    private IEnumerator Stun()
    {
        // 현재 실행중인 코루틴 종료
        StopCoroutine(curCoroutine);

        // 공격 초기화
        InitAttack();

        // 현재 돌 삭제
        if (IsServer)
        {
            curStone.GetComponent<NetworkObject>().Despawn();
        }

        yield return null;
    }

    // 죽었을때
    private IEnumerator Die()
    {
        // 현재 실행중인 코루틴 종료
        StopCoroutine(curCoroutine);

        // 공격 초기화
        InitAttack();

        yield return null;
    }

    // 시간 끝났을때
    private IEnumerator TimeOut()
    {
        // 돌 전부 제거
        DestroyAllRocks();

        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "SpecialAttack").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        // 스킬위치 조정
        circleSkillPos[0].transform.position = new Vector3(bossStateManager.Boss.transform.position.x, 0.3f, bossStateManager.Boss.transform.position.z);

        // 스킬 데미지 설정
        circleAttackColliders[0].GetComponent<BossAttackCollider>().Damage = damage;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().SkillName = skillName;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        circleAttackColliders[0].transform.localScale = new Vector3(range, 0.5f, range);

        // 스킬 표시
        circleFullRangeDecals[0].size = new Vector3(range, range, 1f);

        // 스킬 차는거 표시
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            circleChargingRangeDecals[0].size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            yield return null;
        }
        yield return null;

        // 사거리 표시 없에기
        circleFullRangeDecals[0].size = new Vector3(0f, 0f, 0f);
        circleChargingRangeDecals[0].size = new Vector3(0f, 0f, 0f);

        yield return new WaitForSeconds(0.1f);

        // attackCollider 활성화
        circleAttackColliders[0].SetActive(true);

        // 파티클 재생
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.SpecialAttack, circleAttackColliders[0].transform.position);


        yield return attackColliderTime;

        // attackCollider 비활성화
        circleAttackColliders[0].SetActive(false);

        yield return null;
    }
    #endregion

    #region [Function]
    // 부채꼴 모양 만듦.
    private Mesh BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ThickFanMesh";

        int vertCount = (segmentCount + 2) * 2; // 상단 + 하단 점
        Vector3[] vertices = new Vector3[vertCount];
        int vertIndex = 0;

        // 상단(Top) 점 계산
        float angleStep = angle / segmentCount * Mathf.Deg2Rad;
        vertices[vertIndex++] = Vector3.zero; // 상단 중심점
        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = i * angleStep;
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            vertices[vertIndex++] = new Vector3(x, y, thickness / 2); // 상단 외곽
        }

        // 하단(Bottom) 점 계산
        vertices[vertIndex++] = new Vector3(0, 0, -thickness / 2); // 하단 중심점
        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = i * angleStep;
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            vertices[vertIndex++] = new Vector3(x, y, -thickness / 2); // 하단 외곽
        }

        // 삼각형 인덱스 배열 크기 계산 (상단 + 하단 + 측면)
        int[] triangles = new int[segmentCount * 6 + segmentCount * 6 + segmentCount * 6];
        int triIndex = 0;

        // 상단 삼각형
        for (int i = 1; i <= segmentCount; i++)
        {
            triangles[triIndex++] = 0; // 중심점
            triangles[triIndex++] = i;
            triangles[triIndex++] = i + 1;
        }

        // 하단 삼각형
        int bottomStart = segmentCount + 2;
        for (int i = 1; i <= segmentCount; i++)
        {
            triangles[triIndex++] = bottomStart; // 중심점
            triangles[triIndex++] = bottomStart + i + 1;
            triangles[triIndex++] = bottomStart + i;
        }

        // 측면 삼각형
        for (int i = 1; i <= segmentCount; i++)
        {
            // 외곽 상단-하단 연결 (앞면)
            int topOuter = i;
            int bottomOuter = bottomStart + i;
            triangles[triIndex++] = topOuter;
            triangles[triIndex++] = bottomOuter;
            triangles[triIndex++] = topOuter + 1;

            triangles[triIndex++] = topOuter + 1;
            triangles[triIndex++] = bottomOuter;
            triangles[triIndex++] = bottomOuter + 1;
        }

        // UV 및 노멀 설정
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / radius + 0.5f, vertices[i].y / radius + 0.5f);
        }

        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = vertices[i].z > 0 ? Vector3.forward : Vector3.back;
        }

        // Mesh 데이터 설정
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;
    }

    // 랜덤 타겟 설정
    private void SetRandomTarget(ulong _index)
    {
        randomTarget = bossStateManager.AlivePlayers.FirstOrDefault(p => p != null && p.GetComponent<NetworkObject>().OwnerClientId == _index);
    }

    // 공격 초기화
    private void InitAttack()
    {
        // 데칼 size 조정
        fanFullRangeDecal.size = new Vector3(0f, 0f, 0f);
        fanChargingRangeDecal.size = new Vector3(0f, 0f, 0f);
        foreach (DecalProjector circleFullRangeDecal in circleFullRangeDecals)
        {
            circleFullRangeDecal.size = new Vector3(0f, 0f, 0f);
        }
        foreach (DecalProjector circleChargingRangeDecal in circleChargingRangeDecals)
        {
            circleChargingRangeDecal.size = new Vector3(0f, 0f, 0f);
        }

        // 공격 콜라이더 끄기
        fanAttackCollider.SetActive(false);
        foreach (GameObject circleAttackCollider in circleAttackColliders)
        {
            circleAttackCollider.SetActive(false);
        }

        // 태그 정상화
        bossStateManager.Boss.tag = "Untagged";
    }

    // 전멸기 돌뒤에 있는지 check
    // 돌 뒤에 있다면 BehindRock으로 tag를 바꿈.
    private void CheckPlayerBehindRock()
    {
        Vector3 bossPos = new Vector3(bossStateManager.Boss.transform.position.x, 0.5f, bossStateManager.Boss.transform.position.z);
        LayerMask defaultLayerMask = LayerMask.GetMask("Player");

        for (int i = 0; i < 4; i++)
        {
            if (bossStateManager.AlivePlayers[i] == null) continue;

            Vector3 playerPos = new Vector3(bossStateManager.AlivePlayers[i].transform.position.x, 0.5f, bossStateManager.AlivePlayers[i].transform.position.z);
            Vector3 dir = (playerPos - bossPos).normalized;

            RaycastHit[] hits = Physics.RaycastAll(bossPos, dir, 100f, defaultLayerMask);

            // Ray 시각적으로 표시
            Debug.DrawRay(bossPos, dir * 100f, Color.green, 2f);

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit ray in hits)
            {
                Debug.LogWarning("레이를 쏜 플레이어 :" + bossStateManager.AlivePlayers[i].name + " || 레이에 맞은 물체 :" + ray.collider.gameObject.name);

                if (ray.collider.CompareTag("Rock"))
                {
                    bossStateManager.AlivePlayers[i].tag = "BehindRock";
                    break;
                }
                else if (ray.collider.gameObject == bossStateManager.AlivePlayers[i])
                {
                    break;
                }
            }
        }

        // 각 플레이어한테 ray를쏴서 돌뒤에 있는지 확인
        //foreach (GameObject player in bossStateManager.Players)
        //{
        //    Vector3 playerPos = new Vector3(player.transform.position.x, 0.5f, player.transform.position.z);
        //    Vector3 dir = (playerPos - bossPos).normalized;

        //    RaycastHit[] hits = Physics.RaycastAll(bossPos, dir, 100f, defaultLayerMask);

        //    System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        //    foreach (RaycastHit ray in hits)
        //    {
        //        if (ray.collider.CompareTag("Rock"))
        //        {
        //            player.tag = "BehindRock";
        //            break;
        //        }
        //        else if (ray.collider.gameObject == player)
        //        {
        //            break;
        //        }
        //    }
        //}
    }

    // 태그 리셋
    private void ResetTag()
    {
        for (int i = 0; i <  bossStateManager.AlivePlayers.Length; i++)
        {
            if (bossStateManager.AlivePlayers[i] == null) continue;

            bossStateManager.AlivePlayers[i].tag = "Player";
        }
    }

    // attack8 데칼 찍는 코루틴
    private IEnumerator rushDecalCoroutine(float _delayTime)
    {
        float elapseTime = 0f;
        float dis = CheckDistanceToWall();

        recDecal.pivot = new Vector3(0f, 0f, dis / 2f);
        Vector3 startSize = new Vector3(0f, 1f, dis);
        Vector3 targetSize = new Vector3(3f, 1f, dis);

        while (true)
        {
            elapseTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapseTime / _delayTime);
            recDecal.size = Vector3.Lerp(startSize, targetSize, t);

            if (elapseTime >= _delayTime)
            {
                break;
            }

            yield return null;
        }

        recDecalPos.SetActive(false);
    }

    // Attack5 데칼 찍는 코루틴2
    private IEnumerator rushDecalCoroutine2(float _delayTime)
    {
        float elapseTime = 0f;
        float dis = CheckDistanceToWall();

        if (dis >= 18f) dis = 18f;

        recDecal.pivot = new Vector3(0f, 0f, dis / 2f);
        Vector3 startSize = new Vector3(0f, 1f, dis);
        Vector3 targetSize = new Vector3(3f, 1f, dis);

        while (true)
        {
            elapseTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapseTime / _delayTime);
            recDecal.size = Vector3.Lerp(startSize, targetSize, t);

            if (elapseTime >= _delayTime)
            {
                break;
            }

            yield return null;
        }

        recDecalPos.SetActive(false);
    }

    public void Attack5Decal()
    {
        // 돌진 데칼 설정
        recDecalPos.transform.position = bossStateManager.Boss.transform.position + bossStateManager.Boss.transform.forward * 2f;
        recDecalPos.transform.rotation = bossStateManager.Boss.transform.localRotation;
        recDecal.size = new Vector3(0f, 0f, 0f);
        recDecalPos.SetActive(true);
        StartCoroutine(rushDecalCoroutine2(0.5f));
    }

    // 벽까지 거리 계산하는 코드
    private float CheckDistanceToWall()
    {
        float distance = 0f;

        int layer = LayerMask.GetMask("Wall");

        // 앞쪽방향 설정
        Vector3 direction = bossStateManager.Boss.transform.forward;
        direction.y = 0f;
        direction = direction.normalized;

        if (Physics.Raycast(bossStateManager.Boss.transform.position, direction, out RaycastHit hit, 100f, layer))
        {
            distance = hit.distance;
        }

        return distance;
    }

    // 돌 전부 제거
    private void DestroyAllRocks()
    {
        if (IsServer)
        {
            GameObject[] rocks = GameObject.FindGameObjectsWithTag("Rock");

            foreach (GameObject rock in rocks)
            {
                rock.GetComponent<NetworkObject>().Despawn();
            }
        }
    }
    #endregion
}