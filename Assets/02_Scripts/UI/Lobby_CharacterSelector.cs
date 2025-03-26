using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;

public class Lobby_CharacterSelector : NetworkBehaviour
{
    public static Lobby_CharacterSelector Instance { get; private set; }

    public RawImage[] characterImages;  // 캐릭터 미리보기 UI
    public Transform[] characterModels; // 3D 캐릭터 모델
    public Button[] characterButtons;   // UI 버튼 (각 슬롯)
    public Button readyButton;
    private bool isReady = false; // Ready 상태 저장

    public Dictionary<int, string> characterSelections = new Dictionary<int, string>(); // <캐릭터 번호, 플레이어 ID>


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

    // 네트워크 데이터 기반으로 UI 업데이트
    public void UpdateCharacterUI()
    {
        Debug.Log("[Lobby_CharacterSelector] UI 업데이트 실행");

        string localPlayerId = AuthenticationService.Instance.PlayerId;
        Debug.Log($"[Lobby_CharacterSelector] localPlayerId 확인: {localPlayerId}");

        int localSelectedCharacter = -1;

        // 현재 플레이어가 선택한 캐릭터 찾기
        foreach (var entry in characterSelections)
        {
            Debug.Log($"[DEBUG] 현재 characterSelections: {entry.Key} => {entry.Value}");

            if (entry.Value == localPlayerId)
            {
                localSelectedCharacter = entry.Key;
                Debug.Log($"[DEBUG] 본인이 선택한 캐릭터 확인 완료: {localSelectedCharacter}");
                break;
            }
        }
        if (!IsHost)
        {
            Debug.Log($"[ClientDEBUG] 현재 characterSelections 상태: {string.Join(", ", characterSelections.Select(kv => kv.Key + "=>" + kv.Value))}");
        }

        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (!IsHost)
            {
                Debug.Log($"[ClientDEBUG] 시작 확인 : {i}");
            }
            if (characterSelections.ContainsKey(i))
            {
                if(!IsHost)
                {
                    Debug.Log($"[ClientDEBUG]characterSelections[{i}] 확인 : {characterSelections[i]}");
                }
                string ownerId = characterSelections[i];
                Debug.Log($"[Lobby_CharacterSelector] ownerId 확인: {ownerId}, localPlayerId: {localPlayerId}");

                if (ownerId == localPlayerId)
                {
                    // 본인이 선택한 캐릭터 → 불투명 (1.0)
                    SetButtonState(characterButtons[i], 1.0f, false);
                    SetImageState(characterImages[i], 1.0f);
                    Debug.Log($"[Lobby_CharacterSelector] 본인이 선택한 캐릭터 {i} → 불투명 (1.0)");
                }
                else
                {
                    // 다른 플레이어가 선택한 캐릭터 → 검은색 반투명 (0.8)
                    SetButtonState(characterButtons[i], 0.8f, true);
                    SetImageState(characterImages[i], 0.8f);
                    Debug.Log($"[Lobby_CharacterSelector] 다른 플레이어가 선택한 캐릭터 {i} → 검은색 반투명 (0.8)");
                }
            }
            else
            {
                // 선택되지 않은 캐릭터 → 반투명 (0.5)
                SetButtonState(characterButtons[i], 0.5f, false);
                SetImageState(characterImages[i], 0.5f);
                Debug.Log($"[Lobby_CharacterSelector] 선택 가능한 캐릭터 {i} → 반투명 (0.5)");
            }
        }

        //if (localSelectedCharacter == -1)
        //{
        //    Debug.Log("[Lobby_CharacterSelector] 본인이 선택 해제, 모든 캐릭터를 기본 상태(0.5)로 변경");
        //    for (int i = 0; i < characterButtons.Length; i++)
        //    {
        //        SetButtonState(characterButtons[i], 1.0f, false);
        //        SetImageState(characterImages[i], 1.0f);
        //    }
        //}
    }





    public void UpdateCharacterSelection(string playerId, int selectedCharacter)
    {
        Debug.Log($"[Lobby_CharacterSelector] Player {playerId} 선택한 캐릭터: {selectedCharacter}");

        // 기존 선택 해제
        foreach (var key in characterSelections.Keys.ToList())
        {
            if (characterSelections[key] == playerId)
            {
                characterSelections.Remove(key); // 기존 선택 제거
            }
        }

        if (selectedCharacter == -1)
        {
            Debug.Log($"[Lobby_CharacterSelector] Player {playerId} 캐릭터 선택 해제 (-1)");
        }
        else
        {
            // 새로운 선택 저장
            characterSelections[selectedCharacter] = playerId;
            Debug.Log($"새로운 선택 characterSelections[{selectedCharacter}]: {characterSelections[selectedCharacter]}");
        }

        // UI 강제 갱신
        Debug.Log("[Lobby_CharacterSelector] UI 갱신 실행");
        UpdateCharacterUI();
    }



    // 버튼 상태 설정
    private void SetButtonState(Button button, float alpha, bool disable = false)
    {
        ColorBlock colors = button.colors;
        Color newColor = colors.normalColor;
        newColor.a = alpha;

        if (disable)
        {
            // 다른 플레이어가 선택한 캐릭터 -> 검은색 반투명 (선택 불가능)
            newColor = Color.black * 0.8f;
        }

        colors.normalColor = newColor;
        button.colors = colors;

        // 검은색 반투명(선택 불가능)일 때만 interactable = false
        button.interactable = !disable;

        Debug.Log($"[Lobby_CharacterSelector] 버튼 상태 변경됨: Alpha={alpha}, Interactable={button.interactable}");
    }
    private void SetImageState(RawImage image, float alpha, bool isDisabled = false)
    {
        Color newColor = image.color;
        newColor.a = alpha;

        if (isDisabled)
        {
            // 다른 플레이어가 선택한 캐릭터 → 검은색 반투명 처리
            newColor = Color.black * 0.8f;
        }

        image.color = newColor;
        Debug.Log($"[Lobby_CharacterSelector] 이미지 상태 변경됨: Alpha={alpha}");
    }


    public void SelectCharacter(int characterIndex)
    {
        string playerId = AuthenticationService.Instance.PlayerId;

        if (characterSelections.ContainsKey(characterIndex) && characterSelections[characterIndex] == playerId)
        {
            // 본인이 선택한 캐릭터를 다시 선택 -> 선택 해제
            RoleManager.Instance.SelectCharacter(-1);
            //characterSelections.Remove(characterIndex); // 즉시 반영
            Debug.Log($"[Lobby_CharacterSelector] Player {playerId} 캐릭터 선택 해제");
        }
        else
        {
            // 새로운 캐릭터 선택
            RoleManager.Instance.SelectCharacter(characterIndex);
            //characterSelections[characterIndex] = playerId; // 즉시 반영
            Debug.Log($"[Lobby_CharacterSelector] Player {playerId}가 캐릭터 {characterIndex} 선택");
        }

        // UI 업데이트
        UpdateCharacterUI();
    }


}
