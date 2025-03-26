using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListManager : MonoBehaviour
{
    public static PlayerListManager Instance { get; private set; }

    [SerializeField] private Transform playerListParent; // PlayerItem들이 배치될 부모 오브젝트
    [SerializeField] private GameObject playerItemPrefab; // PlayerItem 프리팹

    private Dictionary<string, PlayerItem> playerItems = new Dictionary<string, PlayerItem>(); // 플레이어 목록

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPlayer(string playerId, string username, PlayerStatus status, int selectedCharacter = -1)
    {
        Debug.Log($"[PlayerListManager] AddPlayer 호출됨 - {username} / {status}");

        if (playerItems.ContainsKey(playerId))
        {
            Debug.Log($"[PlayerListManager] 이미 존재하는 플레이어: {playerId}");
            return; // 중복 추가 방지
        }

        GameObject newItem = Instantiate(playerItemPrefab, playerListParent); //PlayerList에 추가
        PlayerItem playerItem = newItem.GetComponent<PlayerItem>();

        if (playerItem == null)
        {
            Debug.LogError("[PlayerListManager] PlayerItem 컴포넌트가 없음!");
            return;
        }
        playerItem.SetPlayerInfo(playerId, username, status);
        playerItem.SetCharacterSelection(selectedCharacter); // 선택한 캐릭터 설정
        playerItems.Add(playerId, playerItem);
        Debug.Log($"[PlayerListManager] Player 추가 완료: {playerId} {username} 선택한 캐릭터: {selectedCharacter}");
    }
    // 플레이어의 캐릭터 선택 변경
    public void UpdatePlayerCharacter(string playerId, int selectedCharacter)
    {
        if (playerItems.ContainsKey(playerId))
        {
            playerItems[playerId].SetCharacterSelection(selectedCharacter);
            Debug.Log($"[PlayerListManager] {playerId}의 캐릭터 선택 상태가 {selectedCharacter}로 변경됨.");
        }
    }

    public void UpdatePlayerStatus(string playerId, PlayerStatus status)
    {
        if (playerItems.ContainsKey(playerId))
        {
            playerItems[playerId].SetStatus(status);
        }
    }

    public void RemovePlayer(string playerId)
    {
        if (playerItems.ContainsKey(playerId))
        {
            Destroy(playerItems[playerId].gameObject);
            playerItems.Remove(playerId);
        }
    }

    public void ClearPlayers()
    {
        foreach (var item in playerItems.Values)
        {
            Destroy(item.gameObject);
        }
        playerItems.Clear();
    }

    public void UpdatePlayerReadyState(string playerId, bool isReady, bool isHost = false)
    {
        if (playerItems.ContainsKey(playerId))
        {
            playerItems[playerId].SetReadyState(isReady, isHost);
            Debug.Log($"[PlayerList] {playerId}의 Ready 상태가 {isReady}로 변경됨.");
        }
    }
    public bool ContainsPlayer(string playerId)
    {
        return playerItems.ContainsKey(playerId);
    }



}
