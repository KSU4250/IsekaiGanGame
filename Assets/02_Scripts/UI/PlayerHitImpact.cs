using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHitImpact : MonoBehaviour
{
    private float impactTime = 0.2f;
    private Image Image;

    private WaitForSeconds waitSeconds = null;

    private void Awake()
    {
        Image = GetComponent<Image>();
        waitSeconds = new WaitForSeconds(impactTime);
        PlayerDamagedImpactOff();
    }

    public void PlayerDamagedImpactOn()
    {
        StopAllCoroutines();
        StartCoroutine(ShowHitCoroutine());
    }

    public void PlayerDamagedImpactOff()
    {
        Image.enabled = false;
    }

    private IEnumerator ShowHitCoroutine()
    {
        Image.enabled = true;

        yield return waitSeconds;

        Image.enabled = false;
    }
}
