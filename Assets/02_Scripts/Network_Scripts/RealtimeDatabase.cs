using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class RealtimeDatabase : MonoBehaviour
{
    private DatabaseReference databaseReference;

    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    [Header("User Data")]
    private string username;
    private string password;
    private float bestClearTime = 600f;  // 기본값 10분 (600초)
    private int highestScore = 0;        // 기본값 0


    void Start()
    {
        Debug.Log("RealtimeDatabase.Start() 실행됨");


        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Firebase 초기화 실패: " + task.Exception);
                return;
            }

            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase 초기화 성공");
                FirebaseApp app = FirebaseApp.DefaultInstance;

                if (app == null)
                {
                    Debug.LogError("FirebaseApp이 null입니다.");
                    return;
                }

                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

                if (databaseReference == null)
                {
                    Debug.LogError("FirebaseDatabase RootReference가 null입니다.");
                }
                else
                {
                    Debug.Log("FirebaseDatabase RootReference 정상적으로 초기화됨");
                }
            }
            else
            {
                Debug.LogError($"Firebase 사용 불가: {task.Result}");
            }
        });
    }

    // 회원가입 처리
    public void RegisterUser()
    {
        string userId = usernameInput.text;
        string userPass = passwordInput.text;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userPass))
        {
            Debug.LogError("Username or password cannot be empty!");
            return;
        }

        UserData newUser = new UserData(userId, userPass);
        string json = JsonUtility.ToJson(newUser);

        databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Error registering user: " + task.Exception);
            }
            else
            {
                Debug.Log("User registered successfully");
            }
        });
    }

    // 로그인 처리
    public void LoginUser()
    {
        Debug.Log("LoginUser() 실행됨");

        string userId = usernameInput.text;
        string userPass = passwordInput.text;

        if (usernameInput == null || passwordInput == null)
        {
            Debug.LogError("usernameInput 또는 passwordInput이 null입니다.");
            return;
        }

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userPass))
        {
            Debug.LogError("Username or password cannot be empty!");
            return;
        }

        if (databaseReference == null)
        {
            Debug.LogError("databaseReference가 null 상태입니다. Firebase가 올바르게 초기화되지 않았을 수 있습니다.");
            return;
        }

        Debug.Log($"사용자 로그인 시도: {userId}");

        databaseReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Error loading user data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Debug.Log($"사용자 데이터 확인됨: {userId}");
                    UserData userData = JsonUtility.FromJson<UserData>(snapshot.GetRawJsonValue());

                    if (userData == null)
                    {
                        Debug.LogError("UserData 객체 변환 중 null 발생");
                        return;
                    }

                    if (userData.password == userPass)
                    {
                        username = userData.username;
                        password = userData.password;
                        bestClearTime = userData.bestClearTime;
                        highestScore = userData.highestScore;
                        PlayerPrefs.SetString("username", userData.username);
                        PlayerPrefs.Save();
                        Debug.Log($"로그인 성공: {username}");
                        SceneManager.LoadScene("LobbyTest");
                        
                    }
                    else
                    {
                        Debug.Log("비밀번호 불일치");
                    }
                }
                else
                {
                    Debug.Log("사용자 데이터 없음");
                }
            }
        });
    }

    // 유저 데이터 저장
    public void SaveUserData()
    {
        string userId = usernameInput.text;

        databaseReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Error loading data: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    UserData existingData = JsonUtility.FromJson<UserData>(snapshot.GetRawJsonValue());

                    // 기존 최고 점수와 클리어 타임 비교 후 업데이트
                    if (highestScore > existingData.highestScore)
                    {
                        existingData.highestScore = highestScore;
                    }

                    if (bestClearTime < existingData.bestClearTime)
                    {
                        existingData.bestClearTime = bestClearTime;
                    }

                    string json = JsonUtility.ToJson(existingData);

                    databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(saveTask => {
                        if (saveTask.IsFaulted)
                        {
                            Debug.LogError("Error saving data: " + saveTask.Exception);
                        }
                        else
                        {
                            Debug.Log("User data updated successfully!");
                        }
                    });
                }
            }
        });
    }



    // 유저 데이터 구조체
    [System.Serializable]
    public class UserData
    {
        public string username;
        public string password;
        public float bestClearTime = 600f;  // 기본값 10분
        public int highestScore = 0;        // 기본값 0

        public UserData(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
}
