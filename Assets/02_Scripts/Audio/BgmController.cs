using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BgmController : MonoBehaviour
{

    public List<BgmCharacterTheme> bgmCharacterThemes = new List<BgmCharacterTheme>();
    public int curCharacterIndex = 0;
    public List<BgmBossTheme> bgmBossThemes = new List<BgmBossTheme> ();
    public int curBgmBossIndex = 0;
    private List<BgmExciter> bgmExciters = new List<BgmExciter> ();
    public float excitedLevel = 0;
    public BgmDefeat bgmDefeat = null;
    public BgmVictory bgmVictory = null;
    public AudioSource curPlayingAudio = null;
    public List<AudioSource> audioList = null;
    public float maxVolumeSound = 1;//볼륨 올리는거 최대치
    private bool isFading = false;  // 상태 관리 변수
    private void Awake()
    {
        audioList = GetComponentsInChildren<AudioSource>().ToList(); // 전체 오디오 리스트
        foreach (AudioSource audio in audioList) 
        { 
            audio.spatialBlend = 0f; // 거리 상관없이 일정한 소리로 들리게 해줌
        }
        bgmCharacterThemes = GetComponentsInChildren<BgmCharacterTheme>().ToList(); // 캐릭터 테마 리스트
        bgmBossThemes = GetComponentsInChildren<BgmBossTheme>().ToList(); // 보스 테마 리스트
        bgmExciters = GetComponentsInChildren<BgmExciter>().ToList(); // 보스 테마 효과컨트롤러 리스트 
        bgmVictory = GetComponentInChildren<BgmVictory>(); // 승리 Bgm
        bgmDefeat = GetComponentInChildren<BgmDefeat>(); // 패배 Bgm
        curBgmBossIndex *= 2; // 보스마다 분노모드 일반모드 구분을 해놓을거기 때문에 인덱스 *2 를 해줘서 관리하기 용이하게 만듬
    }

    private void Start()
    {
        // 현재 캐릭터 테마 오디오 소스 시작
        PlayCharacterBgm();
    }
    private void Update()
    {

#if DEBUG //빌드하면 비활성화되는 코드
        if (Input.GetKeyDown(KeyCode.B))
        {
            PlayCharacterBgm();
        }// 플레이어 테마
        if (Input.GetKeyDown(KeyCode.N))
        {
            PlayBossBgm();
        }// 보스 테마
        if (Input.GetKeyDown(KeyCode.M))
        {
            PlayBossRageBgm();
        }// 보스 테마 +1
        if (Input.GetKeyDown(KeyCode.C))
        {
            PlayDefeat();
        }// 패배 Bgm
        if (Input.GetKeyDown(KeyCode.V))
        {
            PlayVictory();
        }// 승리 Bgm
#endif
    }
    public void SetCurCharacterIndex(int _int)
    {
        curCharacterIndex = _int;
    }
    #region 재생
    private void PlayCharacterBgm()      //캐릭터 테마 재생
    {
        // 현재 오디오를 멈추고
        if (curPlayingAudio)
        {
            StartCoroutine(FadeOutAudio(curPlayingAudio, 1f)); // 1초 동안 페이드 아웃
        }
        //캐릭터 테마 재생
        curPlayingAudio = bgmCharacterThemes[curCharacterIndex].GetComponent<AudioSource>();
        curPlayingAudio.Play();
        StartCoroutine(FadeInAudio(curPlayingAudio, 1f)); // 1초 동안 페이드 인
        // 캐릭터 테마가 끝난 후 보스 테마 자동 재생
        StartCoroutine(PlayBossBgmAfterDelay(curPlayingAudio.clip.length));

    }
    private IEnumerator PlayBossBgmAfterDelay(float delay)
    {
        // 캐릭터 테마의 길이만큼 대기
        yield return new WaitForSeconds(delay);

        // 캐릭터 테마가 끝났다면 보스 테마 재생
        PlayBossBgm();
    }
    private void PlayBossBgm()      //보스 테마 재생
    {
        // 현재 오디오를 멈추고
        if (curPlayingAudio)
        {
            StartCoroutine(FadeOutAudio(curPlayingAudio, 1f)); // 1초 동안 페이드 아웃
        }
        //보스 테마 재생
        curPlayingAudio = bgmBossThemes[curBgmBossIndex].GetComponent<AudioSource>();
        curPlayingAudio.Play();
        StartCoroutine(FadeInAudio(curPlayingAudio, 1f)); // 1초 동안 페이드 인
    }
    public void PlayBossRageBgm()        //보스 테마 인덱스+1 재생
    {
        // 현재 오디오를 멈추고
        if (curPlayingAudio)
        {
            StartCoroutine(FadeOutAudio(curPlayingAudio, 1f)); // 1초 동안 페이드 아웃
        }
        //보스 테마 인덱스+1 재생
        curPlayingAudio = bgmBossThemes[curBgmBossIndex+1].GetComponent<AudioSource>();
        curPlayingAudio.Play();
        StartCoroutine(FadeInAudio(curPlayingAudio, 1f)); // 1초 동안 페이드 인
    }

    public void PlayVictory()
    {
        // 현재 오디오를 멈추고
        if (curPlayingAudio)
        {
            StartCoroutine(FadeOutAudio(curPlayingAudio, 1f)); // 1초 동안 페이드 아웃
        }
        curPlayingAudio = bgmVictory.GetComponent<AudioSource>();
        curPlayingAudio.Play();
        StartCoroutine(FadeInAudio(curPlayingAudio, 1f)); // 1초 동안 페이드 인
    } // 승리 bgm 재생
    public void PlayDefeat()
    {
        // 이미 페이드 중이라면 예외 처리
        if (isFading)
        {
            return; // 현재 페이드가 진행 중이라면 함수를 종료하고 다시 시작하지 않음
        }
        // 현재 오디오를 멈추고
        if (curPlayingAudio)
        {
            StartCoroutine(FadeOutAudio(curPlayingAudio, 1f)); // 1초 동안 페이드 아웃
        }
        curPlayingAudio = bgmDefeat.GetComponent<AudioSource>();
        curPlayingAudio.Play();
        StartCoroutine(FadeInAudio(curPlayingAudio, 1f)); // 1초 동안 페이드 인
    } // 패배 bgm 재생
    #endregion
    #region 볼륨컨트롤
    private IEnumerator FadeOutAudio(AudioSource audioSource, float duration)
    {
        isFading = true; // 페이드 시작
        float startVolume = audioSource.volume;

        // 오디오 볼륨을 서서히 줄여서 0으로 만듬
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        // 볼륨을 0으로 설정하고 오디오를 멈춤
        audioSource.Stop();
        audioSource.volume = startVolume; // 다음에 재생할 때 볼륨을 원래대로 되돌림
        isFading = false; // 페이드 완료
    }
    private IEnumerator FadeInAudio(AudioSource audioSource, float duration)
    {
        isFading = true; // 페이드 시작
        audioSource.volume = 0; // 처음엔 볼륨을 0으로 설정

        // 볼륨을 서서히 증가시킴
        while (audioSource.volume < 1)
        {
            audioSource.volume += Time.deltaTime / duration;
            yield return null;
        }

        // 볼륨을 완전히 max로 설정
        audioSource.volume = maxVolumeSound;
        isFading = false; // 페이드 완료
    }
    #endregion
    #region excited Level Controll 보스 흥분도에 따른 bgm 음정 로우패스 필터 변화, 보스 피가 적을 수 록 증가하게 만들어주세요
    public void ExcitedLevel(float _level)
    {
        if(_level < 0 || _level > 1)
        {
            Debug.Log("excitedLevel Range is 0 ~ 1");
            return;
        }
        excitedLevel = _level;
        foreach(BgmExciter bgmExciter in bgmExciters)
        {
            bgmExciter.excitedLevel = excitedLevel;
        }
    }
    #endregion
}

