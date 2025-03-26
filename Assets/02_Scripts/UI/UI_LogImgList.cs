using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LogImgList : MonoBehaviour
{
    public List<Sprite> logImgList = null;

    private void Start()
    {
        Image bossImage = GetComponent<Image>();

        if (bossImage != null && GameManager.Instance != null)
        {
            if (GameManager.Instance.IsGolem)
            {
                bossImage.sprite = logImgList[0];
            }
            else if (GameManager.Instance.IsMush)
            {
                bossImage.sprite = logImgList[1];
            }
        }
    }
}
