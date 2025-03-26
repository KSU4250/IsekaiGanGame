using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum PlayerStatus { Host, Ready, Not_Ready }

public class PlayerItem : MonoBehaviour
{
    private string playerId;
    private string username;
    private PlayerStatus status;

    public TMP_Text usernameText;
    public Image statusIcon;


    public void SetPlayerInfo(string id, string name, PlayerStatus playerStatus)
    {
        playerId = id;
        username = name;
        status = playerStatus;

        UpdateUI();
    }

    private void UpdateUI()
    {
        usernameText.text = username;
        SetStatus(status);
    }

    public void SetCharacterSelection(int selectedCharacter)
    {
        Debug.Log($"[PlayerItem] Player {playerId} 선택한 캐릭터: {selectedCharacter}");
    }

    public void SetStatus(PlayerStatus status)
    {
        string labelKey = "LobbyPlayerIcons"; // 지정한 Label 사용

        Addressables.LoadAssetsAsync<Sprite>(labelKey, null).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var sprite in handle.Result)
                {
                    if (sprite.name.Contains(status.ToString()))
                    {
                        statusIcon.sprite = sprite;
                        return;
                    }
                }
                Debug.LogError($"[PlayerItem] {status.ToString()} 아이콘을 찾을 수 없습니다!");
            }
            else
            {
                Debug.LogError($"[PlayerItem] {labelKey}에서 아이콘 로드 실패!");
            }
        };
    }

    public void SetReadyState(bool isReady, bool isHost = false)
    {
        string statusName = isHost ? "Host" : (isReady ? "Ready" : "Not_Ready");

        string labelKey = "LobbyPlayerIcons"; // 동일한 Label 사용
        Addressables.LoadAssetsAsync<Sprite>(labelKey, null).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var sprite in handle.Result)
                {
                    if (sprite.name.Contains(statusName))
                    {
                        statusIcon.sprite = sprite;
                        return;
                    }
                }
                Debug.LogError($"[PlayerItem] {statusName} 아이콘을 찾을 수 없습니다!");
            }
            else
            {
                Debug.LogError($"[PlayerItem] {labelKey}에서 아이콘 로드 실패!");
            }
        };

        Debug.Log($"[PlayerItem] Ready 상태 변경 - 플레이어: {usernameText.text}, 상태: {statusName}");
    }

}
