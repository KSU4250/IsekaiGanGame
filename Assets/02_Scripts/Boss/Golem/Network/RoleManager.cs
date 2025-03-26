using System.Collections.Generic;
using System.Data;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

public class RoleManager : NetworkBehaviour
{
    public static RoleManager Instance;

    private string selectedCharacter = "-1"; // 기본값 (-1: 선택하지 않음)

    public string SelectedCharacter => selectedCharacter; // selectedCharacter의 Getter

    private string currentLobbyId => RoomManager.Instance?.currentLobby?.Id;

    private Dictionary<ulong, string> playerRoles = new Dictionary<ulong, string>();

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

    // 캐릭터 선택 시 서버에 업데이트
    public async void SelectCharacter(int characterIndex)
    {
        if (string.IsNullOrEmpty(currentLobbyId))
        {
            Debug.LogError("[RoleManager] 로비가 존재하지 않습니다!");
            return;
        }

        string playerId = AuthenticationService.Instance.PlayerId;

        if (characterIndex == -1)
        {
            Debug.Log($"[RoleManager] 플레이어 {playerId} 캐릭터 선택 해제 (-1)");
        }
        else
        {
            Debug.Log($"[RoleManager] 플레이어 {playerId} -> 캐릭터 {characterIndex} 선택");
        }

        try
        {
            var options = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "CharacterSelection", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, characterIndex.ToString()) }
                }
            };

            await LobbyService.Instance.UpdatePlayerAsync(currentLobbyId, playerId, options);
            Debug.Log($"[RoleManager] 플레이어 {playerId} 캐릭터 선택 업데이트 완료: {characterIndex}");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"[RoleManager] 캐릭터 선택 업데이트 실패: {ex.Message}");
        }
    }


    // 플레이어 역활과 id를 매칭해서 저장하는 함수
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerRoleServerRpc(ulong _clientId, string _role)
    {
        Debug.Log($"[Server] SetPlayerRoleServerRpc 호출됨 - ClientID: {_clientId}, Role: {_role}");
        if (!playerRoles.ContainsKey(_clientId))
        {
            playerRoles.Add(_clientId, _role);
            Debug.Log($"[Server] {playerRoles[_clientId]} 역할로 설정 완료.");
        }
    }

    // 저장된 정보 리셋시키는 함수
    [ServerRpc(RequireOwnership = false)]
    public void ResetPlayerRoleServerRpc(ulong _clientId)
    {
        if (playerRoles.ContainsKey(_clientId))
        {
            playerRoles.Remove(_clientId);
        }
    }


    // 플레이 씬 넘어가서 clientId에 맞는 플레이어 역할 가져오는 함수
    public string GetPlayerRole(ulong clientId)
    {
        return playerRoles.ContainsKey(clientId) ? playerRoles[clientId]: "None";
    }

    public void ClickWarrior()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Warrior");

        Lobby_CharacterSelector.Instance.SelectCharacter(0);
    }

    public void ClickArcher()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Archer");

        Lobby_CharacterSelector.Instance.SelectCharacter(1);
    }

    public void ClickAssassin()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Assassin");

        Lobby_CharacterSelector.Instance.SelectCharacter(2);
    }

    public void ClickMagician()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Magician");

        Lobby_CharacterSelector.Instance.SelectCharacter(3);
    }


    // 직업 선택 되돌리기
    public void ResetRole()
    {
        Debug.Log("클라 아이디 : " + NetworkManager.Singleton.LocalClientId);
        ResetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId);
    }
}
