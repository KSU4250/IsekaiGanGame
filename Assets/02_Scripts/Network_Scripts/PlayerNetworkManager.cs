using EnumTypes;
using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

public class PlayerNetworkManager : NetworkBehaviour
{
    public UnityAction<ulong> OnNetworkPlayerDeath;
    public UnityAction<ulong> OnNetworkPlayerRevive;

    private PlayerManager playerManager = null;
    private NetworkAnimator networkAnimator = null;

    public void SetNetworkAnimatorTrigger(int _hashId)
    {
        if (networkAnimator == null)
        {
            Debug.Log("Network Animator is not valid!");
            return;
        }

        if(IsClientPlayer())
            networkAnimator.SetTrigger(_hashId);
    }

    public bool IsClientPlayer()
    {
        ulong clientId = NetworkManager.LocalClientId;

        if (clientId != OwnerClientId)
            return false;

        return true;
    }

    public void NetworkRevivePlayer()
    {
        RevivePlayerServerRpc(OwnerClientId);
    }

    public void NetworkChangeState(PlayerStateType _state)
    {
        ChangeStateServerRpc(OwnerClientId, _state);
    }

    public void NetworkSetSpeedAnimator(float _speed)
    {
        SetSpeedAnimServerRpc(OwnerClientId, _speed);
    }

    [Rpc(SendTo.Server)]
    private void RevivePlayerServerRpc(ulong _clientId)
    {
        ApplyRevivePlayerClientRpc(_clientId);
    }

    [Rpc(SendTo.Server)]
    private void SetSpeedAnimServerRpc(ulong _clientId, float _speed)
    {
        var obj = NetworkManager.ConnectedClients[_clientId].PlayerObject;
        if(obj != null)
        {
            obj.GetComponent<PlayerManager>().AnimationManager.ApplyAnimatorWalkSpeed(_speed);
        }
    }

    [Rpc(SendTo.Server)]
    private void ChangeStateServerRpc(ulong _clientId, PlayerStateType _state)
    {
        ChangeStateClientRpc(_clientId, _state);
    }

    [Rpc(SendTo.Server)]
    private void SetAnimationTriggerRpc(ulong _clientId, int _hashId)
    {
        networkAnimator.SetTrigger(_hashId);
    }

    [Rpc(SendTo.Everyone)]
    private void ApplyRevivePlayerClientRpc(ulong _clientId)
    {
        var obj = NetworkManager.ConnectedClients[_clientId].PlayerObject;

        if (obj != null)
        {
            obj.GetComponent<PlayerManager>().ApplyRevive();
        }
    }


    [Rpc(SendTo.Everyone)]
    private void ChangeStateClientRpc(ulong _clientId, PlayerStateType _state)
    {
        var obj = NetworkManager.ConnectedClients[_clientId].PlayerObject;

        if (obj != null)
        {
            obj.GetComponent<PlayerManager>().ChangeState_Local(_state);
        }
    }

    public override void OnNetworkSpawn()
    {
        FindAnyObjectByType<NetworkGameManager>().CheckPlayerSpawnServerRpc();
    }

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        networkAnimator = playerManager.GetComponent<NetworkAnimator>();
    }

}
