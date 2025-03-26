using UnityEngine;

public class CheatManager : MonoBehaviour
{
    [SerializeField]
    private PlayerManager playerManager = null;
    [SerializeField]
    private GameManager gameManager = null;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            gameManager.ApplyDamageToPlayer(playerManager, 20);
        }
    }
}
