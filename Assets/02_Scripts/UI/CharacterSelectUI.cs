using UnityEngine;

public class CharacterSelectUI : MonoBehaviour
{
    public RectTransform[] characterSlots;  // 4개의 캐릭터 선택 UI 슬롯
    public Transform[] characterModels;    // 4개의 3D 캐릭터
    public Camera worldCamera;             // 3D 씬을 렌더링하는 카메라
    public Canvas canvas;

    private Vector2 initialCanvasSize;
    private Vector3[] initialCharacterPositions;

    void Start()
    {
        if (canvas != null)
            initialCanvasSize = canvas.pixelRect.size;

        initialCharacterPositions = new Vector3[characterModels.Length];

        for (int i = 0; i < characterModels.Length; i++)
        {
            initialCharacterPositions[i] = characterModels[i].position;
        }
    }

    void Update()
    {
        Vector2 currentCanvasSize = canvas.pixelRect.size;
        float scaleFactorX = currentCanvasSize.x / initialCanvasSize.x;
        float scaleFactorY = currentCanvasSize.y / initialCanvasSize.y;

        for (int i = 0; i < characterSlots.Length; i++)
        {
            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, characterSlots[i].position);
            Ray ray = worldCamera.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                characterModels[i].position = hit.point;
            }

            // UI 크기에 맞게 3D 캐릭터 위치 자동 조정
            characterModels[i].position = new Vector3(
                initialCharacterPositions[i].x * scaleFactorX,
                initialCharacterPositions[i].y * scaleFactorY,
                initialCharacterPositions[i].z
            );
        }
    }
}
