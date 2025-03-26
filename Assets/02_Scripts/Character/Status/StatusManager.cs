using System.Collections.Generic;
using UnityEngine;

using EnumTypes;

public class StatusManager
{
    private PlayerManager playerMng = null;

    private int maxHp = 0;
    private int currentHp = 0;

    public int MaxHp
    {
        get { return maxHp; }
    }

    public int CurrentHp
    {
        get { return currentHp; }
    }

    public void Init(PlayerManager _playerMng)
    {
        playerMng = _playerMng;
        maxHp = _playerMng.PlayerData.maxHp;
        currentHp = maxHp;
    }

    public void SetCurrentHp(int _hp)
    {
        currentHp = Mathf.Clamp(_hp, 0, maxHp);

        if (currentHp <= 0)
            playerMng.ChangeState(PlayerStateType.Death);
    }

    public void SetMaxHp(int _maxHp)
    {
        maxHp = Mathf.Clamp(_maxHp, 1, maxHp);
        
        if (currentHp > maxHp)
            SetCurrentHp(maxHp);
    }

    /// <summary>
    /// 오너에게 데미지를 가하는 함수.
    /// </summary>
    /// <param name="_damage"></param>
    public void OnDamaged(int _damage)
    {
        SetCurrentHp(currentHp - _damage);
    }
}
