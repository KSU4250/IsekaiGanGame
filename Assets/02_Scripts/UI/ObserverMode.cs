using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObserverMode : MonoBehaviour
{
    // 관전하기 버튼은 콤퍼넌트에서 관리중
    private List<Button> buttonList = new List<Button>();
    public List<TextMeshProUGUI> textMeshProUGUI;
    public bool isObserverModeUI = false;
    private void Awake()
    {
        buttonList = GetComponentsInChildren<Button>().ToList(); // 자식 버튼 리스트로 받아옴
        textMeshProUGUI = GetComponentsInChildren<TextMeshProUGUI>().ToList(); //자식 텍스트 받아옴

        ObserverModeHow(false);
    }

    public void ObserverModeHow(bool _how)
    {
        foreach (Button button in buttonList) 
        {
            button.gameObject.SetActive(_how); // 버튼들 _how 처럼
        }
        foreach (TextMeshProUGUI texts in textMeshProUGUI)
        {
            if (texts == textMeshProUGUI[0])
            {
                continue;
            }
            texts.gameObject.SetActive(_how);// 텍스트들 _how 처럼
        }
        isObserverModeUI = _how; //불값 _how 처럼
    }
    public void ObserverModeButtonOn()
    {
        buttonList[0].gameObject.SetActive(true);
    }
}
