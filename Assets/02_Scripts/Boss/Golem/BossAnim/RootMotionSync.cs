using NUnit.Framework;
using UnityEngine;



public class RootMotionSync : MonoBehaviour
{
    [System.Serializable]
    public struct SetAnim
    {
        public string name;
        public float moveMultiplier;
    }

    public Animator animator;
    public SetAnim[] anims;

    private float curMoveMultiplier;

    void OnAnimatorMove()
    {
        // 현재 애니메이터 상태의 애니메이션 이름을 확인
        if (animator.applyRootMotion && CheckAnim())
        {
            // 애니메이션의 이동 값을 가져와서 배율 적용
            Vector3 newPosition = transform.position + animator.deltaPosition * curMoveMultiplier;

            // 이동 적용
            transform.position = newPosition;

            // 회전도 동기화
            transform.rotation *= animator.deltaRotation;
        }
    }

    // 설정한 애니메이션인지 check
    private bool CheckAnim()
    {
        foreach (SetAnim anim in anims)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(anim.name))
            {
                curMoveMultiplier = anim.moveMultiplier;
                return true;
            }
        }
        return false;
    }
}
