using Unity.Netcode;
using UnityEngine;

public class SelectBossScene : NetworkBehaviour
{
    public void SelectGolemScene()
    {
        if (IsHost)
        {
            RoomManager.Instance.gameSceneName = "GolemSceneTest";
        }
    }

    public void SelectMushScene()
    {
        if (IsHost)
        {
            RoomManager.Instance.gameSceneName = "MushRoomSceneTest";
        }
        
    }
}
