using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FollowCamera : MonoBehaviour
{
    private PlayerManager playerManager = null;
    private BossStateManager bossStateManager = null;
    private MushStateManager mushStateManager = null;
    private int myIndex;
    private bool IsDie = false;
    private GameObject DieUi;

    // 카메라 흔들기 관련
    public Transform Cam;
    public float shakeAmout = 0.2f;
    public float shakeTime = 0.2f;

    private GameObject[] alivePlayer;

    private void Awake()
    {
        FindAnyObjectByType<NetworkGameManager>().loadingFinishCallback += FindPlayerObjectForClient;
        FindAnyObjectByType<NetworkGameManager>().gameEndCallback += () => { Invoke("DieUiOff", 1f); };
        bossStateManager = FindAnyObjectByType<BossStateManager>();
        mushStateManager = FindAnyObjectByType<MushStateManager>();
    }

    private void Update()
    {
        if(playerManager != null)
            transform.position = playerManager.transform.position;
    }

    private void FindPlayerObjectForClient()
    {
        // 내 id 가져오기
        ulong myClientId = NetworkManager.Singleton.LocalClientId;

        // 내꺼 찾기
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Debug.Log("접속중인 클라 아이디 : " + client.ClientId);

            if (client.ClientId == myClientId)
            {
                playerManager = client.PlayerObject.GetComponent<PlayerManager>();
            }
        }

        Invoke("SetPlayerInfo", 1f);
    }

    // 플레이어 정보 세팅하는 함수
    private void SetPlayerInfo()
    {
        if (bossStateManager != null)
        {
            alivePlayer = bossStateManager.AlivePlayers;
        }
        else if (mushStateManager != null)
        {
            alivePlayer = mushStateManager.AlivePlayers;
        }

        for (int i = 0; i < 4; ++i)
        {
            if (alivePlayer[i] == null) continue;

            if (alivePlayer[i].GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                myIndex = i;
            }
        }

        StartCoroutine(DieCheckCoroutine());
    }

    // 카메라 바꾸는 함수
    private void ChangePlayerCamRight()
    {
        if (IsDie)
        {
            int startIndex = myIndex; // 무한 루프 방지용 (전체 순회 후에도 못 찾으면 종료)

            do
            {
                // 다음 인덱스로 이동 (원형 순환)
                myIndex = (myIndex + 1) % alivePlayer.Length;

                // 만약 유효한 오브젝트면 탈출
                if (alivePlayer[myIndex] != null)
                {
                    playerManager = alivePlayer[myIndex].GetComponent<PlayerManager>();
                    break;
                }

            } while (myIndex != startIndex); // 모든 요소를 돌았으면 종료 

        }
    }

    // 카메라 바꾸는 함수2
    private void ChangePlayerCamLeft()
    {
        if (IsDie)
        {
            int startIndex = myIndex; // 무한 루프 방지용 (전체 순회 후에도 못 찾으면 종료)

            do
            {
                // 다음 인덱스로 이동 (원형 순환)
                myIndex = (myIndex - 1 + alivePlayer.Length) % alivePlayer.Length;

                // 만약 유효한 오브젝트면 탈출
                if (alivePlayer[myIndex] != null)
                {
                    playerManager = alivePlayer[myIndex].GetComponent<PlayerManager>();
                    break;
                }

            } while (myIndex != startIndex); // 모든 요소를 돌았으면 종료 

        }
    }

    // 내가 죽었는지 check하는 함수
    private IEnumerator DieCheckCoroutine()
    {
        WaitForSeconds checkTime = new WaitForSeconds(1f);
        while (true)
        {
            if (alivePlayer[myIndex] == null)
            {
                IsDie = true;
                PlayerDieSetting();
                break;
            }

            yield return checkTime;
        }
    }

    // 카메라 흔드는 함수
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

    // 플레이어 죽었을때 UI 키는 함수
    private void PlayerDieSetting()
    {
        DieUi = GameObject.Find("ObserverMode");
        DieUi.transform.GetChild(0).gameObject.SetActive(true);
        DieUi.transform.GetChild(1).gameObject.SetActive(true);
        DieUi.transform.GetChild(2).gameObject.SetActive(true);
        DieUi.transform.GetChild(3).gameObject.SetActive(true);

        Button LeftBtn = DieUi.transform.Find("Left").gameObject.GetComponent<Button>();
        Button RightBtn = DieUi.transform.Find("Right").gameObject.GetComponent<Button>();
        LeftBtn.onClick.AddListener(ChangePlayerCamLeft);
        RightBtn.onClick.AddListener(ChangePlayerCamRight);
    }

    // 게임 끝났을때 ui끄기
    private void DieUiOff()
    {
        DieUi.SetActive(false);
    }
}
