using TMPro;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.Services.Authentication;

public class LobbyManager : MonoBehaviour
{
    public TMP_InputField roomNameInput;
    public TMP_InputField roomCodeInput;
    public Button createButton;
    public Button joinButton;

    private Lobby currentLobby;
    int maxPlayers = 4;
    private string lobbyCode;

    private async void Start()
    {
        await InitializeUnityServices();
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initialized");

            if (AuthenticationService.Instance.IsSignedIn == false)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in anonymously");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Unity Services Initialization Failed: {e.Message}");
        }
    }

    public async void CreateLobby()
    {
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
        {
            Debug.LogError("Unity Services not initialized yet!");
            return;
        }

        string lobbyName = roomNameInput.text;
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = false
        };

        try
        {
            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            lobbyCode = currentLobby.LobbyCode;
            Debug.Log("Lobby Created: " + lobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Create lobby failed: " + e.Message);
        }
    }

    public async void JoinLobby()
    {
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
        {
            Debug.LogError("Unity Services not initialized yet!");
            return;
        }

        string code = roomCodeInput.text;
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
            Debug.Log("Joined Lobby: " + currentLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Join lobby failed: " + e.Message);
        }
    }
}
