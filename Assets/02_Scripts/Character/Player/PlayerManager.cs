using UnityEngine;

using EnumTypes;
using UnityEngine.Events;
using Unity.Netcode;

public class PlayerManager : MonoBehaviour
{
    #region Variables

        #region Inspector Variables

    [Header("References")]
    [SerializeField]
    private PlayerData playerData = null;
    [SerializeField]
    private UIBattleUIManager battleUIManager = null;
    [SerializeField]
    private SkillUIManager skillUIManager = null;

    [Header("Attack Settings")]
    [SerializeField]
    private Transform rangeAttackStartTr = null;
    [SerializeField]
    private MeleeWeapon meleeWeapon = null;

        #endregion

        #region Private Variables

            #region Manager References

    private PlayerInputManager playerInputManager = null;
    private PlayerSkillManager skillManager = null;
    private StatusManager statusManager = null;
    private PlayerStateMachine stateMachine = null;
    private PlayerAttackManager attackManager = null;
    private PlayerAnimationManager animationManager = null;
    private PlayerMovementManager movementManager = null;
    private PlayerParticleController particleController = null;
    private PlayerName playerNameUI = null;

            #endregion

            #region Components

    private CharacterController characterController = null;
    private PlayerNetworkManager playerNetworkManager = null;
    private PlayerProjectileManager projectileManager = null;

    #endregion

    #endregion

    #endregion

    #region Properties

    public PlayerData PlayerData
    {
        get { return playerData; }
    }

    public Transform RangeAttackStartTr
    {
        get { return rangeAttackStartTr; }
    }

    public MeleeWeapon PlayerMeleeWeapon
    {
        get { return meleeWeapon; }
    }

        #region Manager References

    public PlayerStateMachine StateMachine
    {
        get { return stateMachine; }
    }

    public PlayerInputManager InputManager
    {
        get { return playerInputManager; }
    }

    public StatusManager StatusManager
    {
        get { return statusManager; }
    }

    public UIBattleUIManager BattleUIManager
    {
        get { return battleUIManager; }
    }

    public PlayerAnimationManager AnimationManager
    {
        get { return animationManager; }
    }

    public PlayerNetworkManager PlayerNetworkManager
    {
        get { return playerNetworkManager; }
    }

    public PlayerSkillManager SkillManager
    {
        get { return skillManager; }
    }

    public PlayerAttackManager AttackManager
    {
        get { return attackManager; }
    }

    public PlayerMovementManager MovementManager
    {
        get { return movementManager; }
    }

    public SkillUIManager SkillUIManager
    {
        get { return skillUIManager; }
    }

    public PlayerParticleController ParticleController
    {
        get { return particleController; }
    }

    public PlayerName PlayerNameUI
    {
        get { return playerNameUI; }
    }

        #endregion

    #endregion

    #region Public Functions

        #region State Functions

    public void ChangeState(PlayerStateType _type)
    {
        //if(GameManager.Instance.IsLocalGame)
        //{
        //    ChangeState_Local(_type);
        //}
        //else if(playerNetworkManager.IsClientPlayer())
        //{
        //    playerNetworkManager.NetworkChangeState(_type);
        //}

        if(GameManager.Instance.IsLocalGame || playerNetworkManager.IsClientPlayer())
            ChangeState_Local(_type);
    }

    public void ChangeState_Local(PlayerStateType _type)
    {
        StateMachine.ChangeState(_type);
    }

    public void RevivePlayer()
    {
        if (stateMachine.CurrentState.StateType != PlayerStateType.Death)
        {
            Debug.Log("Player Can't revive before died!");

            return;
        }

        if (!GameManager.Instance.IsLocalGame)
        {
            playerNetworkManager.NetworkRevivePlayer();
        }
        else
        {
            animationManager.PlayGetRevivedAnimation();
            ApplyRevive();
        }
    }

    public void ApplyRevive()
    {
        StatusManager.SetMaxHp(StatusManager.MaxHp / 2);
        StatusManager.SetCurrentHp(StatusManager.MaxHp);

        BattleUIManager.UpdatePlayerHp();
        GetComponent<CharacterController>().enabled = true;

        if(!GameManager.Instance.IsLocalGame)
        {
            PlayerNetworkManager.OnNetworkPlayerRevive?.Invoke(PlayerNetworkManager.OwnerClientId);
        }
    }

    public void OnPlayerDamageEnd()
    {
        if (statusManager.CurrentHp > 0)
            ChangeState(PlayerStateType.Idle);
    }

    #endregion

        #region Skill Functions
    public void StartSkill(SkillSlot _type)
    {
        // 멀티 게임이면서 로컬 플레이어가 아닐 때는 스킬을 사용하지 않는다.
        if (!GameManager.Instance.IsLocalGame && !PlayerNetworkManager.IsClientPlayer())
            return;

        skillManager.GetSkillData(_type).StartSkill(this);
    }

    public void EndSkill(SkillSlot _type)
    {
        // 멀티 게임이면서 로컬 플레이어가 아닐 때는 스킬을 사용하지 않는다.
        if (!GameManager.Instance.IsLocalGame && !PlayerNetworkManager.IsClientPlayer())
            return;

        skillManager.GetSkillData(_type).EndSkill(this);
    }
    public void UseSkill(SkillSlot _slot)
    {
        // 멀티 게임이면서 로컬 플레이어가 아닐 때는 스킬을 사용하지 않는다.
        if (!GameManager.Instance.IsLocalGame && !PlayerNetworkManager.IsClientPlayer())
            return;

        skillManager.SkillAction(_slot);
    }

    #endregion

    public void ShowDamagedUI()
    {
        battleUIManager.ShowDamagedUI();
    }

    #endregion

    #region Private Functions

    private void InitStates()
    {
        stateMachine = new PlayerStateMachine();

        stateMachine.AddState(PlayerStateType.Idle, new IdleState(this));
        stateMachine.AddState(PlayerStateType.Action, new ActionState(this));
        stateMachine.AddState(PlayerStateType.Death, new DeathState(this));
        stateMachine.AddState(PlayerStateType.Damaged, new DamagedState(this));
        stateMachine.AddState(PlayerStateType.Dash, new DashState(this));
        stateMachine.AddState(PlayerStateType.Skill, new SkillState(this));
    }

    private void InitManagers()
    {
        skillManager = new PlayerSkillManager();
        skillManager.Init(this);

        statusManager = new StatusManager();
        statusManager.Init(this);

        playerInputManager = new PlayerInputManager();
        playerInputManager.Init(this);

        attackManager = new PlayerAttackManager();
        attackManager.Init(this);

        animationManager = new PlayerAnimationManager();
        animationManager.Init(this);

        movementManager = new PlayerMovementManager();
        movementManager.Init(this);
    }

    #endregion

    #region Unity Callback

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerNetworkManager = GetComponent<PlayerNetworkManager>();
        particleController = GetComponent<PlayerParticleController>();
        playerNameUI = GetComponentInChildren<PlayerName>();
        projectileManager = GetComponent<PlayerProjectileManager>();

        InitStates();

        InitManagers();
    }

    private void Start()
    {
        stateMachine.ChangeState(PlayerStateType.Idle);

        if (GameManager.Instance.IsLocalGame || PlayerNetworkManager.IsOwner)
        {
            if(battleUIManager != null)
                battleUIManager.transform.parent.gameObject.SetActive(true);

            characterController.enabled = true;

            InputManager.StartInputBufferPop();
        }
    }

    private void Update()
    {
        stateMachine.UpdateState();

        skillManager.DecreaseCoolTimes(Time.deltaTime);


    }

    #endregion
}
