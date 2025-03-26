using System.Collections.Generic;
using UnityEngine;

using EnumTypes;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public CharacterClass characterClass;

    public int maxHp = 100;

    public float walkSpeed = 3f;

    public List<PlayerSkillBase> skills = null;
}
