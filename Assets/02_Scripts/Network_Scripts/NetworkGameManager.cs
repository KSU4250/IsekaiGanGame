using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using EnumTypes;

public class NetworkGameManager : NetworkBehaviour
{
    public event Action loadingFinishCallback;
    public event Action gameEndCallback;
    public event UnityAction<ulong> playerDieCallback;

    // 플레이어 로딩 동기화 관련 변수들
    private int playerCnt = 0;
    private int loadingCnt = 0;
    private bool spawnPlayer = false;
    private bool loadingScene = false;

    // 플레이어 생성 관련 변수들
    [SerializeField] private GameObject[] prefabs = new GameObject[4];
    [SerializeField] private Transform[] spwanTr = new Transform[4];
    public GameObject[] players = new GameObject[4];
    private ulong[] objectId = new ulong[4];

    // 사망 관련 변수
    private int playerDieCnt = 0;

    public GameObject[] Players { get { return players; } }
    public GameObject playerObject;

    #region Public Functions

    /// <summary>
    /// 플레이어가 데미지를 받았음을 서버에게 알려주는 RPC를 호출한다.
    /// </summary>
    /// <param name="_damageReceiver"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    public void OnPlayerDamaged
        (PlayerManager _damageReceiver, int _damage)
    {
        ulong clientId = _damageReceiver.PlayerNetworkManager.OwnerClientId;

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            PlayerDamagedServerRpc(clientId, _damage);
        }
    }

    /// <summary>
    /// 플레이어가 넉백 효과를 받았음을 서버에게 알려주는 RPC를 호출한다.
    /// </summary>
    /// <param name="_damageReceiver"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    public void OnPlayerKnockback
        (PlayerManager _target, Vector3 _attackPos, float _knockbackDist)
    {
        ulong clientId = _target.PlayerNetworkManager.OwnerClientId;

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            PlayerKnockbackServerRpc(clientId, _attackPos, _knockbackDist);
        }
    }

    public void OnPlayerDeath(ulong _clientId)
    {
        PlayerDeathServerRpc(_clientId);
    }

    #endregion

    #region [Private Funtions]

    // 프리펩 생성 및 Players 배열에 저장
    private void SpawnPlayerControlledObjects()
    {
        int cnt = 0;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // 임시용 프리펩 설정
            //playerObject = prefabs[cnt];

            // GameObject를 역할에 맞게 설정
            string role = RoleManager.Instance.GetPlayerRole(clientId);
            foreach (GameObject prefab in prefabs)
            {
                if (prefab.name == role) playerObject = prefab;
            }


            players[cnt] = Instantiate(playerObject, spwanTr[cnt].position, Quaternion.identity);

            players[cnt].GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

            cnt++;
        }

        objectId = GetNetworkId(cnt);
    }

    // 생성 네트워크 ID 리턴
    private ulong[] GetNetworkId(int _cnt)
    {
        ulong[] objectIds = new ulong[_cnt];

        _cnt = 0;

        foreach (GameObject player in players)
        {
            if (player == null) continue;

            objectIds[_cnt++] = player.GetComponent<NetworkObject>().NetworkObjectId;
            
        }

        return objectIds;
    }

    // 플레이어 전부 죽었는지 Check => 다 죽었다면 패배 코루틴 실행
    private void CheckPlayerAllDie()
    {
        // 플레이어 전부 사망시 씬넘김
        if (playerDieCnt == NetworkManager.Singleton.ConnectedClients.Count)
        {
            if (IsServer)
            {
                FailClientRpc();
                GameEndClientRpc();
            }
        }
    }

    // 게임 오버시 실행되는 코루틴
    private IEnumerator FailCoroutine()
    {
        yield return new WaitForSeconds(5f);

        FindAnyObjectByType<UIBattleUIManager>().FadeInResult(false);
        FindAnyObjectByType<BgmController>().PlayDefeat();

        // 씬에 있는 버튼명 "GameResultFail" 오브젝트의 자식으로 있는 ResultButton이 클릭됬을때"
        Button failBtn = GameObject.Find("GameResultFail").transform.Find("ResultButton").gameObject.GetComponent<Button>();
        failBtn.onClick.AddListener(ClickFailBtn);

        yield return new WaitForSeconds(15f);

        ClickFailBtn();
    }

    // 게임 클리어시 실행되는 코루틴
    private IEnumerator VictoryCoroutine()
    {
        yield return new WaitForSeconds(5f);

        FindAnyObjectByType<UIBattleUIManager>().FadeInResult(true);
        FindAnyObjectByType<BgmController>().PlayVictory();

        // 씬에 있는 버튼명 "GameResultSuccess" 오브젝트의 자식으로 있는 ResultButton이 클릭됬을때"
        Button successBtn = GameObject.Find("GameResultSuccess").transform.Find("ResultButton").gameObject.GetComponent<Button>();
        successBtn.onClick.AddListener(ClickSucessBtn);

        yield return new WaitForSeconds(15f);

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MushRoomSceneTest", LoadSceneMode.Single);
        }
    }

    // Fail버튼 눌렀을때
    private void ClickFailBtn()
    {
        if (GameManager.Instance.IsGolem)
        {
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("GolemSceneTest", LoadSceneMode.Single);
            }
        }
        else if (GameManager.Instance.IsMush)
        {
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("MushRoomSceneTest", LoadSceneMode.Single);
            }
        }
    }

    private void ClickSucessBtn()
    {
        if (IsServer && GameManager.Instance.IsMush)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("LobbyTest", LoadSceneMode.Single);
        }
        else if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MushRoomSceneTest", LoadSceneMode.Single);
        }
    }

    #endregion

    #region RPC

    #region Client To Server RPC

    /// <summary>
    /// 플레이어가 데미지를 받았음을 서버에게 알린다.
    /// </summary>
    /// <param name="_clientId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Server)]
    private void PlayerDamagedServerRpc
        (ulong _clientId, int _damage)
    {
        // 서버에서 다른 클라이언트들에게 특정 플레이어에게 데미지를 적용하라고 명령한다.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
        ApplyDamageToPlayerClientRpc(_clientId, _damage);
    }

    /// <summary>
    /// 플레이어가 넉백상태임을 서버에게 알린다.
    /// </summary>
    /// <param name="_clientId"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Server)]
    private void PlayerKnockbackServerRpc(ulong _clientId, Vector3 _attackPos, float _knockbackDist)
    {
        ApplyKnockbackClientRpc(_clientId, _attackPos, _knockbackDist);
    }

    [Rpc(SendTo.Server)]
    public void PlayerDeathServerRpc(ulong _clientId)
    {
        Debug.LogWarningFormat("Server Death!!! : {0}", _clientId);
        PlayerDieClientRpc(_clientId);
    }

    #endregion

    #region Server To Client RPC

    /// <summary>
    /// 서버에서 특정 플레이어에게 데미지를 적용하도록 모든 클라이언트에게 명령한다.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Everyone)]
    private void ApplyDamageToPlayerClientRpc(ulong _cliendId, int _damage)
    {
        var obj = NetworkManager.ConnectedClients[_cliendId].PlayerObject;

        if(obj != null)
        {
            GameManager.Instance.ApplyDamageToPlayer(obj.GetComponent<PlayerManager>(), _damage);
        }

    }

    /// <summary>
    /// 서버에서 특정 플레이어에게 넉백 효과를 부여하도록 모든 클라이언트에게 명령한다.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Everyone)]
    private void ApplyKnockbackClientRpc(ulong _cliendId, Vector3 _attackPos, float _knockbackDist)
    {
        var obj = NetworkManager.ConnectedClients[_cliendId].PlayerObject;

        if (obj != null)
        {
            GameManager.Instance.ApplyKnockbackToPlayer
                (obj.GetComponent<PlayerManager>(), _attackPos, _knockbackDist);
        }
    }

        #endregion

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        if (GameManager.Instance.IsLocalGame)
            return;

        if (IsServer)
        {
            if (GameManager.Instance.IsGolem)
            {
                BossStateManager Boss = FindAnyObjectByType<BossStateManager>();

                if (Boss != null)
                {
                    Boss.bossDieCallback += VictoryClientRpc;
                    Boss.bossDieCallback += GameEndClientRpc;
                }
            }
            else if (GameManager.Instance.IsMush)
            {
                MushStateManager Mush = FindAnyObjectByType<MushStateManager>();

                if (Mush != null)
                {
                    Mush.bossDieCallback += VictoryClientRpc;
                    Mush.bossDieCallback += GameEndClientRpc;
                }
            }
        }

       LoadingCheckServerRpc();

        if (IsServer)
        {
            SpawnPlayerControlledObjects();
        }
    }

    private void Update()
    {
        if (GameManager.Instance.IsLocalGame)
            return;

        if (spawnPlayer && loadingScene)
        {
            SynPlayerClientRpc(objectId);
            LoadingFinishClientRpc();
            SetPlayerDieCallbackClientRpc();
            spawnPlayer = false;
            loadingScene = false;
        }
    }

    #endregion

    #region [ServerStart]

    // 플레이어 전부 네트워크 상에서 스폰됬는지 확인하는 함수
    [ServerRpc(RequireOwnership = false)]
    public void CheckPlayerSpawnServerRpc()
    {
        playerCnt++;

        if (playerCnt == NetworkManager.Singleton.ConnectedClients.Count)
        {
            spawnPlayer = true;
        }
    }

    // 플레이어 전부 네트워크 상에서 씬로딩이 됬는지 확인하는 함수
    [ServerRpc(RequireOwnership = false)]
    private void LoadingCheckServerRpc()
    {
        loadingCnt++;

        if (loadingCnt == NetworkManager.Singleton.ConnectedClients.Count)
        {
            loadingScene = true;
        }
    }

    // 로딩이 끝나면 모든 클라이언트에 실행이 끝났다고 콜백이 됨.
    [ClientRpc]
    private void LoadingFinishClientRpc()
    {
        loadingFinishCallback?.Invoke();
    }

    // 플레이어 사망 콜백 등록
    [ClientRpc]
    private void SetPlayerDieCallbackClientRpc()
    {
        for (int i = 0; i < 4; i++)
        {
            if (players[i] == null) continue;

            players[i].GetComponent<PlayerNetworkManager>().OnNetworkPlayerDeath += OnPlayerDeath;
        }
    }

    #endregion

    #region [ClientRpc]

    [ClientRpc]
    private void SynPlayerClientRpc(ulong[] _networkObjectIds)
    {
        int cnt = 0;

        foreach (ulong clientId in _networkObjectIds)
        {
            players[cnt] = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_networkObjectIds[cnt]].gameObject;

            Debug.Log("SetPlayer : " + players[cnt]);

            cnt++;
        }
    }

    // 플레이어 죽었을때 호출(모든 클라에 호출해야함. -> 모든 클라가 플레이어 정보를 가져야함)
    [ClientRpc]
    private void PlayerDieClientRpc(ulong _clientId)
    {
        foreach(var playerObj in Players)
        {
            if(playerObj.GetComponent<PlayerManager>().PlayerNetworkManager.OwnerClientId
                == _clientId)
            {
                Debug.LogFormat("Died Client Id : {0}", _clientId);
                Debug.LogFormat("Character Class : {0}", playerObj.GetComponent<PlayerManager>().PlayerData.characterClass);

                if(GameManager.Instance.IsGolem)
                {
                    GameManager.Instance.BattleLog.SetKillLog(BossType.Golem, 
                        playerObj.GetComponent<PlayerManager>().PlayerData.characterClass);
                }
                else
                {
                    GameManager.Instance.BattleLog.SetKillLog(BossType.Mushroom,
                        playerObj.GetComponent<PlayerManager>().PlayerData.characterClass);
                }

                GameManager.Instance.BattleLog.ShowLog();

                break;
            }
        }

        if (IsServer)
        {
            playerDieCallback?.Invoke(_clientId);
            playerDieCnt++;
            CheckPlayerAllDie();
        }
    }

    // 플레이어 다 죽었을때 클라에 호출됨
    [ClientRpc]
    private void FailClientRpc()
    {
        StartCoroutine(FailCoroutine());
    }

    // 보스 죽였을때 클라에 호출
    [ClientRpc]
    private void VictoryClientRpc()
    {
        StartCoroutine(VictoryCoroutine());
    }

    // 게임 끝났을때, 클라에 호출
    [ClientRpc]
    private void GameEndClientRpc()
    {
        gameEndCallback?.Invoke();
    }
    #endregion
}
