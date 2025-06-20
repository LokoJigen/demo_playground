using com.trashpandaboy.events;
using UnityEngine;

/// <summary>
/// This class contains data related to audio tracks and manages audio playback.
/// </summary>
public class SoundManager : MonoBehaviour
{
    
    #region VARIABLES

    [SerializeField] private AudioCollectionSO _audioClipCollection;
    [SerializeField] private AudioSource _audioSourceVfx;
    [SerializeField] private AudioSource _audioSourceMusic;


    #endregion

    #region UNITY CALLBACKS

    private void OnEnable()
    {
        PlayMainMusic();
        EventDispatcher.StartListening(EventType.Collision, OnCollisionEvent);
    }

    private void OnDisable()
    {
        EventDispatcher.StopListening(EventType.Collision, OnCollisionEvent);
    }

    #endregion

    #region PRIVATE METHODS

    private void PlayMainMusic()
    {
        _audioSourceMusic.clip = _audioClipCollection.GetAudio(AudioClipType.Music).TargetAudio;
        _audioSourceMusic.Play();
    }

    private void PlayVfx()
    {
        PlayTargetVfx(_audioClipCollection.GetAudio(AudioClipType.Hit));
    }

    private void PlayEpicVfx()
    {
        PlayTargetVfx(_audioClipCollection.GetAudio(AudioClipType.EpicHit));
    }

    private void PlayTargetVfx(AudioSO audioData)
    {
        _audioSourceVfx.clip = audioData.TargetAudio;
        float pitch = Random.Range(0.9f, 1.1f);
        _audioSourceVfx.pitch = pitch;
        _audioSourceVfx.Play();
    }

    private void OnCollisionEvent(object data)
    {
        if (data is CollisionEventData collisionEventData)
        {
            if (collisionEventData.isEpicCollision)
            {
                SetRandomLowPitch();
                PlayEpicVfx();
                CancelInvoke(nameof(ResetGlobalPitch));
                Invoke(nameof(ResetGlobalPitch), 1.5f);
            }
            else
            {
                PlayVfx();
            }
        }
    }

    private void ResetGlobalPitch()
    {
        _audioSourceMusic.pitch = 1f;
        _audioSourceVfx.pitch = 1f;
    }

    private void SetRandomLowPitch()
    {
        float pitch = Random.Range(0.3f, .7f);
        _audioSourceMusic.pitch = pitch;
        _audioSourceVfx.pitch = pitch;
    }

    #endregion

}