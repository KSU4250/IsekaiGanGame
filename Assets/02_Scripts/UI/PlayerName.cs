using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerName : MonoBehaviour
{
    private TextMeshPro nameText = null;
    private Camera mainCam = null;

    public void SetName(string _name)
    {
        nameText.text = _name;
    }

    private void Awake()
    {
        nameText = GetComponentInChildren<TextMeshPro>();
    }

    private void Start()
    {
        mainCam = Camera.main;

        if(mainCam != null)
        {
            StartCoroutine(UpdateCoroutine());
        }
    }

    private IEnumerator UpdateCoroutine()
    {
        while(true)
        {
            transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward, mainCam.transform.rotation * Vector3.up);
            yield return null;
        }
    }
}
