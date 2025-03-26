using System.Collections;
using UnityEngine;

public class BossPhase2Cam : MonoBehaviour
{
    // 카메라 move 관련
    public Transform Cam;
    public float moveTime;

    private Vector3 origin;
    private Vector3 target;

    // 카메라 shake 관련
    public float shakeAmout;
    public float shakeTime;

    public IEnumerator MoveCam()
    {
        origin = Cam.localPosition;
        target = new Vector3(0, 13f, -13f);

        Quaternion targetRotation = Quaternion.Euler(-20f, 0f, 0f);

        float elapseTime = 0f;

        while (true)
        {
            elapseTime += Time.deltaTime;

            Cam.localPosition = Vector3.Lerp(origin, target, elapseTime / moveTime);
            Cam.rotation = Quaternion.Lerp(Quaternion.identity, targetRotation, elapseTime / moveTime);

            if (elapseTime > moveTime)
            {
                break;
            }
            yield return null;
        }
    }

    public IEnumerator ReturnCam()
    {
        origin = Cam.localPosition;
        target = new Vector3(0f, 12f, -7f);

        Quaternion curRotation = Quaternion.Euler(-20f, 0f, 0f);

        float elapseTime = 0f;

        while (true)
        {
            elapseTime += Time.deltaTime;

            Cam.localPosition = Vector3.Lerp(origin, target, elapseTime / moveTime);
            Cam.rotation = Quaternion.Lerp(curRotation, Quaternion.identity, elapseTime / moveTime);

            if (elapseTime > moveTime)
            {
                break;
            }
            yield return null;
        }
    }

    public IEnumerator ShakeCam()
    {
        float elapseTime = 0f;

        Vector3 originPos = Cam.localPosition;

        while (true)
        {
            elapseTime += Time.deltaTime;

            Cam.localPosition = Random.insideUnitSphere * shakeAmout + originPos;

            if (elapseTime >= shakeTime)
            {
                break;
            }

            yield return null;
        }

        Cam.localPosition = originPos;
    }


}
