using UnityEngine;

public class BossSkill
{
    private BossSkillData skillData;
    private float lastUsedTime;
    private float cooldownModifier = 1.0f;

    public BossSkillData SkillData {  get { return skillData; } }
    public float CooldownModifier { set { cooldownModifier = value; } }


    public BossSkill(BossSkillData _data)
    {
        skillData = _data;
        lastUsedTime = -skillData.CoolDown;
    }

    // 스킬쿨 돌림.
    public void UseSkill()
    {
        if (CheckCooldown())
        {
            lastUsedTime = Time.time;
        }
    }

    // 스킬 쓸수있는지 check
    public bool CheckCooldown()
    {
        return Time.time >= lastUsedTime + (skillData.CoolDown * cooldownModifier);
    }

    // 사거리 체크
    public bool CheckRange(float _range)
    {
        if (skillData.MinRange <= _range && skillData.MaxRange >= _range)
        {
            return true;
        }

        return false;
    }
}
