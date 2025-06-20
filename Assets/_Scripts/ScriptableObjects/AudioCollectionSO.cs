using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents a collection of AudioSOs. It allows you to easily retrieve audio clips based on their type.
/// </summary>
[CreateAssetMenu(fileName = "AudioCollectionSO", menuName = "SO/AudioCollectionSO")]
public class AudioCollectionSO : ScriptableObject
{
    [SerializeField] private AudioSO _defaultAudio;
    [SerializeField] private List<AudioSO> _audioSOCollection = new List<AudioSO>();

    public AudioSO GetAudio(AudioClipType audioType)
    {
        foreach (var audioSO in _audioSOCollection)
        {
            if (audioSO.AudioClipType == audioType)
            {
                return audioSO;
            }
        }

        return _defaultAudio;
    }

}
