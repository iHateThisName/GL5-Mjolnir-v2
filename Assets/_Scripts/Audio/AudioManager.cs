using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Central audio manager responsible for all game audio.
///
/// Responsibilities:
/// - Plays and crossfades background music using two dedicated AudioSources.
/// - Plays overlapping sound effects using a pooled set of AudioSources.
/// - Reuses AudioSources through Unity's ObjectPool to minimize allocations.
/// - Uses DOTween for smooth music fade transitions.
///
/// Design:
/// - Music uses two persistent AudioSources so tracks can crossfade seamlessly.
/// - Sound effects borrow an AudioSource from the pool, play, then automatically
///   return it when finished.
/// </summary>
[DefaultExecutionOrder(-10)]
public class AudioManager : Singleton<AudioManager> {
    protected override bool IsPersistent => true; // Make it a perssistent singelton.

    // Need to soudtrack sources to be able to crossfade between them
    [SerializeField] private AudioSource soundTrackSource1;
    [SerializeField] private AudioSource soundTrackSource2;
    [SerializeField] private bool isSoundTrack1Main = true;

    private ObjectPool<AudioSource> sfxPool;
    private readonly List<AudioSource> activeSFXSources = new List<AudioSource>();
    [SerializeField] private AudioSource sfxAudioPrefab;

    //[Header("Frequent Audio Clips")]
    //[field:SerializeField] public AudioClip AudioButtonPressed { get; private set; }
    //[field:SerializeField, UnityEngine.Range(0f, 1f)] public float AudioButtonPressedVolume { get; private set; } = 0.5f;

    private void Start() {
        InitializePool();
    }

    private AudioSource CreateAudioSource() {
        AudioSource newAudioSource = Instantiate(sfxAudioPrefab);
        newAudioSource.gameObject.SetActive(false);
        newAudioSource.transform.SetParent(this.transform);
        return newAudioSource;
    }

    #region Object Pooling

    private void InitializePool() {
        this.sfxPool = new ObjectPool<AudioSource>(
            createFunc: CreateAudioSource,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: 5,
            maxSize: 20
        );
    }

    private void OnDestroyPoolObject(AudioSource audioSource) {
        Destroy(audioSource.gameObject);
    }

    private void OnReturnToPool(AudioSource audioSource) {
        ReSetAudioSource(audioSource);
        audioSource.gameObject.SetActive(false);
        this.activeSFXSources.Remove(audioSource);
    }

    private void OnTakeFromPool(AudioSource audioSource) {
        audioSource.gameObject.SetActive(true);
        this.activeSFXSources.Add(audioSource);
    }

    private static void ReSetAudioSource(AudioSource audioSource) {
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.volume = 1f;
        audioSource.pitch = 1f;
        audioSource.loop = false;
    }

    #endregion

    /// <summary>
    /// Plays a background music track using a double AudioSource setup.
    /// 
    /// The next track is loaded into the inactive AudioSource and faded in while
    /// the currently playing track fades out, allowing seamless music transitions.
    /// Existing DOTween fade operations are stopped before starting new fades to
    /// prevent overlapping volume changes.
    /// </summary>
    /// <param name="clip">The music track to play.</param>
    /// <param name="volume">The playback volume multiplier.</param>
    /// <param name="fadeTime">The duration of the crossfade in seconds.</param>
    public void PlaySoundTrack(AudioClip clip, float volume = 1f, float fadeTime = 1f) {
        AudioSource active = isSoundTrack1Main ? soundTrackSource1 : soundTrackSource2;
        AudioSource next = isSoundTrack1Main ? soundTrackSource2 : soundTrackSource1;

        // Skip if the requested clip is already playing on the active AudioSource
        if (active.clip == clip && active.isPlaying) {
            return;
        }

        this.isSoundTrack1Main = !this.isSoundTrack1Main;

        next.clip = clip;
        next.volume = 0;
        next.Play();

        // Kill any existing tweens to avoid overlapping fades
        active.DOKill();
        next.DOKill();

        // Fade out the active track and fade in the next track
        active.DOFade(0, fadeTime).SetTarget(active).OnComplete(() => active.Stop());
        next.DOFade(volume, fadeTime).SetTarget(next);
    }

    /// <summary>
    /// Plays a sound effect using the SFX pool without waiting for playback to finish.
    /// 
    /// This is the default method for triggering sound effects during gameplay.
    /// The AudioSource is automatically returned to the pool once the sound effect
    /// has completed playing.
    /// </summary>
    /// <param name="clip">The sound effect clip to play.</param>
    /// <param name="volume">The playback volume multiplier.</param>
    /// <param name="loop">Whether to loop the sound effect.</param>
    public AudioSource PlaySFX(AudioClip clip, float volume = 1f, bool loop = false) {
        _ = PlaySFXAsync(clip, volume, loop); // Fire-and-forget async call to play the sound effect
        return this.activeSFXSources.Count > 0 ? this.activeSFXSources[this.activeSFXSources.Count - 1] : null;
    }

    /// <summary>
    /// Plays a sound effect using an AudioSource retrieved from the SFX pool and
    /// waits asynchronously until playback has finished.
    /// 
    /// The AudioSource is configured with the provided clip, volume, and a small
    /// random pitch variation before playback. Once the sound effect finishes,
    /// the AudioSource is automatically returned to the pool.
    /// 
    /// This method is useful for timed sequences, cutscenes, or gameplay events
    /// where execution needs to continue after the sound effect completes.
    /// </summary>
    /// <param name="clip">The sound effect clip to play.</param>
    /// <param name="volume">The playback volume multiplier.</param>
    /// <param name="loop">Whether to loop the sound effect.</param>
    public async Awaitable PlaySFXAsync(AudioClip clip, float volume = 1f, bool loop = false) {
        AudioSource src = this.sfxPool.Get();

        src.clip = clip;
        src.volume = volume;
        src.pitch = Random.Range(0.95f, 1.05f); // Add a slight random pitch variation for more natural sound
        src.loop = loop;
        src.Play();

        await ReturnSFXWhenFinishedAsync(src);
    }

    public void StopAllSFX() {
        foreach (AudioSource source in activeSFXSources) {
            source.Stop();
        }
    }

    private async Awaitable ReturnSFXWhenFinishedAsync(AudioSource source) {
        while (source.isPlaying && source.gameObject.activeSelf) {
            await Awaitable.NextFrameAsync();
        }
        this.sfxPool.Release(source);
    }
}
