using UnityEngine;

[CreateAssetMenu(fileName = "BossSkill Data(Duration)", menuName = "Scriptable Object/BossSkill Data(Duration)", order = int.MaxValue)]
public class BSD_Duration : BossSkillData
{
    [SerializeField]
    private float duration;

    public float Duration { get { return duration; } }
}
