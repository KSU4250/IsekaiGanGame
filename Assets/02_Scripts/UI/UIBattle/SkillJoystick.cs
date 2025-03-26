using UnityEngine;
using UnityEngine.EventSystems;

public class SkillJoystick : FixedJoystick
{
    // 이렇게 가져오면 아마도 멀티 동기화 부분에서 문제가 생길 수도 있음.
    // 우선은 기능 구현을 빠르게 하는 것에 중점을 두기로 하고 구현하겠음.
    private PlayerManager playerManager = null;

    private ButtonSetting buttonSetting = null;

    public override void OnPointerDown(PointerEventData eventData)
    {
        if(playerManager.SkillManager.IsSkillUsable(buttonSetting.ButtonSkillType))
        {
            base.OnPointerDown(eventData);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
    }

    private void Awake()
    {
        playerManager = FindAnyObjectByType<PlayerManager>();
        buttonSetting = GetComponent<ButtonSetting>();
    }
}
