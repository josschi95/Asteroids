using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private void Awake()
    {
        instance = this;
    }

    private AudioSource source;

    [SerializeField] private AudioClip[] clips;
    private Dictionary<string, AudioClip> audioClips;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        audioClips = new Dictionary<string, AudioClip>();
        for (int i = 0; i < clips.Length; i++)
        {
            audioClips.Add(clips[i].name, clips[i]);
        }
    }

    public static void PlayClip(string clipName)
    {
        instance.source.PlayOneShot(instance.GetClip(clipName));
    }

    private AudioClip GetClip(string clipName)
    {
        return audioClips[clipName];
    }
}
