using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class UI_GameResultManager : MonoBehaviour
{
    public Image backgroundImg = null;
    public List<Image> imgs = new List<Image>();
    public List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
    public List<Button> buttons = new List<Button>();
    public bool isGood;

    // 저장할 변수들
    private List<Color> initialImgColors = new List<Color>();
    private List<Vector3> initialImgScales = new List<Vector3>();
    private List<Color> initialTextColors = new List<Color>();
    private List<Vector3> initialTextScales = new List<Vector3>();
    private Color initialBackgroundColor;
    private Vector3 initialBackgroundScale;


    private void Awake()
    {
        // 자식 이미지들 및 텍스트들을 리스트에 저장
        imgs = GetComponentsInChildren<Image>().ToList();
        texts = GetComponentsInChildren<TextMeshProUGUI>().ToList();
        backgroundImg = GetComponentInChildren<FillScreenImage>().GetComponent<Image>();
        buttons = GetComponentsInChildren<Button>().ToList();  // 버튼 리스트도 초기화

        // 각 UI 요소들의 초기 알파 값과 크기를 저장
        foreach (var img in imgs)
        {
            initialImgColors.Add(img.color);
            initialImgScales.Add(img.transform.localScale);
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);  // 초기 알파값 0으로 설정
            img.transform.localScale = Vector3.zero;  // 초기 크기 0으로 설정
        }

        foreach (var text in texts)
        {
            initialTextColors.Add(text.color);
            initialTextScales.Add(text.transform.localScale);
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);  // 초기 알파값 0으로 설정
            text.transform.localScale = Vector3.zero;  // 초기 크기 0으로 설정
        }

        if (backgroundImg != null)
        {
            initialBackgroundColor = backgroundImg.color;
            initialBackgroundScale = backgroundImg.transform.localScale;
            backgroundImg.color = new Color(backgroundImg.color.r, backgroundImg.color.g, backgroundImg.color.b, 0f);  // 초기 알파값 0으로 설정
            backgroundImg.transform.localScale = Vector3.zero;  // 초기 크기 0으로 설정
        }

        // 버튼을 초기화 할 때 모두 비활성화
        foreach (var button in buttons)
        {
            button.interactable = false;
        }
    }

    // UI 요소들 페이드 인 및 스케일 인을 처리하는 코루틴
    public IEnumerator FadeInAndScaleIn()
    {
        // 애니메이션 지속 시간
        float duration = 0.3f;

        // 배경 이미지 페이드 인 처리 (가장 먼저 나타나야 함)
        if (backgroundImg != null)
        {
            Color currentColor = initialBackgroundColor;

            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                backgroundImg.color = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(0f, currentColor.a, t)); // 알파 값 페이드 인

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            backgroundImg.color = currentColor; // 원래 알파값으로 설정
        }

        // 이미지들에 대해 페이드 인과 스케일 인 처리
        for (int i = 0; i < imgs.Count; i++)
        {
            var img = imgs[i];
            Color currentColor = initialImgColors[i];
            Vector3 currentScale = initialImgScales[i];

            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                img.color = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(0f, currentColor.a, t)); // 알파 값 페이드 인
                img.transform.localScale = Vector3.Lerp(Vector3.zero, currentScale, t); // 크기 증가

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            img.color = currentColor; // 원래 알파값으로 설정
            img.transform.localScale = currentScale; // 원래 크기로 설정
        }

        // 텍스트들에 대해 페이드 인과 스케일 인 처리
        for (int i = 0; i < texts.Count; i++)
        {
            var text = texts[i];
            Color currentColor = initialTextColors[i];
            Vector3 currentScale = initialTextScales[i];

            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                text.color = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(0f, currentColor.a, t)); // 알파 값 페이드 인
                text.transform.localScale = Vector3.Lerp(Vector3.zero, currentScale, t); // 크기 증가

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            text.color = currentColor; // 원래 알파값으로 설정
            text.transform.localScale = currentScale; // 원래 크기로 설정
        }

        // 모든 UI 요소들이 다 페이드 인 되면 버튼을 활성화
        foreach (var button in buttons)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                if (isGood == true)
                {
                    // button.transform.GetChild(0).GetComponent<TMP_Text>().text = "Go Lobby";
                    button.transform.GetChild(0).GetComponent<TMP_Text>().text = "Next";
                }
                else
                {
                    button.transform.GetChild(0).GetComponent<TMP_Text>().text = "Try Again";
                }
                button.interactable = true;
            }
            else
            {
                button.transform.GetChild(0).GetComponent<TMP_Text>().text = "Wait";
            }
        }
    }

    // 코루틴을 시작하는 예시 (필요한 곳에서 호출)
    public void StartFadeIn(bool _isGood)
    {
        if (_isGood == isGood)
        {
            StartCoroutine(FadeInAndScaleIn());
        }
    }
}
