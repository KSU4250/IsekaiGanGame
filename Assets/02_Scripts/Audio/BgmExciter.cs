using UnityEngine;

public class BgmExciter : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioLowPassFilter lowPassFilter;
    [SerializeField, Range(0f, 1f)]
    public float excitedLevel = 0; // 이 수치로 LowPassFilter랑 Pitch 조정 예정
    public float previousLevel = 0;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();
        SoundExcitedLevel(1);
        SoundExcitedLevel(0);
    }

    public void SoundExcitedLevel(float _level)
    {
        if(_level < 0 || _level > 1)
        {
            Debug.Log("Only 0 ~ 1 Level is Allowed");
            return;
        }

        if (_level != previousLevel)
        {
            // 초반에 천천히 올라가고 후반에 급격히 올라가는 효과 (비선형 변화)
            float adjustedLevel = Mathf.Pow(_level, 2); // 제곱을 사용하여 점진적인 상승
            lowPassFilter.cutoffFrequency = Mathf.Lerp(3000,22000, adjustedLevel);
            audioSource.pitch = Mathf.Lerp(0.95f, 1.15f, adjustedLevel);

            // 이전 값을 현재 값으로 업데이트
            previousLevel = _level;
        }
    }

    private void Update()
    {
        SoundExcitedLevel(excitedLevel);
    }
}
