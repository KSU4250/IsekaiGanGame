using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBossHpsManager : MonoBehaviour
{
    public int maxHp = 0;
    public int curHp = 0;

    private List<Image> images = new List<Image>();
    private TextMeshProUGUI textMeshPro = null;
    public RectTransform rectTransform = null; // 흔들릴 RectTransform
    public float shakeAmount = 3f;     // 흔드는 강도
    public float shakeDuration = 0.5f;  // 흔드는 시간
    private const int maxHpIndex = 0;
    private const int curHpIndex = 1;
    private const int bossImgBackgroundIndex = 2;
    private const int bossImgIndex = 3;
    private void Awake()
    {
        // rectTransform을 이 오브젝트의 RectTransform으로 설정
        rectTransform = GetComponent<RectTransform>();
        images = GetComponentsInChildren<Image>().ToList();
        textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        foreach (Image img in images)
        {
            img.raycastTarget = false;
        }
        textMeshPro.raycastTarget = false;
    }

    public void SetMaxHp(int _maxHp)
    {
        maxHp = _maxHp;
        curHp = _maxHp;
    } // 최대체력 설정 그리고 최대체력 수치를 현재체력 수치로 만드는 함수 

    public void SetCurrentHp(int _curHp)
    {
        curHp = Mathf.Clamp(_curHp, 0, maxHp);
    }
    public void BossDamage(int _Damage)
    {
        if (IsAlive())
        {
            curHp -= _Damage;
            if (IsDead())
            {
                curHp = 0;
            }
        }
    } //데미지 주는 함수

    public void HpBarUIUpdate()
    {
        textMeshPro.text = (curHp + "/" + maxHp); // 텍스트 업데이트 "현재체력/최대체력"
        Vector2 newCurHP = images[curHpIndex].rectTransform.sizeDelta;
        Vector2 maxHpImage = images[maxHpIndex].rectTransform.sizeDelta;
        float hpRatio = (float)curHp / (float)maxHp;
        newCurHP.x = hpRatio * maxHpImage.x;  // X 값만 변경
        images[curHpIndex].rectTransform.sizeDelta = newCurHP;  // 변경된 sizeDelta 적용
        StartCoroutine(Shake());//보스체력바 흔들기
    }//UI 표시 함수
    IEnumerator Shake()
    {
        Vector3 originalPos = rectTransform.localPosition;  // 원래 위치를 저장

        float elapsed = 0f;  // 경과 시간
        while (elapsed < shakeDuration)
        {
            float xOffset = Random.Range(-shakeAmount, shakeAmount); // 좌우로 랜덤한 값
            rectTransform.localPosition = originalPos + new Vector3(xOffset, 0f, 0f); // 위치 변경

            elapsed += Time.deltaTime; // 시간 증가
            yield return null; // 한 프레임 기다림
        }

        rectTransform.localPosition = originalPos; // 원래 위치로 복원
    }
    #region 상태 체크
    public bool IsAlive()
    {
        if(curHp > 0)
            return true;
        else
            return false;
    } //현재 체력 0 초과면 true
    public bool IsDead()
    {
        if (curHp <= 0)
            return true;
        else
            return false;
    }//현재 체력 0 이하이면 true
    #endregion
}
