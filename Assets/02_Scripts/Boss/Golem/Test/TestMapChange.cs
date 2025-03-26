using System.Collections;
using UnityEngine;

public class TestMapChange : MonoBehaviour
{
    public Material map;

    private void Start()
    {
        map.SetFloat("_Range", 0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(ChangeColor());
        }
    }

    private IEnumerator ChangeColor()
    {
        float elapseTime = 0f;
        float newValue = 0f;
        while (true)
        {
            elapseTime += Time.deltaTime;
            newValue = Mathf.Lerp(0f, 100f, elapseTime / 5f);

            map.SetFloat("_Range", newValue);

            if (elapseTime >= 5f)
            {
                break;
            }
            yield return null;
        }
    }
}
