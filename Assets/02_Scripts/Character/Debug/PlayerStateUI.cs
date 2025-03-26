using UnityEngine;
using TMPro;

public class PlayerStateUI : MonoBehaviour
{
    [SerializeField]
    private PlayerManager playerManager = null;

    [SerializeField]
    private TextMeshPro stateText = null;

    private Camera mainCam = null;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        stateText.text = playerManager.StateMachine.CurrentState.StateType.ToString();
        transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward, mainCam.transform.rotation * Vector3.up);
    }
}
