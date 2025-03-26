using UnityEngine;

public class FPSManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
#if UNITY_ANDROID
        Application.targetFrameRate = 60;
#else
        QualitySettings.vSyncCount = 1;
#endif
    }
}
