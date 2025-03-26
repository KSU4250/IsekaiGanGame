using UnityEngine;

public class CanvasGroupSetting : MonoBehaviour
{
    private CanvasGroup CanvasGroup = null;
    private void Awake()
    {
        CanvasGroup = GetComponent<CanvasGroup>();
    }
    private void Start()
    {
        CanvasGroup.enabled = true;
        CanvasGroup.interactable = true;
        CanvasGroup.blocksRaycasts = true;
    }
}
