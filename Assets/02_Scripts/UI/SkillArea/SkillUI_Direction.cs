using UnityEngine;

using EnumTypes;
using StructTypes;

public class SkillUI_Direction : SkillUI_Base
{
    [SerializeField]
    private RectTransform arrowImagePivot = null;

    public override void AimSkill(float _horizontal, float _vertical)
    {
        SetDirection(_horizontal, _vertical);
    }

    public override void AimSkill(Vector3 position)
    {
        Vector3 direction = (position - arrowImagePivot.transform.position).normalized;

        SetDirection(direction.x, direction.z);
    }

    public override SkillPointData GetSkillAimPoint()
    {
        SkillPointData pointData = new SkillPointData();
        pointData.type = SkillPointType.Direction;
        // pointData.skillUsedPosition = arrowImagePivot.rotation.eulerAngles;
        pointData.skillUsedRotation = arrowImagePivot.rotation;
        return pointData;
    }

    public override void SetEnabled(bool _enabled)
    {
        arrowImagePivot.gameObject.SetActive(_enabled);
    }

    public override bool IsEnabled()
    {
        return arrowImagePivot.gameObject.activeSelf;
    }

    private void SetDirection(float x, float z)
    {
        float angle = Mathf.Atan2(x, z) * Mathf.Rad2Deg;
        arrowImagePivot.rotation = Quaternion.Euler(0f, angle, 0f);
    }
}
