using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerAudioManager : MonoBehaviour
{
    [Serializable]
    private class AudioData
    {
        public string sfxName = string.Empty;
        public AudioClip sfxClip = null;
        public float startTime = 0f;
    }

    [SerializeField]
    private AudioMixer audioMixer = null;

    [SerializeField]
    private List<AudioData> audioDataList = null;

    private Dictionary<string, AudioData> audioDataDic = null;

    private AudioSource playerAudioSource = null;

    public void PlaySFX(string _sfxName)
    {
        if(audioDataDic.TryGetValue(_sfxName, out AudioData sfxData))
        {
            playerAudioSource.clip = sfxData.sfxClip;
            playerAudioSource.time = sfxData.startTime;
            playerAudioSource.Play();
        }
    }

    private void Awake()
    {
        audioDataDic = new Dictionary<string, AudioData>();

        playerAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if(audioMixer != null)
        {
            AudioMixerGroup[] mixerGroup = audioMixer.FindMatchingGroups("SFX");

            if (playerAudioSource != null && mixerGroup.Length > 0)
            {
                playerAudioSource.outputAudioMixerGroup = mixerGroup[0];
            }
        }

        foreach (AudioData audioData in audioDataList)
        {
            if (!audioDataDic.ContainsKey(audioData.sfxName))
            {
                audioDataDic.Add(audioData.sfxName, audioData);
            }
        }
    }
}
