using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton Variable, Properties
    private static GameManager instance = null;

    public static GameManager Instance
    {
        get { return instance; }
    }
    #endregion

    #region Inspector Variables

    [SerializeField]
    private bool isLocalGame = false;

    [SerializeField]
    private bool isPCMode = false;

    [SerializeField]
    private BgmController bgmController = null;


    [SerializeField]
    public bool IsGolem = false;

    [SerializeField]
    public bool IsMush = false;
    #endregion

    #region Private Variables

    private BossStateManager bossStateManager = null;
    private MushStateManager mushStateManager = null;
    private NetworkGameManager networkGameManager = null;
    private BattleLog battleLog = null;

    #endregion

    #region Properties

    public bool IsLocalGame
    {
        get { return isLocalGame; }
    }

    public bool IsPCMode
    {
        get { return isPCMode; }
    }

    public BattleLog BattleLog
    {
        get { return battleLog; }
    }

    #endregion

    #region Public Functions

    public Transform GetBossTransform()
    {
        if (IsGolem)
        {
            return bossStateManager.transform;
        }
        else if (IsMush)
        {
            return mushStateManager.transform;
        }
        else
        {
            return null;
        }    
    }

    public int GetBossHp()
    {
        return -1;
    }

    public void DamageToBoss(PlayerManager _damageGiver, int _damage, float _aggro)
    {
        ulong clientId = _damageGiver.GetComponent<NetworkObject>().OwnerClientId;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            DamageToBoss_Multi(clientId, _damage, _aggro);
        }
    }


    public void DamageToPlayer(PlayerManager _damageReceiver, int _damage)
    {
        if (_damageReceiver.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Damaged
            || _damageReceiver.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Death
            || _damageReceiver.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Dash)
        {
            return;
        }

        if (isLocalGame)
            ApplyDamageToPlayer(_damageReceiver, _damage);
        else 
        {
            networkGameManager.OnPlayerDamaged(_damageReceiver, _damage);
        }
    }


    public void DamageToPlayer(PlayerManager _damageReceiver, int _damage, Vector3 _attackPos, float _knockBackDist)
    {
        if (_damageReceiver.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Damaged
        || _damageReceiver.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Death
        || _damageReceiver.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Dash)
        {
            return;
        }

        DamageToPlayer(_damageReceiver, _damage);

        if (isLocalGame)
        {
            ApplyKnockbackToPlayer(_damageReceiver, _attackPos, _knockBackDist);
        }
        else
        {
            networkGameManager.OnPlayerKnockback(_damageReceiver, _attackPos, _knockBackDist);
        }
    }

    public void DamageToBoss_Multi(ulong _clientId, int _damage, float _aggro)
    {
        if (IsGolem)
        {
            bossStateManager.BossDamageReceiveServerRpc(_clientId, _damage, _aggro);
        }
        else if (IsMush)
        {
            mushStateManager.BossDamageReceiveServerRpc(_clientId, _damage, _aggro);
        }
    }

    public void ApplyDamageToPlayer
        (PlayerManager _damageReceiver, int _damage)
    {
        if (_damageReceiver.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Damaged
        || _damageReceiver.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Death
        || _damageReceiver.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Dash)
        {
            return;
        }

        _damageReceiver.AttackManager.TakeDamage(_damage);
        _damageReceiver.BattleUIManager.ShowDamagedUI();
        UpdatePlayerHpUI(_damageReceiver);
    }

    public void ApplyKnockbackToPlayer(PlayerManager _target, Vector3 _attackPos, float _knockbackDist)
    {
        if (_target.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Damaged
        || _target.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Death
        || _target.StateMachine.CurrentState.StateType == EnumTypes.PlayerStateType.Dash)
        {
            return;
        }

        _target.AttackManager.KnockbackPlayer(_attackPos, _knockbackDist);
    }

    #endregion

    #region Private Functions

    private void UpdatePlayerHpUI(PlayerManager _player)
    {
        _player.BattleUIManager.UpdatePlayerHp();
    }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        bossStateManager = FindAnyObjectByType<BossStateManager>();
        mushStateManager = FindAnyObjectByType<MushStateManager>();
        networkGameManager = FindAnyObjectByType<NetworkGameManager>();

        battleLog = FindAnyObjectByType<BattleLog>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        if(bgmController != null)
        {
            // 캐릭터 종류에 따라서 인덱스를 다르게 두어서 각각 다른 BGM을 재생시킨다.
            bgmController.SetCurCharacterIndex(0);
        }

    }
}

    #endregion
