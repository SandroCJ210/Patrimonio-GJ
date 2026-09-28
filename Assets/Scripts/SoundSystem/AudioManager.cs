using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : PersistentSingleton<AudioManager> {
	[SerializeField] GameObject m_soundPoolPrefab;

	public Sound[] m_MusicList;
	public Sound[] m_SoundList;

	private float m_generalVolumen = 1.0f;
	private float m_musicVolume = 1.0f; //Valores de 0 a 1
	private float m_soundVolume = 1.0f; //Valores de 0 a 1

	AudioSource m_backgroundAudioSource;
	Sound m_currentBackground;

	Dictionary<string, Sound> m_musicDict;
	Dictionary<string, Sound> m_soundDict;

	protected override void Awake() {
		base.Awake();
		m_backgroundAudioSource = GetComponent<AudioSource>();
		SetupClips();
		SceneManager.sceneLoaded += OnSceneLoaded;
		EnsureSoundPool();
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		if(mode == LoadSceneMode.Additive) return;
		EnsureSoundPool();
	}

	private void OnDestroy() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void EnsureSoundPool() {
		if(SoundPool.Instance != null) return;
		if(m_soundPoolPrefab == null) {
			Debug.LogWarning("AudioManager needs a SoundPool prefab assigned to play SFX.");
			return;
		}

		Instantiate(m_soundPoolPrefab);
	}

	private void SetupClips() {
		m_musicDict = m_MusicList.ToDictionary(s => s.name, s => s);
		m_soundDict = m_SoundList.ToDictionary(s => s.name, s => s);
	}

	#region Sound Effects
	public EmmiterController Play(string name) {
		if(string.IsNullOrEmpty(name)) return null;
		if(!m_soundDict.ContainsKey(name)){
			Debug.LogError("Sound with name " + name + " not found!");
			return null;
		}
		if(SoundPool.Instance == null) {
			Debug.LogWarning("SoundPool is not available. Sound '" + name + "' could not be played.");
			return null;
		}
		var sound = m_soundDict[name];
		var emmiter = SoundPool.Instance.PoolSoundEmmiter();
		emmiter.SetupSound(sound, m_soundVolume * m_generalVolumen);
		emmiter.PlaySound();
		return emmiter;
	}

	public void Stop(EmmiterController emmiter) {
		if(emmiter == null) return;
		emmiter.StopSound();
	}

	public void StopAllSounds() {
		if(SoundPool.Instance == null) return;

		for(int i = SoundPool.Instance.m_ActiveObjects.Count - 1; i >= 0; i--) {
			GameObject activeObject = SoundPool.Instance.m_ActiveObjects[i];
			if(activeObject == null) continue;

			EmmiterController emmiter = activeObject.GetComponent<EmmiterController>();
			if(emmiter != null)
				emmiter.StopSound();
		}
	}

	public static bool TryPlay(string name) {
		if(Instance == null || string.IsNullOrEmpty(name)) return false;
		return Instance.Play(name) != null;
	}

	public static EmmiterController TryPlayEmitter(string name) {
		if(Instance == null || string.IsNullOrEmpty(name)) return null;
		return Instance.Play(name);
	}

	public static void TryStop(EmmiterController emmiter) {
		if(Instance == null || emmiter == null) return;
		Instance.Stop(emmiter);
	}
	#endregion
	

	#region Background Music
	public void PlayBGM(string name, float time = 0) {
		if(!m_musicDict.ContainsKey(name)){
			Debug.LogError("Music with name " + name + " not found!");
			return;
		}
		var music = m_musicDict[name];
		m_currentBackground = music;
		m_backgroundAudioSource.clip 	= music.clip;
		m_backgroundAudioSource.volume 	= music.volume * m_musicVolume * m_generalVolumen;
		m_backgroundAudioSource.pitch 	= music.pitch;
		m_backgroundAudioSource.loop 	= music.loop;
		m_backgroundAudioSource.time 	= time;
		m_backgroundAudioSource.Play();
	}

	public void UpdateBGMusic(string name, float time = 0) {
		if(m_currentBackground.name == name) return;
		m_backgroundAudioSource.Stop();
		PlayBGM(name, time);
	}

	public void UpdateBGMusicInTime(string name){
		if(m_currentBackground.name == name) return;
		float time = m_backgroundAudioSource.time;
		UpdateBGMusic(name, time);
	}

	public void StopBGM() {
		m_backgroundAudioSource.Stop();
	}
	
	public void ResumeBGM() {
		m_backgroundAudioSource.Play();
	}
	#endregion

	#region Volume
	public void UpdateGeneralVolume(float volume){
		if(volume < 0 || volume > 1){
			Debug.LogError("Volume " + volume + " its not in 0-1 boundaries");
			return;
		}

		m_generalVolumen = volume;

		if(m_currentBackground != null)
			m_backgroundAudioSource.volume = m_currentBackground.volume * m_musicVolume * m_generalVolumen;	
		if(SoundPool.Instance != null) {
			SoundPool.Instance.m_ActiveObjects.ForEach( r => {
				r.GetComponent<EmmiterController>().UpdateVolume(m_soundVolume * m_generalVolumen);
			});
		}
	}

	public void UpdateMusicVolume(float volume) {
		if(volume < 0 || volume > 1){
			Debug.LogError("Volume " + volume + " its not in 0-1 boundaries");
			return;
		}
		m_musicVolume = volume;
		if(m_currentBackground != null)
			m_backgroundAudioSource.volume = m_currentBackground.volume * m_musicVolume * m_generalVolumen;
	}

	public void UpdateSoundVolume(float volume) {
		if(volume < 0 || volume > 1){
			Debug.LogError("Volume " + volume + " its not in 0-1 boundaries");
			return;
		}

		m_soundVolume = volume;

		if(SoundPool.Instance != null) {
			SoundPool.Instance.m_ActiveObjects.ForEach( r => {
				r.GetComponent<EmmiterController>().UpdateVolume(m_soundVolume * m_generalVolumen);
			});
		}
	}

	public float GetGeneralVolume() { return m_generalVolumen; }
	public float GetMusicVolume() 	{ return m_musicVolume; }
	public float GetSoundVolume() 	{ return m_soundVolume; }
	#endregion
}
