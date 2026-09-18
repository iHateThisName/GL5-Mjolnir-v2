using UnityEngine;

public class SFXController : MonoBehaviour {
    [SerializeField] private AudioClip sfxClip;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = false;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.5f;

    private AudioSource audioSourceRefrence;

    private void Start() {
        if (playOnStart) {
            PlaySFX();
        }
    }

    public void PlaySFX() {
        this.audioSourceRefrence = AudioManager.Instance.PlaySFX(clip: this.sfxClip, volume: this.sfxVolume, loop: this.loop);
    }

    public void StopSFX() {
        if (this.audioSourceRefrence != null) this.audioSourceRefrence.Stop();
    }

    public async Awaitable PlaySFXInOrderAsync(AudioClip[] clips, float[] volumes = null) {
        for (int i = 0; i < clips.Length; i++) {
            float currentVolume = volumes != null && volumes.Length > i ? volumes[i] : this.sfxVolume;
            await AudioManager.Instance.PlaySFXAsync(clips[i], volume: currentVolume);
        }
    }
}
