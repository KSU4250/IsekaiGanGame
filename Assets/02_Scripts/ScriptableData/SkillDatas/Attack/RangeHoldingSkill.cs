using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RangeHoldingSkill", menuName = "Scriptable Objects/Player Skill/RangeHolding")]
public class RangeHoldingSkill : RangeSkill
{
    [Header("Range Holding Skill")]
    public float duration = 2f;

    public override void StartSkill(PlayerManager _player)
    {
        base.StartSkill(_player);
    }

    public override void EndSkill(PlayerManager _player)
    {
        base.EndSkill(_player);
    }
}
