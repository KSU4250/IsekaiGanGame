using UnityEngine;

public class UITo3DObjectScaler : MonoBehaviour
{
    public RectTransform uiElement;  // UI 요소 (RectTransform)
    public Transform object3D;       // 3D 오브젝트
    public Camera worldCamera;       // 월드 카메라
    public Canvas canvas;            // UI가 포함된 Canvas

    private Vector3 initial3DPos;    // 초기 3D 오브젝트 위치
    private Vector2 initialCanvasSize; // 초기 캔버스 크기

    void Start()
    {
        if (object3D != null)
            initial3DPos = object3D.position; // 초기 3D 오브젝트 위치 저장

        if (canvas != null)
            initialCanvasSize = canvas.pixelRect.size; // 초기 Canvas 크기 저장
    }

    void Update()
    {
        if (uiElement == null || object3D == null || worldCamera == null || canvas == null) return;

        // 현재 화면 크기 가져오기
        Vector2 currentCanvasSize = canvas.pixelRect.size;

        // UI 요소의 화면 좌표(Screen Position) 가져오기
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, uiElement.position);

        // 화면 좌표를 3D 공간의 Ray로 변환
        Ray ray = worldCamera.ScreenPointToRay(screenPos);

        // Ray가 충돌하는 3D 오브젝트 배치할 위치 찾기
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            object3D.position = hit.point; // 충돌한 지점으로 3D 오브젝트 이동
        }

        // 화면 크기에 따라 위치 자동 조정 (비율 유지)
        float scaleFactorX = currentCanvasSize.x / initialCanvasSize.x;
        float scaleFactorY = currentCanvasSize.y / initialCanvasSize.y;

        // 3D 오브젝트 위치를 화면 비율에 맞춰 보정
        object3D.position = new Vector3(
            initial3DPos.x * scaleFactorX,
            initial3DPos.y * scaleFactorY,
            initial3DPos.z
        );
    }
}
