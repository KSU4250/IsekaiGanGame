using System.Collections.Generic;
using System.Collections;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using EnumTypes;

public class ButtonSetting : MonoBehaviour
{
    [SerializeField]
    private SkillSlot buttonSkillType = SkillSlot.None;

    public List<Image> images = new List<Image>();
    public List<TextMeshProUGUI> textMeshPros = null;
    public CoolTime cooltime = null;

    private SkillButtonsManager skillButtonsManager = null;
    private FixedJoystick joystick = null;

    private Coroutine currentSkillCoroutine = null;

    public SkillSlot ButtonSkillType
    {
        get { return buttonSkillType; }
    }

    private void Awake()
    {
        images = GetComponentsInChildren<Image>(true).ToList();
        textMeshPros = GetComponentsInChildren<TextMeshProUGUI>().ToList();
        cooltime = GetComponentInChildren<CoolTime>();
        joystick = GetComponent<FixedJoystick>();
        skillButtonsManager = GetComponentInParent<SkillButtonsManager>();
    }

    private void Start()
    {
        SetCooltime(0.2f);
    }

    public void SetCooltime(float _cooltime)
    {
        cooltime.SetMaxCooltime(_cooltime);
    }
    public void JoystickDown(BaseEventData _eventData)
    {
        skillButtonsManager.OnSkillJoystickDown(ButtonSkillType);
        currentSkillCoroutine = StartCoroutine(DirectionSkillHoldingCoroutine(ButtonSkillType));
    }

    public void JoystickUp(BaseEventData _eventData)
    {
        if(currentSkillCoroutine != null)
            StopCoroutine(currentSkillCoroutine);
        skillButtonsManager.OnSkillJoystickUp(ButtonSkillType);
    }

    /// <summary>
    /// 버튼 타입의 스킬이 눌러졌다가 때질 때 호출됨.
    /// </summary>
    /// <param name="_eventData"></param>
    public void ButtonUp(BaseEventData _eventData)
    {
        skillButtonsManager.OnSkillButtonUp(ButtonSkillType);
    }

    public void DisableSkillButton()
    {
        foreach(var image in images)
        {
            image.raycastTarget = false;
        }

        if (joystick != null)
        {
            joystick.enabled = false;
        }
    }

    /// <summary>
    /// 지정한 스킬의 조이스틱 좌표 값을 상위 매니저에게 계속해서 보내는 코루틴.
    /// </summary>
    /// <param name="_type">지정하고자 하는 스킬.</param>
    /// <returns></returns>
    private IEnumerator DirectionSkillHoldingCoroutine(SkillSlot _type)
    {
        while (true)
        {
            skillButtonsManager.SendSkillDirection(_type, joystick.Horizontal, joystick.Vertical);

            yield return null;
        }
    }
}
