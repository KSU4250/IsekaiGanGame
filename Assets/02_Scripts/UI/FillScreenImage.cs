using UnityEngine;
using UnityEngine.UI;

public class FillScreenImage : MonoBehaviour
{
    RectTransform rectTransform;
    Vector2 lastScreenSize;

    void Start()
    {
        // RectTransform 초기화
        Image image = GetComponent<Image>();
        rectTransform = image.GetComponent<RectTransform>();

        // 처음 화면 크기 설정
        lastScreenSize = new Vector2(Screen.width, Screen.height);

        // 초기 사이즈 설정
        UpdateSize();
    }

    void Update()
    {
        // 화면 크기 변동이 있을 때만 갱신
        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);

        if (currentScreenSize != lastScreenSize)
        {
            lastScreenSize = currentScreenSize;
            UpdateSize();
        }
    }

    void UpdateSize()
    {
        // 화면 크기 기반으로 이미지 크기 업데이트
        rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
    }
}
