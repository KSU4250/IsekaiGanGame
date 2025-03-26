using UnityEngine;

using EnumTypes;

public class PlayerCheatManager : MonoBehaviour
{
    private PlayerManager playerManager = null;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            playerManager.StatusManager.OnDamaged(10);
            Debug.LogFormat("Current Health : {0}", playerManager.StatusManager.CurrentHp);
            if(playerManager.BattleUIManager != null)
                playerManager.BattleUIManager.UpdatePlayerHp();
        }
    }
}
