using System.Collections;
using UnityEngine;

public class TestTrigger : MonoBehaviour
{
    private bool isFire = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "BossAttack")
        {
            if (transform.tag == "BehindRock")
            {
                Debug.Log("µ¹µÚ¿¡ ¼û¾î¼­ ¾È¸ÂÀ½");
                return;
            }

            float damage = other.GetComponent<BossAttackCollider>().Damage;
            Debug.Log("µ¥¹ÌÁö ¹ÞÀ½" + damage);
        }

        if (other.gameObject.tag == "Fire")
        {
            Debug.Log("¾Ñ¶ß°Å");
            if (!isFire)
            {
                StartCoroutine(FireDamage());
            }
        }
    }

    private IEnumerator FireDamage()
    {
        isFire = true;

        float elapseTime = 0f;
        int tickCnt = 0;

        // 0.5ÃÊ´ç 1Æ½, 6Æ½ÈÄ ºÒ²¨Áü
        while (true)
        {
            elapseTime += Time.deltaTime;

            if (elapseTime >= 0.5f)
            {
                elapseTime = 0f;
                tickCnt++;
                Debug.Log("Æ½µ© " + tickCnt + "¹ø ¹ÞÀ½");
            }

            if (tickCnt == 6)
            {
                break;
            }

            yield return null;
        }

        Debug.Log("È­»ó ³¡³²");

        isFire = false;
    }
}
