using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class BossAudioManager : MonoBehaviour
{
    public static BossAudioManager Instance;

    public AudioClip Attack1;
    public AudioClip Attack2;
    public AudioClip Attack3;
    public AudioClip Attack4;
    public AudioClip Attack5;
    public AudioClip Attack6;
    public AudioClip Attack7;
    public AudioClip Attack8;
    public AudioClip Attack9;
    public AudioClip SpecialAttack;
    public AudioClip Phase2;
    public AudioClip GetHit;
    public AudioMixer audioMixer; // 임의 추가 황승원 02.14 오디오 믹서

    private AudioSource audioSource;
    private Coroutine curCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        audioSource = FindAnyObjectByType<BossStateManager>().GetComponent<AudioSource>();
        audioSource.loop = false; // 반복 재생 여부
        audioSource.playOnAwake = false; // 자동 재생 비활성화 (수동으로 제어)
        audioSource.spatialBlend = 1f; // 3D 사운드 (0 = 2D, 1 = 3D)
        audioSource.rolloffMode = AudioRolloffMode.Linear; // 소리가 거리 비례 감소
        audioSource.minDistance = 15f; // 이 거리까지는 볼륨이 유지됨
        audioSource.maxDistance = 40f; // 이 거리 이상에서는 소리가 거의 안 들림

        AudioMixerGroup[] mixerGroups = audioMixer.FindMatchingGroups("SFX"); // 임의 추가 황승원 02.14 오디오 믹서 SFX 그룹 찾기
        audioSource.outputAudioMixerGroup = mixerGroups[0]; // 임의 추가 황승원 02.14 오디오 믹서 out put 설정
    }

    // 오디오 재생 함수
    public void AudioPlay(AudioClip _audioClip)
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.clip = _audioClip;
        audioSource.loop = false;
        audioSource.Play();
    }

    // 오디오 재생 함수 딜레이 버전
    public void AudioPlay(AudioClip _audioClip, float _delay)
    {
        curCoroutine = StartCoroutine(AudioPlayDelay(_audioClip, _delay));
    }

    // 점점 커졌다가 작아지는 오디오 재생 함수
    public void AudioPlayFadeInAndOut(AudioClip _audioClip, float _duration, float _fadeIn, float _fadeOut)
    {
        curCoroutine = StartCoroutine(FadeInAndOut(_audioClip, _duration, _fadeIn, _fadeOut));
    }

    // 오디오 정지 함수
    public void AudioStop(bool isStop = true, float fadeOutTime = 0.5f)
    {
        if (isStop)
        {
            audioSource.Stop();
        }
        else
        {
            StartCoroutine(FadeOut(fadeOutTime));
        }
    }

    // 오디오 코루틴 정지 함수
    public void StopAudioCoroutine()
    {
        if (curCoroutine != null)
        {
            StopCoroutine(curCoroutine);
            curCoroutine = null;
            audioSource.volume = 1f;
        }
    }

    // 페이드인 효과 (점점 소리 커짐)
    private IEnumerator FadeIn(AudioClip _audioClip, float duration)
    {
        float startVolume = 0f;
        audioSource.volume = startVolume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 1f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 1f;
    }

    // 페이드아웃 효과 (점점 소리 작아짐)
    private IEnumerator FadeOut(float duration)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    // 페이드인 + 페이드아웃
    private IEnumerator FadeInAndOut(AudioClip _audioClip, float _duration, float fadeInDuration, float fadeOutDuration)
    {
        // 페이드인 구간
        float startVolume = 0f;
        audioSource.volume = startVolume;
        float elapsedTime = 0f;

        audioSource.clip = _audioClip;
        audioSource.loop = false;
        audioSource.Play();

        while (elapsedTime < fadeInDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 1f, elapsedTime / fadeInDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 1f;

        yield return new WaitForSeconds(_duration);

        startVolume = audioSource.volume;
        elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeOutDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();

        audioSource.volume = 1f;


    }

    private IEnumerator AudioPlayDelay(AudioClip _audioClip, float _delay)
    {
        yield return new WaitForSeconds(_delay);

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.clip = _audioClip;
        audioSource.loop = false;
        audioSource.Play();
    }
}
