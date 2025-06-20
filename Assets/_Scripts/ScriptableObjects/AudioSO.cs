using UnityEngine;

public enum AudioClipType
{
    None,
    Hit,
    EpicHit,
    Music,
}

/// <summary>
/// Atomic class that holds audio clip data.
/// </summary>
[CreateAssetMenu(fileName = "AudioSO", menuName = "SO/AudioSO")]
public class AudioSO : ScriptableObject
{
    [SerializeField] private AudioClip _targetAudio;
    [SerializeField] private AudioClipType _audioClipType;
    public AudioClip TargetAudio => _targetAudio;
    public AudioClipType AudioClipType => _audioClipType;
}
