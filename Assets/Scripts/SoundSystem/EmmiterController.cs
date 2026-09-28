using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EmmiterController : MonoBehaviour {
    AudioSource m_audioSource;
    Sound m_sound;

    private void Awake() {
        m_audioSource = GetComponent<AudioSource>();
    }
	
    void Update(){
        if(m_sound == null) return;
        if(!m_audioSource.isPlaying){
            m_sound = null;
            if(SoundPool.Instance != null)
                SoundPool.Instance.ReturnEmmiter(this);
        }
    }

    public void SetupSound(Sound sound, float volume) {
        if (m_audioSource == null) m_audioSource = GetComponent<AudioSource>();
        m_sound 				= sound;
        m_audioSource.clip 		= m_sound.clip;
        m_audioSource.volume 	= m_sound.volume * volume;
        m_audioSource.pitch 	= m_sound.pitch;
        m_audioSource.loop 		= m_sound.loop;
        m_audioSource.spatialBlend = 0;
        m_audioSource.spatialize = false;
    }

    public void PlaySound() {
        m_audioSource.Play();
    }

    public void StopSound() {
        m_audioSource.Stop();
        m_sound = null;
        if(SoundPool.Instance != null && gameObject.activeInHierarchy)
            SoundPool.Instance.ReturnEmmiter(this);
    }

    public void UpdateVolume(float volumen){
        if(m_sound == null) return;
        m_audioSource.volume = m_sound.volume * volumen;
    }
}
