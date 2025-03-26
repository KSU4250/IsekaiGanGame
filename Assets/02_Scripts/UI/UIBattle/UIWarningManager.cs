using UnityEngine;
using UnityEngine.UI;

public class UIWarningManager : MonoBehaviour
{
    public Image errorImage = null;
    public bool isError = false;

    private void Awake()
    {
        errorImage = GetComponentInChildren<Image>();
    }
    
    public void ConnectionError() // 에러 표시 On
    {
        errorImage.enabled = true;
    }
    public void ReConnection() // 에러 표시 OFF
    {
        errorImage.enabled = false;
    }
}
