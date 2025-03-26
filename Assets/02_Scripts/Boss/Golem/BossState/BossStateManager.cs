using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Linq;



public class BossStateManager : NetworkBehaviour
{
    // 델리게이트
    public delegate void BossStateDelegate();
    public delegate void BossStateDelegate2(ulong _index);
    public BossStateDelegate bossDieCallback;
    public BossStateDelegate bossHp10Callback;
    public BossStateDelegate bossHpHalfCallback;
    public BossStateDelegate bossStunCallback;
    public BossStateDelegate bossWallTriggerCallback;
    public BossStateDelegate bossTimeOutCallback;
    public BossStateDelegate2 bossRandomTargetCallback;

    // 프로퍼티
    public GameObject Boss { get { return boss; } }
    public GameObject AggroPlayer { get { return aggroPlayer; } }
    public GameObject BossSkin { get { return bossSkin; } }
    public GameObject[] AlivePlayers { get { return alivePlayers; } }
    public BoxCollider HitCollider { get { return hitCollider; } }

    // 보스 상태 관련 변수들
    public List<BossChain> activeChain = new List<BossChain>();
    public bool[] hpCheck = new bool[9];
    public GameObject randomTarget;
    public bool isPhase2 = false;
    public float chainTime = 0f;
    public float reduceAggro = 5f;
    public float reduceAggroTime = 10f;
    public GameObject[] allPlayers;
    public GameObject[] alivePlayers;
    public GameObject aggroPlayer;

    // 네트워크로 동기화 할것들
    public NetworkVariable<float> bestAggro;
    public NetworkVariable<int> aggroPlayerIndex = new NetworkVariable<int>(-1);
    public NetworkVariable<int> curHp = new NetworkVariable<int>(-1);
    public NetworkVariable<int> maxHp = new NetworkVariable<int>(25000);
    public NetworkList<float> playerDamage = new NetworkList<float>();
    public NetworkList<float> playerAggro = new NetworkList<float>();

    // 가져와서 넣는거
    public DamageParticle damageParticle;
    public UIBossHpsManager bossHpUI;
    public BgmController bgmController;
    public BossAttackCollider attackCollider;
    public BossBT bossBT;
    public GameObject boss;
    public GameObject bossSkin;
    public BoxCollider hitCollider;
    public FollowCamera followCam;
    public HitMaterial hitMaterial;

    private void Awake()
    {
        FindAnyObjectByType<NetworkGameManager>().playerDieCallback += PlayerDieCallback;
        FindAnyObjectByType<NetworkGameManager>().loadingFinishCallback += () => 
        {
            InitMulti();
            Invoke("ChangeBossState", 3f);
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall")
        {
            bossWallTriggerCallback?.Invoke();
            Debug.Log("벽이랑 부딛침");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            BossDamageReceiveServerRpc(100, 999, 0);
        }
    }

    // 서버에서만 데미지 받는 함수 실행
    [ServerRpc(RequireOwnership = false)]
    public void BossDamageReceiveServerRpc(ulong _clientId, int _damage, float _aggro)
    {
        Debug.Log("데미지 받음 실행됨");

        if (curHp.Value <= 0) return;

        // 데미지, 어그로 세팅(공유되는 변수에 저장)
        RegisterDamageAndAggro(_clientId, _damage, _aggro);

        // 보스에 상태이상 추가(공유되는 변수에 저장)
        // AddChainList();

        // 데미지를 입힘(공유되는 변수에 저장)
        TakeDamage(_damage);

        // 클라이언트 모두 어그로 플레이어 설정하는 함수 (네트워크에서 계산후 쏴줌)
        if (IsServer)
        {
            GetHighestAggroTarget();
        }

        // 클라이언트 모두 보스 피격 파티클 실행
        DamageParticleClientRpc(_damage, _clientId);

        // 데미지 피격 mat 설정
        DamageMatClientRpc(_clientId);

        // 클라이언트 모두 보스 UI설정
        UpdateBossUIClientRpc(_damage);

        // 클라이언트 모두 보스 브금 설정
        ChangeExcitedLevelClientRpc();

        // 플레이어 카메라 흔들림 실행
        ShakePlayerCamClientRpc(_clientId);

        // 맞는 오디오 재생
        GetHitSoundClientRpc(_clientId);

        // 서버만 hp콜백(현재 피에 따라 패턴 설정)
        CheckHpCallback();
    }

    #region [ClientRPC]
    // aggroPlayer 변경
    [ClientRpc]
    private void SetAggroPlayerClientRpc(int _num)
    {
        aggroPlayer = alivePlayers[_num];
    }

    // 데미지 파티클 실행
    [ClientRpc]
    private void DamageParticleClientRpc(float _damage, ulong _clientId)
    {
        // 데미지 폰트
        if (NetworkManager.Singleton.LocalClientId == _clientId)
        {
            // 데미지 파티클
            damageParticle.SetupAndPlayParticlesMine(_damage);

            // 히트 파티클
            Vector3 pos = new Vector3(Boss.transform.position.x, 3f, Boss.transform.position.z);
            ParticleManager.Instance.PlayParticle(ParticleManager.Instance.hitParticle, pos);
        }
        else
        {
            damageParticle.SetupAndPlayParticles(_damage);
        }
    }

    // 보스 UI 업데이트
    [ClientRpc]
    private void UpdateBossUIClientRpc(int _damage)
    {
        bossHpUI.BossDamage(_damage);
        bossHpUI.HpBarUIUpdate();
    }

    // ExcitedLevel 변경
    [ClientRpc]
    private void ChangeExcitedLevelClientRpc()
    {
        bgmController.ExcitedLevel(ChangeHpToExciteLevel());
    }

    [ClientRpc]
    private void RandomPlayerClientRpc(ulong _num)
    {
        bossRandomTargetCallback?.Invoke(_num);
    }

    [ClientRpc]
    private void SetAlivePlayerClientRpc(int _index)
    {
        alivePlayers[_index] = null;
    }

    [ClientRpc]
    private void ShakePlayerCamClientRpc(ulong _clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == _clientId)
        {
            // 카메라 흔들리게 설정
            StartCoroutine(followCam.ShakeCam());
        }
    }

    // 데미지 히트 mat 실행
    [ClientRpc]
    private void DamageMatClientRpc(ulong _clientId)
    {
        // 데미지 폰트
        if (NetworkManager.Singleton.LocalClientId == _clientId)
        {
            StartCoroutine(hitMaterial.ChangeMat());
        }
    }

    // 최대체력으로 다같이 세팅
    [ClientRpc]
    private void ResetBossUIClientRpc(int _value)
    {
        bossHpUI.SetMaxHp(_value);
        bossHpUI.HpBarUIUpdate();
    }
    #endregion

    #region [BossState]
    // chain 상태를 관리하는 List에 저장
    private void AddChainList(BossChain _chainType)
    {
        activeChain.Add(_chainType);
        StartCoroutine(RemoveChainList(_chainType));
    }

    // chain 특정 시간후 제거
    private IEnumerator RemoveChainList(BossChain _chainType)
    {
        yield return new WaitForSeconds(chainTime);

        if (activeChain.Contains(_chainType))
        {
            activeChain.Remove(_chainType);
        }
    }

    // Chain 상태 확인
    public bool HasChain(BossChain _chainType)
    {
        return activeChain.Contains(_chainType);
    }

    //보스가 스턴걸렸을때 호출되는 함수
    public void BossStun()
    {
        bossStunCallback?.Invoke();
    }

    // 특정 hp이하일때 마다 콜백을 던짐
    private void CheckHpCallback()
    {
        float hp = ((float)curHp.Value / (float)maxHp.Value) * 100f;

        Debug.Log("현재 hp" + hp);

        if (hp <= 90f && !hpCheck[0])
        {
            hpCheck[0] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 80f && !hpCheck[1])
        {
            hpCheck[1] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 70f && !hpCheck[2])
        {
            hpCheck[2] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 60f && !hpCheck[3])
        {
            hpCheck[3] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 50f && !hpCheck[4])
        {
            hpCheck[4] = true;
            bossHpHalfCallback?.Invoke();
        }
        else if (hp <= 40f && !hpCheck[5])
        {
            hpCheck[5] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 30f && !hpCheck[6])
        {
            hpCheck[6] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 20f && !hpCheck[7])
        {
            hpCheck[7] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 10f && !hpCheck[8])
        {
            hpCheck[8] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
    }
    #endregion

    #region [Damage && Aggro]
    // 어그로 수치 초기화 하는 함수
    public void ResetAggro()
    {
        for (int i = 0; i < playerAggro.Count; i++)
        {
            playerAggro[i] = 0f;
        }

        bestAggro.Value = 0f;

        GetHighestAggroTarget();
    }

    // 플레이어의 데미지, 어그로 수치 관리하는 함수
    private void RegisterDamageAndAggro(ulong _clientId, int _damage, float _aggro)
    {
        if (_clientId == 100) return;

        for (int i = 0; i < alivePlayers.Length; i++)
        {
            if (alivePlayers[i] == null) continue;

            if (alivePlayers[i].GetComponent<NetworkObject>().OwnerClientId == _clientId)
            {
                playerDamage[i] += _damage;
                playerAggro[i] += _aggro;
            }
        }
    }

    // 현재 어그로 수치 기준으로 어그로 대상 판별하는 함수
    private void GetHighestAggroTarget()
    {
        bool allAggroZero = true;

        // 전부 어그로 0일때
        foreach (float aggro in playerAggro)
        {
            if (aggro != 0f)
            {
                allAggroZero = false;
            }
        }

        // 전부 어그로 0일때 -> 랜덤 1명 아무나 aggro 10으로 만들고 aggroPlayer 상태로
        if (allAggroZero)
        {
            // 랜덤 플레이어 설정
            ulong randomIndex = RandomPlayer();
            GameObject randomPlayer = alivePlayers.FirstOrDefault(p => p != null && p.GetComponent<NetworkObject>().OwnerClientId == randomIndex);

            for (int i = 0; i < 4; i++)
            {
                if (alivePlayers[i] == null) continue;

                if (alivePlayers[i] == randomPlayer)
                {
                    playerAggro[i] = 10f;
                    bestAggro.Value = playerAggro[i];
                    aggroPlayerIndex.Value = i;
                }
            }

            SetAggroPlayerClientRpc(aggroPlayerIndex.Value);
            return;
        }

        // 기존의 어그로 왕보다 어그로가 1.2배 크면 어그로 바뀜
        for (int i = 0; i < alivePlayers.Length; i++)
        {
            if (bestAggro.Value * 1.2f <= playerAggro[i])
            {
                bestAggro.Value = playerAggro[i];
                aggroPlayerIndex.Value = i;
            }
        }

        // 어그로 플레이어 변경(클라 모두)
        SetAggroPlayerClientRpc(aggroPlayerIndex.Value);
    }

    // 데미지 받는 함수
    private void TakeDamage(int _damage)
    {
        curHp.Value -= _damage;

        if (curHp.Value <= 0)
        {
            curHp.Value = 0;
            bossDieCallback?.Invoke();
        }
    }

    // 어그로 수치 감소시키는 코루틴
    private IEnumerator ReduceAggroCoroutine()
    {
        float elapseTime = 0f;

        while (true)
        {
            elapseTime += Time.deltaTime;

            if (elapseTime >= reduceAggroTime)
            {
                for (int i = 0; i < 4; ++i)
                {
                    playerAggro[i] -= reduceAggro;

                    if (playerAggro[i] <= 0)
                    {
                        playerAggro[i] = 0;
                    }
                }

                bestAggro.Value -= reduceAggro;

                if (bestAggro.Value <= 0) bestAggro.Value = 0;

                elapseTime = 0f;
            }
            yield return null;
        }
    }
    #endregion

    #region [BGM]
    // hp 1페이즈일때 0~1, 2페이즈 일때 0~1
    private float ChangeHpToExciteLevel()
    {
        float hp = ((float)curHp.Value / (float)maxHp.Value);

        if (!isPhase2)
        {
            if (((1 - hp) * 2) >= 1f) return 1f;

            return (1 - hp) * 2;
        }
        else
        {
            return 1 - hp * 2;
        }
    }

    // 페이즈 바뀔때 브금 바꾸고 ExcitedLevel 바꿈.
    private void ChangePhase2BGM()
    {
        bgmController.ExcitedLevel(0);
        bgmController.PlayBossRageBgm();
        isPhase2 = true;
    }

    // 맞는 오디오 재생
    [ClientRpc]
    private void GetHitSoundClientRpc(ulong _clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == _clientId)
        {
            BossAudioManager.Instance.AudioPlay(BossAudioManager.Instance.GetHit);
        }
    }
    #endregion

    #region [Init]

    // 초기화 해야할것들
    private void Init()
    {
        for (int i = 0; i < hpCheck.Length; i++)
        {
            hpCheck[i] = false;
        }

        // 참조 가져오기
        boss = transform.gameObject;
        bossSkin = boss.transform.GetChild(0).gameObject;
        hitCollider = boss.transform.GetChild(1).GetComponent<BoxCollider>();
        attackCollider = GetComponent<BossAttackCollider>();
        damageParticle = FindFirstObjectByType<DamageParticle>();
        bossHpUI = FindFirstObjectByType<UIBossHpsManager>();
        bgmController = FindFirstObjectByType<BgmController>();
        bossBT = FindAnyObjectByType<BossBT>();
        followCam = FindAnyObjectByType<FollowCamera>();
        hitMaterial = FindFirstObjectByType<HitMaterial>();
    }

    // 멀티에서 초기화 해야할것들
    private void InitMulti()
    {
        if (IsServer)
        {
            maxHp.Value = 25000;

            if (FindAnyObjectByType<SetBossHp>() != null && FindAnyObjectByType<SetBossHp>().GetBossHP() != 0)
            {
                maxHp.Value = FindAnyObjectByType<SetBossHp>().GetBossHP();
            }

            for (int i = 0; i < 4; i++)
            {
                playerDamage.Add(0f);
                playerAggro.Add(0f);
            }

            curHp.Value = maxHp.Value;
            aggroPlayerIndex.Value = 0;
            bestAggro.Value = 0f;
        }

        Init();

        SetCallback();

        SetPlayerMulti();

        if (IsServer)
        {
            StartCoroutine(ReduceAggroCoroutine());
            StartCoroutine(CheckTimeOut());
            ResetBossUIClientRpc(maxHp.Value);
        }
    }

    // 콜백 설정
    private void SetCallback()
    {
        BossBT.SpecialAttackEndCallback += ResetAggro;
        attackCollider.rockCollisionCallback += BossStun;
        bossBT.phase2BehaviorStartCallback += ChangePhase2BGM;
    }

    // 멀티 플레이어 설정
    private void SetPlayerMulti()
    {
        allPlayers = FindFirstObjectByType<NetworkGameManager>().Players;
        alivePlayers = (GameObject[])allPlayers.Clone();

        // 초반 aggro 0이여서 세팅하는 함수
        if (IsServer)
        {
            GetHighestAggroTarget();
        }
    }

    // 보스 상태 변경
    private void ChangeBossState()
    {
        bossBT.curState = BossState.Chase;
    }
    #endregion

    #region [Etc.]
    // 보스와 어그로 플레이어 사이의 거리 계산
    public float GetDisWithoutY()
    {
        Vector2 bossPos2D = new Vector2(boss.transform.position.x, boss.transform.position.z);
        Vector2 playerPos2D = new Vector2(alivePlayers[aggroPlayerIndex.Value].transform.position.x, alivePlayers[aggroPlayerIndex.Value].transform.position.z);

        return Vector2.Distance(bossPos2D, playerPos2D);
    }

    // 랜덤한 플레이어를 호출하는 함수
    public ulong RandomPlayer()
    {
        List<ulong> numList = new List<ulong>();
        ulong randomNum = 0;

        for (int i = 0; i < 4; ++i)
        {
            if (alivePlayers[i] == null) continue;

            numList.Add(alivePlayers[i].GetComponent<NetworkObject>().OwnerClientId);
        }

        if (numList.Count != 0)
        {
            randomNum = numList[Random.Range(0, numList.Count)];
        }
        else
        {
            randomNum = 0;
        }


        return randomNum;
    }

    // 플레이어가 죽었을때 호출되는 함수
    private void PlayerDieCallback(ulong _clientId)
    {
        Debug.LogWarning("해당 플레이어가 죽음 : " + _clientId);

        int playerIndex = -1;
        bool IsAggroDie = false;

        // 죽은 플레이어의 Aggro수치 리셋 && Player리스트에서 빼기
        for (int i = 0; i < 4; ++i)
        {
            if (alivePlayers[i] == null) continue;

            if (alivePlayers[i].GetComponent<NetworkObject>().OwnerClientId == _clientId)
            {
                // 만약 죽은 플레이어가 bestAggro였다면 베스트 어그로도 초기화
                if (i == aggroPlayerIndex.Value)
                {
                    bestAggro.Value = 0f;
                    IsAggroDie = true;
                }
                playerAggro[i] = 0f;
                alivePlayers[i] = null;
                playerIndex = i;

                // Aggro플레이어가 죽었다면, bestAggro 재설정
                if (IsAggroDie)
                {
                    // bestAggro 재설정
                    for (int j = 0; j < alivePlayers.Length; j++)
                    {
                        if (bestAggro.Value <= playerAggro[j])
                        {
                            bestAggro.Value = playerAggro[j];
                            aggroPlayerIndex.Value = j;
                        }
                    }
                }
            }
        }

        // alivePlayer 동기화
        SetAlivePlayerClientRpc(playerIndex);

        // 어그로 플레이어 변경(클라 모두)
        SetAggroPlayerClientRpc(aggroPlayerIndex.Value);

        // 어그로 재설정을 위한 어그로 세팅 함수 호출
        GetHighestAggroTarget();
    }

    // 플레이어 살아났을때 호출
    private void PlayerReviveCallback(ulong _clientId)
    {
        // 죽은 플레이어를 alivePlayers배열에 추가
        for (int i = 0; i < 4; ++i)
        {
            if (allPlayers[i] == null) continue;

            if (allPlayers[i].GetComponent<NetworkObject>().OwnerClientId == _clientId)
            {
                alivePlayers[i] = allPlayers[i];
            }
        }
    }

    // 타임아웃 됬을때 호출되는 함수
    private void TimeOutCallback()
    {
        bossTimeOutCallback?.Invoke();
    }

    // 타임아웃 체크하는 코루틴
    private IEnumerator CheckTimeOut()
    {
        float elapseTime = 0f;

        while (true)
        {
            elapseTime += Time.deltaTime;

            if (elapseTime >= 600)
            {
                TimeOutCallback();
                break;
            }

            yield return null;
        }
    }
    #endregion


}