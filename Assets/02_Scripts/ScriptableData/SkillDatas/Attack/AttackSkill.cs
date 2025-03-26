using UnityEngine;

using EnumTypes;

public abstract class AttackSkill : PlayerSkillBase
{
    [Header("Attack Skill")]
    public int damage = 1;
    public float aggro = 1f;
    public bool isBackattackEnable = false;
    public int backAttackTimes = 3;

    protected int bossLayerMask = 0;

    protected void OnEnable()
    {
        bossLayerMask = 1 << LayerMask.NameToLayer("Boss");
    }

    public override void StartSkill(PlayerManager _player)
    {
        base.StartSkill(_player);

        _player.ChangeState(PlayerStateType.Skill);
    }

    public override void EndSkill(PlayerManager _player)
    {
        base.EndSkill(_player);

        _player.ChangeState(PlayerStateType.Idle);
    }

    public int DamageCalculate(PlayerManager _player)
    {
        if (isBackattackEnable && _player.AttackManager.IsPlayerBehindBoss())
        {
            return damage * backAttackTimes;
        }
        else
        {
            return damage;
        }
    }
}
