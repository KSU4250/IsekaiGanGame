using UnityEngine;

public class AnimationTriggerResetBehaviour : StateMachineBehaviour
{
    [SerializeField]
    private string triggerName;

    private int triggerId = -1;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        Debug.LogFormat("Animation By {0} Trigger Entered!", triggerName);

        if(triggerId == -1)
        {
            triggerId = Animator.StringToHash(triggerName);
        }

        animator.ResetTrigger(triggerId);
    }
}
