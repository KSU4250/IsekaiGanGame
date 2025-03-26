using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class PersistentNetworkManager : MonoBehaviour
{
    public static PersistentNetworkManager Instance { get; private set; }

    private Dictionary<ulong, string> clientIdToPlayerId = new Dictionary<ulong, string>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 NetworkManager 유지
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[PersistentNetworkManager] 클라이언트 연결됨 - clientId: {clientId}");

        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"[PersistentNetworkManager] 서버 감지됨. Host ClientId: {NetworkManager.Singleton.LocalClientId}");
            clientId = 0; // Host는 항상 ClientId 0으로 설정
        }

        string playerId = AuthenticationService.Instance.PlayerId;

        if (!clientIdToPlayerId.ContainsKey(clientId))
        {
            clientIdToPlayerId[clientId] = playerId;
            Debug.Log($"[PersistentNetworkManager] 등록 완료 - clientId: {clientId}, PlayerId: {playerId}");
        }

        // 클라이언트에게 playerId 동기화
        SendPlayerIdToClientRpc(clientId, playerId);
    }



    [ClientRpc]
    private void SendPlayerIdToClientRpc(ulong clientId, string playerId)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            clientIdToPlayerId[clientId] = playerId;
            Debug.Log($"[PersistentNetworkManager] 클라이언트에서 PlayerId 업데이트 완료 - clientId: {clientId}, PlayerId: {playerId}");
        }
    }



    private void OnClientDisconnected(ulong clientId)
    {
        if (clientIdToPlayerId.ContainsKey(clientId))
        {
            clientIdToPlayerId.Remove(clientId);
            Debug.Log($"[PersistentNetworkManager] 클라이언트 연결 해제 - clientId: {clientId}");
        }
    }

    public void RegisterPlayer(ulong clientId, string playerId)
    {
        if (!clientIdToPlayerId.ContainsKey(clientId))
        {
            clientIdToPlayerId[clientId] = playerId;
            Debug.Log($"[PersistentNetworkManager] 등록 완료 - clientId: {clientId}, PlayerId: {playerId}");
        }
    }

    public string GetPlayerId(ulong clientId)
    {
        if (clientIdToPlayerId.TryGetValue(clientId, out string playerId))
        {
            return playerId;
        }

        Debug.LogError($"[PersistentNetworkManager] GetPlayerId() 실패: clientId {clientId}에 해당하는 PlayerId 없음.");
        return "Unknown";
    }

}
