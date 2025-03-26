using UnityEngine;

using EnumTypes;
using StructTypes;

public class SkillUI_AOE : SkillUI_Base
{
    [SerializeField]
    private RectTransform targetRectTr = null;
    [SerializeField]
    private GameObject targetImageObj = null;
    [SerializeField]
    private GameObject rangeImageObj = null;

    [SerializeField]
    private float maxRangeRadius = 5f;

    private readonly float defaultY = 0.1f;

    public override void AimSkill(float _horizontal, float _vertical)
    {
        SetTargetPosition(new Vector3(_horizontal, defaultY, _vertical));
    }

    public override void AimSkill(Vector3 position)
    {
        position.y = transform.position.y;

        if(Vector3.Distance(position, transform.position) > (maxRangeRadius / 2f))
        {
            Vector3 dir = position - transform.position;
            dir.Normalize();
            targetRectTr.position = transform.position + dir * (maxRangeRadius / 2f);
        }
        else
            targetRectTr.position = position;
    }

    public override SkillPointData GetSkillAimPoint()
    {
        SkillPointData pointData = new SkillPointData();
        pointData.type = SkillPointType.Position;
        pointData.skillUsedPosition = targetRectTr.position;
        return pointData;
    }

    public override void SetEnabled(bool _enabled)
    {
        targetImageObj.SetActive(_enabled);
        rangeImageObj.SetActive(_enabled);
    }
    public override bool IsEnabled()
    {
        return targetImageObj.activeSelf;
    }

    /// <summary>
    /// 대상 지점으로 타겟 지정 스킬 UI를 옮긴다.
    /// </summary>
    /// <param name="_pos">옮기고자 하는 대상 지점</param>
    private void SetTargetPosition(Vector3 _pos)
    {
        _pos *= (maxRangeRadius / 2f);
        _pos += transform.position;

        targetRectTr.position = _pos;
    }

    private void Start()
    {
        rangeImageObj.GetComponent<RectTransform>().sizeDelta = new Vector2(maxRangeRadius, maxRangeRadius);
    }
}
