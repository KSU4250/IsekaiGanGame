using UnityEngine;
using UnityEngine.UI;

public class ButtonInvoker : MonoBehaviour
{
    private Button targetButton;  // 버튼 컴포넌트

    void Awake()
    {
        // 같은 오브젝트에서 버튼 컴포넌트 가져오기
        targetButton = GetComponent<Button>();

        // 버튼이 존재하는 경우에만 이벤트 호출
        if (targetButton != null)
        {
            // 버튼에 등록된 OnClick 이벤트를 한 번 실행
            targetButton.onClick.Invoke();
        }
    }
}
