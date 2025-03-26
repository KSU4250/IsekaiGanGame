using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossSkillManager : MonoBehaviour
{
    [SerializeField] private List<BossSkillData> skillDatas;
    [SerializeField] private List<BossSkillData> randomSkillDatas;

    private List<BossSkill> skills = new List<BossSkill>();
    private List<BossSkill> randomSkills = new List<BossSkill>();

    public List<BossSkill> Skills { get { return skills; } }
    public List<BossSkill> RandomSkills { get { return randomSkills; } }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        foreach (BossSkillData skillData in skillDatas)
        {
            skills.Add(new BossSkill(skillData));
        }

        foreach (BossSkillData skillData in randomSkillDatas)
        {
            randomSkills.Add(new BossSkill(skillData));
        }
    }

    // 스킬 쿨다운인지 확인하는 함수
    public List<BossSkill> IsSkillCooldown(List<BossSkill> _skills)
    {
        List<BossSkill> availList = new List<BossSkill>();

        foreach (BossSkill skill in _skills)
        {
            if (skill.CheckCooldown())
            {
                availList.Add(skill);
            }
        }

        return availList;
    }

    // 스킬 사거리 안에 있는지 확인하는 함수
    public List<BossSkill> IsSkillInRange(float _range, List<BossSkill> _skills)
    {
        List<BossSkill> availList = new List<BossSkill>();

        foreach (BossSkill skill in _skills)
        {
            if (skill.CheckRange(_range))
            {
                availList.Add(skill);
            }
        }
        return availList;
    }

    // 백어택 가능한지 Check
    public List<BossSkill> CheckBackAttack(List<BossSkill> _skills, GameObject[] _players, GameObject _boss)
    {
        int cnt = 0;

        List<GameObject> playerInRange = new List<GameObject>();

        // 1. 범위안에 2명이상 있는지 check
        foreach (GameObject player in _players)
        {
            if (player == null) continue;

            if (CheckDisXZ(player.transform.position, _boss.transform.position) <= skills.FirstOrDefault(skill => skill.SkillData.SkillName == "Attack9").SkillData.AttackRange)
            {
                cnt++;
                playerInRange.Add(player);
            }
        }

        if (cnt <= 2)
        {
            _skills.RemoveAll(skill => skill.SkillData.SkillName == "Attack9");
            return _skills;
        }

        bool plus = false;
        bool minus = false;
        float value = 0f;

        // 2. 범위안에 Player에 대해서 Vector3.Dot값을 계산해서, +와 -가 둘다 존재하는지 check
        foreach(GameObject player in playerInRange)
        {
            value = CheckBehindObject(_boss.transform, player.transform);

            if (value > 0f) plus = true;

            if (value < 0f) minus = true;
        }

        // 존재한다면 그냥 리턴
        if (plus && minus)
        {
            return _skills;
        }
        else // 존재 안하면 빼고 리턴
        {
            _skills.RemoveAll(skill => skill.SkillData.SkillName == "Attack9");
            return _skills;
        }

    }

    // 두 위치 거리 계산
    private float CheckDisXZ(Vector3 _pos1, Vector3 _pos2)
    {
        Vector2 pos1 = new Vector2(_pos1.x, _pos1.z);
        Vector2 pos2 = new Vector2(_pos2.x, _pos2.z);

        return Vector2.Distance(pos1, pos2);
    }

    // 앞인지 뒤인지 계산
    private float CheckBehindObject(Transform _bossTr, Transform _playerTr)
    {
        Vector3 toPlayer = _playerTr.position - _bossTr.position;

        return Vector3.Dot(toPlayer.normalized, -_bossTr.forward);
    }
}
