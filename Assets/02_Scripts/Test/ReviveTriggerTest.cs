using UnityEngine;

using EnumTypes;

public class ReviveTriggerTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider _other)
    {
        PlayerManager playerManager = _other.GetComponent<PlayerManager>();

        if (playerManager != null)
        {
            playerManager.BattleUIManager.SetSkillButtonEnabled(SkillSlot.Revive, true);
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        PlayerManager playerManager = _other.GetComponent<PlayerManager>();

        if (playerManager != null)
        {
            playerManager.BattleUIManager.SetSkillButtonEnabled(SkillSlot.Revive, false);
        }
    }
}
