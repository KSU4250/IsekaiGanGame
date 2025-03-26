using Unity.Netcode;
using UnityEngine;

public class GameManagerTest : NetworkBehaviour
{
    public GameObject p_Warrior; // Client 0이 조작할 오브젝트
    public GameObject p_Archor; // Client 1이 조작할 오브젝트
    public GameObject p_Assassin; // Client 2이 조작할 오브젝트
    public GameObject p_Magician; // Client 3이 조작할 오브젝트
    public GameObject p_Golem; // 모든 클라이언트가 볼 수 있는 공용 오브젝트

    private void Start()
    {
        Debug.Log($"[GameManagerTest] Start() 실행됨 - IsServer: {IsServer}, IsClient: {IsClient}");

        if (IsServer) // 서버에서만 실행
        {
            SpawnPlayerControlledObjects();
            SpawnSharedObject();
        }
        else
        {
            Debug.Log("[Client] 서버에서 스폰된 오브젝트를 대기 중...");
        }

        if (IsOwner)
        {
            Debug.Log($"[Game Scene] 내 ClientId: {NetworkManager.Singleton.LocalClientId}");
        }
    }

    private void SpawnPlayerControlledObjects()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong clientId = client.ClientId;

            GameObject playerObject = GetPlayerObjectForClient(clientId);

            if (playerObject != null)
            {
                Debug.Log($"clientId:{clientId}");
                GameObject instance = Instantiate(playerObject);
                NetworkObject networkObject = instance.GetComponent<NetworkObject>();

                if (networkObject != null)
                {
                    Debug.Log($"{clientId}의 오브젝트 : {playerObject.name} 스폰 시작.");
                    // 오브젝트가 스폰 가능한 상태인지 확인
                    if (!networkObject.IsSpawned)
                    {
                        Debug.Log($"[Server] 클라이언트 {clientId}의 전용 오브젝트 {playerObject.name} 스폰 완료.");
                        networkObject.SpawnAsPlayerObject(clientId);
                    }
                    else
                    {
                        Debug.LogError($"[Server] 클라이언트 {clientId}의 오브젝트가 이미 스폰되었습니다!");
                    }
                }
                else
                {
                    Debug.LogError($"[Server] 클라이언트 {clientId}의 오브젝트에 NetworkObject가 없습니다!");
                }
            }
            else
            {
                Debug.LogError($"[Server] 클라이언트 {clientId}에 대한 오브젝트를 찾을 수 없음!");
            }
        }
    }


    private void SpawnSharedObject()
    {
        if (p_Golem == null)
        {
            Debug.LogError("[Server] p_Golem 프리팹이 지정되지 않았습니다!");
            return;
        }

        GameObject sharedInstance = Instantiate(p_Golem);
        
        NetworkObject networkObject = sharedInstance.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            if (!networkObject.IsSpawned)
            {
                networkObject.Spawn();
                Debug.Log("[Server] 공용 오브젝트 C가 스폰되었습니다.");
            }
            else
            {
                Debug.LogError("[Server] 공용 오브젝트 C가 이미 스폰된 상태입니다!");
            }
        }
        else
        {
            Debug.LogError("[Server] 공용 오브젝트 C에 NetworkObject가 없습니다!");
        }
    }


    private GameObject GetPlayerObjectForClient(ulong clientId)
    {
        
        GameObject obj = p_Warrior;

        if (clientId == 0)
        {
            obj = p_Warrior;
        }

        if (clientId == 1)
        {
            obj = p_Archor;
        }
        if (clientId == 2)
        {
            obj = p_Assassin;
        }
        if (clientId == 3)
        {
            obj = p_Magician;
        }

        if (obj == null)
        {
            Debug.LogError($"[Server] 클라이언트 {clientId}에 할당할 프리팹이 존재하지 않습니다! / Warrior 할당");
            obj = p_Warrior;
        }

        return obj;
    }

    //public override void OnNetworkSpawn()
    //{
    //    base.OnNetworkSpawn();

    //    // 클라이언트가 다시 연결되면 소유권 재설정
    //    if (IsOwner)
    //    {
    //        RequestOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
    //    }
    //}
    [ServerRpc]
    private void RequestOwnershipServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
    {
        NetworkObject.ChangeOwnership(clientId);
        Debug.Log($"Client {clientId} 에게 소유권 변경 완료!");
    }
}
