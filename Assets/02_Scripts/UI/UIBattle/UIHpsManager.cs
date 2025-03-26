using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHpsManager : MonoBehaviour
{
    public int maxHp = 0;
    public int curHp = 0;
    private List<Image> images = new List<Image>();
    private TextMeshProUGUI textMeshPro = null;

    private void Awake()
    {
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

    public void SetCurrentHp(int _hp)
    {
        curHp = Mathf.Clamp(_hp, 0, maxHp);
    }

    public void Damage(int _Damage)
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
        Vector2 newCurHP = images[0].rectTransform.sizeDelta;
        float hpRatio = (float)curHp / (float)maxHp;
        newCurHP.x = hpRatio * newCurHP.x;  // X 값만 변경
        images[images.Count - 1].rectTransform.sizeDelta = newCurHP;  // 변경된 sizeDelta 적용
    }//UI 표시 함수
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
