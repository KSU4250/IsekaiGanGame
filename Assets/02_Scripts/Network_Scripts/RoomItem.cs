using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomItem : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;
    private string roomId;

    // 방 정보를 설정
    public void SetRoomInfo(string roomId, string roomName, int currentPlayers, int maxPlayers)
    {
        this.roomId = roomId;
        roomNameText.text = $"{roomName}";
        playerCountText.text = $"{currentPlayers}/{maxPlayers}";

        // 디버그 로그 추가
        Debug.Log($"[RoomItem] SetRoomInfo 호출됨: roomId={roomId}, roomName={roomName}, players={currentPlayers}/{maxPlayers}");
    }

    // 클릭 이벤트 처리
    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭된 방 정보 로그
        Debug.Log($"[RoomItem] 방 클릭됨: roomId={roomId}, roomName={roomNameText.text}, players={playerCountText.text}");

        // RoomManager로 이벤트 전달
        RoomManager.Instance.OnRoomItemClicked(this);
    }

    // RoomItem의 RoomId를 반환
    public string GetRoomId()
    {
        return roomId;
    }
}
