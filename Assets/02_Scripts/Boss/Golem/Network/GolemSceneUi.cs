using UnityEngine;

public class GolemSceneUi : MonoBehaviour
{
    [SerializeField] private GameObject loadingImg;

    private void Start()
    {
        FindAnyObjectByType<NetworkGameManager>().loadingFinishCallback += () => Invoke("LoadingSetOff", 1f);
    }

    // ·Îµù ¾À ²ô±â
    private void LoadingSetOff()
    {
        loadingImg.SetActive(false);
    }
}
