using UnityEngine;
using Unity.Netcode;

public class NetworkInitializer : MonoBehaviour
{
    public GameObject networkManagerPrefab;

    private void Awake()
    {
        if (NetworkManager.Singleton == null) // NetworkManager가 없으면 생성
        {
            GameObject networkManager = Instantiate(networkManagerPrefab);
            DontDestroyOnLoad(networkManager);
            Debug.Log("[Network] 새로운 NetworkManager가 생성되었습니다.");
        }
        else
        {
            Debug.Log("[Network] 기존 NetworkManager가 존재합니다.");
        }
    }

    private void Start()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartHost(); // 호스트로 시작 (테스트 모드)
            Debug.Log("[Network] Game 씬에서 바로 Host 시작");
        }
    }
}
