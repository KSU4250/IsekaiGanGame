using UnityEngine;

public class BossAudioAnimCallback : MonoBehaviour
{
    private void Start()
    {
        FindAnyObjectByType<BossStateManager>().bossStunCallback += AudioStop;
    }

    public void Attack1Audio()
    {
        BossAudioManager.Instance.AudioPlay(BossAudioManager.Instance.Attack1);
    }

    public void Attack2Audio()
    {
        BossAudioManager.Instance.AudioPlay(BossAudioManager.Instance.Attack2);
    }

    public void Attack3Audio()
    {
        BossAudioManager.Instance.AudioPlay(BossAudioManager.Instance.Attack3);
    }

    public void Attack4Audio()
    {
        BossAudioManager.Instance.AudioPlayFadeInAndOut(BossAudioManager.Instance.Attack4, 4f, 0.5f, 0.5f);
    }

    public void Attack5Audio()
    {
        BossAudioManager.Instance.AudioPlay(BossAudioManager.Instance.Attack5);
    }

    public void Attack6Audio()
    {
        BossAudioManager.Instance.AudioPlay(BossAudioManager.Instance.Attack6, 1f - 0.5f);
    }

    public void Attack7Audio()
    {
        BossAudioManager.Instance.AudioPlay(BossAudioManager.Instance.Attack7);
    }

    public void Attack8Audio()
    {
        BossAudioManager.Instance.AudioPlay(BossAudioManager.Instance.Attack8);
    }

    public void Attack9Audio()
    {
        BossAudioManager.Instance.AudioPlay(BossAudioManager.Instance.Attack9);
    }

    public void SpecialAttackAudio()
    {
        BossAudioManager.Instance.AudioPlay(BossAudioManager.Instance.SpecialAttack);
    }

    public void Phase2Audio()
    {
        BossAudioManager.Instance.AudioPlay(BossAudioManager.Instance.Phase2);
    }

    private void AudioStop()
    {
        BossAudioManager.Instance.StopAudioCoroutine();
        BossAudioManager.Instance.AudioStop(true);
    }
}
