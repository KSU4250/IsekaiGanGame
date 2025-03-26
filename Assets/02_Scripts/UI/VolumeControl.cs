using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;  // AudioMixer 사용을 위한 네임스페이스

public class VolumeControl : MonoBehaviour
{
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    public AudioMixer audioMixer;  // 믹서

    void Start()
    {
        // 부모 오브젝트에서 모든 슬라이더를 가져옵니다.
        Slider[] sliders = GetComponentsInChildren<Slider>(true);

        // 인덱스를 사용하여 슬라이더를 참조합니다.
        masterSlider = sliders[0];  // 첫 번째 슬라이더 (마스터)
        bgmSlider = sliders[1];     // 두 번째 슬라이더 (BGM)
        sfxSlider = sliders[2];     // 세 번째 슬라이더 (SFX)

        // 초기 슬라이더 값 설정 (최대 볼륨)
        masterSlider.value = 0.5f;
        bgmSlider.value = 0.5f;
        sfxSlider.value = 0.5f;

        // 슬라이더 값이 변경될 때마다 호출될 메서드 등록
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    // 마스터 볼륨 조절
    public void OnMasterVolumeChanged(float value)
    {
        Debug.Log("Master Volume Changed: " + value);  // 슬라이더 값 확인
        if (value == 0f)
        {
            audioMixer.SetFloat("Master", -80f); // 0일 때는 -80dB로 설정 (사람의 귀에 들리지 않음)
        }
        else
        {
            audioMixer.SetFloat("Master", Mathf.Log10(value) * 20); // 음량을 dB 단위로 변환
        }
    }


    // BGM 볼륨 조절
    public void OnBGMVolumeChanged(float value)
    {
        if (value == 0f)
        {
            audioMixer.SetFloat("BGM", -80f); // 0일 때는 -80dB로 설정 (사람의 귀에 들리지 않음)
        }
        else
        {
            audioMixer.SetFloat("BGM", Mathf.Log10(value) * 20); // 음량을 dB 단위로 변환
        }
    }

    // SFX 볼륨 조절
    public void OnSFXVolumeChanged(float value)
    {
        if (value == 0f)
        {
            audioMixer.SetFloat("SFX", -80f); // SFX 볼륨을 -80dB로 설정
        }
        else
        {
            audioMixer.SetFloat("SFX", Mathf.Log10(value) * 20); // 음량을 dB로 변환
        }
    }


}
