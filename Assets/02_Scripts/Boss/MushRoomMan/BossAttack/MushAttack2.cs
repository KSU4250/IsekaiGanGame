using System.Collections;
using UnityEngine;

public class MushAttack2 : MonoBehaviour
{
    private MushStateManager mush;
    private Vector3 direction;
    private Coroutine curCoroutine;
    private void Awake()
    {
        mush = FindAnyObjectByType<MushStateManager>();
    }

    private void Start()
    {
        Vector3 myPos = transform.position;
        Vector3 bossPos = mush.Boss.transform.position;

        myPos.y = 0;
        bossPos.y = 0;

        direction = (myPos - bossPos).normalized;

        curCoroutine = StartCoroutine(GoFront());
    }

    private IEnumerator GoFront()
    {
        while (true)
        {
            transform.position = transform.position + direction * Time.deltaTime;

            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Wall")
        {
            StopCoroutine(curCoroutine);
            Destroy(gameObject);
        }
    }
}
