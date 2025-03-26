using UnityEngine;
using UnityEngine.UI;

public class LogSlot : MonoBehaviour
{
    [SerializeField]
    private Image slotImage = null;

    public void SetImage(Sprite sprite)
    {
        Debug.LogFormat("Sprite Name : {0}", sprite.name);
        slotImage.sprite = sprite;
    }
}
