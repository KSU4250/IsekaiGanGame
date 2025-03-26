using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Collections;
using System;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System.Collections;
using Unity.Services.Authentication;


public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }
    private string joinCode;
    private Coroutine keepAliveCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initialized");

            // Relay 초기화 확인
            if (RelayService.Instance == null)
            {
                Debug.LogError("RelayManager: Relay Service 초기화 실패!");
                return;
            }

            Debug.Log("RelayManager: Relay Service 초기화 완료");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
        }
    }

    // 호스트(방장)가 Relay 생성
    public async Task<string> CreateRelay(int maxPlayers)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        try
        {
            Debug.Log("[RelayManager] Relay 서버 생성 요청...");
            // Relay 할당 요청
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1); // 호스트 제외 인원
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"[Relay] 생성된 Join Code: {joinCode}");
            if (string.IsNullOrEmpty(joinCode))
            {
                Debug.LogError("Relay 생성 실패: Join Code가 반환되지 않음.");
                return null;
            }

            Debug.Log($"[RelayManager] Relay 생성 성공 - JoinCode: {joinCode}");

            // Netcode의 UnityTransport에 Relay 데이터 적용
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );


            // 호스트 시작
            //NetworkManager.Singleton.StartHost();
            //Debug.Log("Host started successfully.");

            // Lobby에 RelayJoinCode 업데이트 (기존 AddHostToLobby 코드 제거하고 여기서 직접 실행)
            //if (!string.IsNullOrEmpty(RoomManager.Instance.currentRoom))
            //{
            //    await LobbyService.Instance.UpdateLobbyAsync(RoomManager.Instance.currentRoom, new UpdateLobbyOptions
            //    {
            //        Data = new Dictionary<string, DataObject>
            //    {
            //        { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
            //    }
            //    });

            //    Debug.Log("Lobby에 RelayJoinCode 업데이트 완료");
            //}
            //else
            //{
            //    Debug.LogWarning("Lobby ID가 없습니다. RelayJoinCode를 업데이트할 수 없습니다.");
            //}

            ////  Keep-Alive 시작
            //if (keepAliveCoroutine == null)
            //{
            //    keepAliveCoroutine = StartCoroutine(SendKeepAlive());
            //}

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay 생성 실패: {e.Message}");
            return null;
        }
    }








    // 클라이언트가 Relay에 참가
    // Relay 연결 후 Keep-Alive 시작
    public async Task<bool> JoinRelay(string joinCode)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );

            //NetworkManager.Singleton.StartClient();
            //Debug.Log("Joined Relay successfully!");

            ////  Keep-Alive 시작
            //if (keepAliveCoroutine == null)
            //{
            //    keepAliveCoroutine = StartCoroutine(SendKeepAlive());
            //}

            return NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay 참가 실패: {e.Message}");
            return false;
        }
    }

    // Relay 메시지 브로드캐스트
    public void SendMessageToClients(string message)
    {
        using (FastBufferWriter writer = new FastBufferWriter(256, Allocator.Temp))
        {
            writer.WriteValueSafe(message);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("OnRelayMessage", writer);
        }
        Debug.Log($"[RelayManager] 모든 클라이언트에 메시지 전송: {message}");
    }


    public async Task TransferHostAndLeave(string currentRoom)
    {
        try
        {
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("Host가 나가므로 새 Host를 지정합니다.");

                // 1현재 방의 정보를 가져옴
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentRoom);
                if (lobby.Players.Count > 1)
                {
                    // 남아 있는 첫 번째 플레이어를 새 Host로 지정
                    string newHostId = lobby.Players[1].Id;

                    await LobbyService.Instance.UpdateLobbyAsync(currentRoom, new UpdateLobbyOptions
                    {
                        HostId = newHostId
                    });

                    Debug.Log($"새로운 Host 지정: {newHostId}");
                }
                else
                {
                    // 남아 있는 플레이어가 없다면 방 삭제
                    await LobbyService.Instance.DeleteLobbyAsync(currentRoom);
                    Debug.Log("남아 있는 플레이어가 없어 방이 삭제되었습니다.");
                }

                // 2 네트워크 종료
                NetworkManager.Singleton.Shutdown();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Host 교체 실패: {e.Message}");
        }
    }


    private IEnumerator SendKeepAlive()
    {
        while (true)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.Log("Sending Keep-Alive packet to prevent timeout...");
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("KeepAlive", NetworkManager.ServerClientId, new FastBufferWriter(1, Allocator.Temp));
            }
            yield return new WaitForSeconds(10); // 10초마다 Keep-Alive 패킷 전송
        }
    }
}
