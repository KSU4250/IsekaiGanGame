using System.Collections;
using UnityEngine;

public class MushHitMat : MonoBehaviour
{
    public Material mat;
    public float hitTime;
    public float hitPower;

    public IEnumerator ChangeMat()
    {
        mat.SetFloat("_Power", hitPower);
        mat.SetColor("_FresnelColor", new Color(1, 1, 1, 0));

        float elapsedTime = 0f;

        while (true)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / hitTime;
            float newValue = Mathf.Lerp(hitPower, 10f, t);

            mat.SetFloat("_Power", newValue);
            mat.SetColor("_FresnelColor", new Color(1 - t, 1 - t, 1 - t, 0));

            if (elapsedTime >= hitTime)
            {
                break;
            }

            yield return null;
        }

        mat.SetColor("_FresnelColor", new Color(0, 0, 0, 0));
        mat.SetFloat("_Power", 10f);
    }

}
