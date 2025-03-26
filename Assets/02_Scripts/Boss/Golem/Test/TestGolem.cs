using UnityEngine;

public class TestGolem : MonoBehaviour
{
    public Animator anim;
    public GameObject skin;

    private AnimatorStateInfo stateinfo;
    private bool Checked = false;

    private void Update()
    {
        stateinfo = anim.GetCurrentAnimatorStateInfo(0);

        if (stateinfo.IsName("Attack7") && stateinfo.normalizedTime >= 1.0f && !Checked)
        {
            Debug.Log("»£√‚µ ");
            skin.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            anim.SetBool("Attack7-1", true);
            skin.SetActive(true);
            Checked = true;
        }
    }
}
