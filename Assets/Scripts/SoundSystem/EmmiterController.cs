using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EmmiterController : MonoBehaviour {
    AudioSource m_audioSource;
    private AudioBus _audioBus;
    private float _clipVolume = 1f;
    private float _busVolume = 1f;
    private float _playbackMultiplier = 1f;
    private bool _retainUntilStopped;
    private PlaybackState _state;

    private enum PlaybackState { Idle, Scheduled, Playing, Completed }

    public bool IsMusic => _audioBus == AudioBus.Music;

    private void Awake() {
        m_audioSource = GetComponent<AudioSource>();
    }
	
    void Update(){
        if (_state == PlaybackState.Idle || m_audioSource == null) return;
        if (AudioListener.pause) return;
        if (_state == PlaybackState.Scheduled) {
            // A scheduled source is not isPlaying until its DSP start; keep it pooled meanwhile.
            if (m_audioSource.isPlaying) _state = PlaybackState.Playing;
            return;
        }
        if (_state == PlaybackState.Playing && !m_audioSource.isPlaying) {
            if (_retainUntilStopped)
                _state = PlaybackState.Completed;
            else
                ReturnToPool();
        }
    }

    public void SetupSound(Sound sound, float volume) {
        if (sound == null) throw new System.ArgumentNullException(nameof(sound));
        SetupClip(sound.clip, sound.volume, volume, AudioBus.Sfx, sound.pitch, sound.loop);
    }

    public void SetupClip(AudioClip clip, float clipVolume, float busVolume,
        AudioBus audioBus, float pitch = 1f, bool loop = false,
        bool retainUntilStopped = false) {
        if (m_audioSource == null) m_audioSource = GetComponent<AudioSource>();
        if (clip == null) throw new System.ArgumentNullException(nameof(clip));
        _clipVolume = Mathf.Clamp01(clipVolume);
        _busVolume = Mathf.Clamp01(busVolume);
        _playbackMultiplier = 1f;
        _retainUntilStopped = retainUntilStopped;
        _audioBus = audioBus;
        _state = PlaybackState.Idle;
        m_audioSource.clip = clip;
        m_audioSource.volume = _clipVolume * _busVolume;
        m_audioSource.pitch = Mathf.Clamp(pitch, 0.01f, 3f);
        m_audioSource.loop = loop;
        m_audioSource.playOnAwake = false;
        m_audioSource.ignoreListenerPause = false;
        m_audioSource.spatialBlend = 0;
        m_audioSource.spatialize = false;
    }

    public void PlaySound() {
        m_audioSource.Play();
        _state = PlaybackState.Playing;
    }

    public void PlayScheduled(double dspStartTime) {
        if (m_audioSource == null || m_audioSource.clip == null)
            throw new System.InvalidOperationException("Configura el emisor antes de programarlo.");
        _state = PlaybackState.Scheduled;
        m_audioSource.PlayScheduled(dspStartTime);
    }

    public void StopSound() {
        if (_state == PlaybackState.Idle) return;
        if (m_audioSource != null) m_audioSource.Stop();
        ReturnToPool();
    }

    public void UpdateVolume(float volumen){
        _busVolume = Mathf.Clamp01(volumen);
        ApplyVolume();
    }

    public void SetPlaybackMultiplier(float multiplier) {
        _playbackMultiplier = Mathf.Clamp01(multiplier);
        ApplyVolume();
    }

    private void ApplyVolume() {
        if (m_audioSource != null)
            m_audioSource.volume = _clipVolume * _busVolume * _playbackMultiplier;
    }

    private void ReturnToPool() {
        _state = PlaybackState.Idle;
        if (SoundPool.Instance != null && gameObject.activeInHierarchy)
            SoundPool.Instance.ReturnEmmiter(this);
    }

    private void OnDisable() {
        _state = PlaybackState.Idle;
    }
}
