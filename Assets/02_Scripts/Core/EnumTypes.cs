using UnityEngine;

namespace EnumTypes
{
    public enum SkillSlot
    {
        None = 0,
        Skill_A,
        Skill_B,
        Skill_C,
        BasicAttack,
        Dash,
        Revive,
    }

    public enum PlayerStateType
    {
        Idle,       // 이동 및 대기 
        Damaged,    // 피격
        Action,     // 액션을 취하는 중 (부활, 상호작용 등등)
        Dash,       // 회피
        Death,       // 사망
        Skill, // 스킬 사용 중
    }

    /// <summary>
    /// 스킬 사용 지점을 어떻게 지정하는지를 정의해주는 타입
    /// </summary>
    public enum SkillPointType
    {
        None = 0,   // 사용 지점을 지정하지 않는 스킬.
        Direction,  // 방향을 지정하고 사용하는 스킬.
        Position,   // 위치를 지정하고 사용하는 스킬.
    }

    public enum CharacterClass
    {
        Warrior, // 전사
        Mage, // 법사
        Archer, // 궁수
        Thief // 도적
    }

    public enum BossType
    {
        Golem = 0,
        Mushroom,
    }

    public enum BehaviourLogType
    {
        None = 0,
        Kill,
        Revive
    }
}