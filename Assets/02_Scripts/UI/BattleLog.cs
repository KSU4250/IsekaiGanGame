using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using StructTypes;
using EnumTypes;

public class BattleLog : MonoBehaviour
{
    [Header("UI")]
    /// <summary>
    /// 행동 이미지
    /// </summary>
    [SerializeField]
    private LogSlot image_Behaviour = null;
    /// <summary>
    /// 주체 이미지
    /// </summary>
    [SerializeField]
    private LogSlot image_Who = null;
    /// <summary>
    /// 대상 이미지
    /// </summary>
    [SerializeField]
    private LogSlot image_Whom = null;
    /// <summary>
    /// 로그 출력 지속시간
    /// </summary>
    [SerializeField]
    private float showLogDuration = 2f;

    [Header("Data")]
    [SerializeField]
    private List<BossImageData> bossImageDataList = null;
    [SerializeField]
    private List<PlayerImageData> playerImageDataList = null;
    [SerializeField]
    private List<BehaviourImageData> behaviourImageDataList = null;

    private Dictionary<BossType, Sprite> bossImageDic = null;
    private Dictionary<CharacterClass, Sprite> CharacterImageDic = null;
    private Dictionary<BehaviourLogType, Sprite> behaviourImageDic = null;

    private Coroutine LogCoroutine = null;

    /// <summary>
    /// 사망 로그를 출력하는 함수
    /// </summary>
    public void SetKillLog(BossType _boss, CharacterClass _player, bool _isPlayerDied = true)
    {
        if(_isPlayerDied)
        {
            Debug.LogFormat("{0} image : {1}", _player, CharacterImageDic[_player]);
            image_Who.SetImage(bossImageDic[_boss]);
            image_Whom.SetImage(CharacterImageDic[_player]);
        }
        else
        {
            image_Who.SetImage(CharacterImageDic[_player]);
            image_Whom.SetImage(bossImageDic[_boss]);
        }

        image_Behaviour.SetImage(behaviourImageDic[BehaviourLogType.Kill]);
    }

    /// <summary>
    /// 부활 로그를 출력하는 함수
    /// </summary>
    /// <param name="_revive"></param>
    /// <param name="_getRevived"></param>
    public void SetReviveLog(CharacterClass _revive, CharacterClass _getRevived)
    {
        image_Who.SetImage(CharacterImageDic[_revive]);
        image_Whom.SetImage(CharacterImageDic[_getRevived]);
        image_Behaviour.SetImage(behaviourImageDic[BehaviourLogType.Revive]);
    }

    /// <summary>
    /// 로그를 출력하는 함수
    /// </summary>
    public void ShowLog()
    {
        // 로그 출력 코루틴이 아직 실행중이라면 코루틴을 정지한 후 초기화한다.
        if (LogCoroutine != null)
        {
            StopCoroutine(LogCoroutine);
            LogCoroutine = null;
        }

        // 로그를 출력한다.
        {
            image_Behaviour.gameObject.SetActive(true);
            image_Who.gameObject.SetActive(true);
            image_Whom.gameObject.SetActive(true);
        }

        // 일정 시간 후 로그를 다시 종료한다.
        LogCoroutine = StartCoroutine(HideLogCoroutine(showLogDuration));
    }

    private IEnumerator HideLogCoroutine(float _duration)
    {
        yield return new WaitForSeconds(_duration);

        // 로그를 다시 숨긴다.
        {
            image_Behaviour.gameObject.SetActive(false);
            image_Who.gameObject.SetActive(false);
            image_Whom.gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        bossImageDic = new Dictionary<BossType, Sprite>();
        CharacterImageDic = new Dictionary<CharacterClass, Sprite>();
        behaviourImageDic = new Dictionary<BehaviourLogType, Sprite>();

        foreach (var data in bossImageDataList)
        {
            bossImageDic.Add(data.bossType, data.bossImage);
        }
        foreach (var data in playerImageDataList)
        {
            CharacterImageDic.Add(data.characterClass, data.playerImage);
        }
        foreach (var data in behaviourImageDataList)
        {
            behaviourImageDic.Add(data.behaviourType, data.behaviourImage);
        }
    }
}
