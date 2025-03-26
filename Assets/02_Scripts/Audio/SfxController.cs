using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SfxController : MonoBehaviour
{
    public List<AudioSource> audioList = null;
    private void Awake()
    {
        audioList = GetComponentsInChildren<AudioSource>().ToList(); // 전체 오디오 리스트
    }
    public void PlayByNum(int _num, float _delayTime)
    {
        if (_delayTime < 0)
        {
            Debug.LogWarning("Delay time cannot be negative. Using 0 instead.");
            _delayTime = 0f; // 음수일 경우 0으로 설정
        }
        if (audioList == null)
        {
            Debug.Log("No Sfx exists");
            return;
        }

        // 딜레이 후 오디오 재생을 위한 코루틴 시작
        StartCoroutine(PlayWithDelay(_num, _delayTime));
    }

    // 실제로 딜레이 후 오디오를 재생하는 코루틴
    private IEnumerator<WaitForSeconds> PlayWithDelay(int _num, float _delayTime)
    {
        yield return new WaitForSeconds(_delayTime); // _delayTime 초 동안 대기
        audioList[_num].Play(); // 오디오 재생
    }
}

