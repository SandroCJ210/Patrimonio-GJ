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
	private bool m_resumeBackgroundAfterStop;
	private float m_backgroundResumeTime;

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
		if (m_MusicList == null) m_MusicList = System.Array.Empty<Sound>();
		if (m_SoundList == null) m_SoundList = System.Array.Empty<Sound>();
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

	/// <summary>Schedules any clip from the shared pool at an absolute DSP time.</summary>
	public EmmiterController PlayScheduled(AudioClip clip, double dspStartTime,
		AudioBus bus = AudioBus.Music, float clipVolume = 1f, float pitch = 1f,
		bool loop = false, bool retainEmitterUntilStopped = false) {
		if (clip == null) {
			Debug.LogError("Cannot schedule a null audio clip.");
			return null;
		}
		if (clip.loadState != AudioDataLoadState.Loaded) {
			Debug.LogError($"Audio clip '{clip.name}' must be loaded before it can be scheduled.");
			return null;
		}
		if (double.IsNaN(dspStartTime) || double.IsInfinity(dspStartTime) || dspStartTime < AudioSettings.dspTime) {
			Debug.LogError("Scheduled DSP time must be finite and in the future.");
			return null;
		}
		if (float.IsNaN(clipVolume) || float.IsInfinity(clipVolume) ||
			float.IsNaN(pitch) || float.IsInfinity(pitch) ||
			clipVolume < 0f || clipVolume > 1f || pitch <= 0f) {
			Debug.LogError("Clip volume must be 0-1 and pitch must be positive.");
			return null;
		}
		if (SoundPool.Instance == null) {
			Debug.LogWarning("SoundPool is not available. The scheduled clip could not be played.");
			return null;
		}

		var emitter = SoundPool.Instance.PoolSoundEmmiter();
		if (emitter == null) return null;
		emitter.SetupClip(clip, clipVolume, GetBusVolume(bus), bus, pitch, loop,
			retainEmitterUntilStopped);
		try {
			emitter.PlayScheduled(dspStartTime);
			return emitter;
		}
		catch {
			emitter.StopSound();
			throw;
		}
	}

	public EmmiterController PlayScheduled(Sound sound, double dspStartTime, AudioBus bus = AudioBus.Music) {
		if (sound == null) {
			Debug.LogError("Cannot schedule a null Sound.");
			return null;
		}
		return PlayScheduled(sound.clip, dspStartTime, bus, sound.volume, sound.pitch, sound.loop);
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
		m_resumeBackgroundAfterStop = false;
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

	public void StopBGM()
	{
		if (m_backgroundAudioSource.clip == null) return;
		m_resumeBackgroundAfterStop = m_backgroundAudioSource.isPlaying;
		m_backgroundResumeTime = m_backgroundAudioSource.time;
		m_backgroundAudioSource.Stop();
	}
	
	public void ResumeBGM() {
		if (!m_resumeBackgroundAfterStop || m_currentBackground == null) return;
		m_backgroundAudioSource.time = m_backgroundResumeTime;
		m_backgroundAudioSource.Play();
		m_resumeBackgroundAfterStop = false;
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
		UpdateActiveEmitterVolumes(AudioBus.Sfx, m_soundVolume * m_generalVolumen);
		UpdateActiveEmitterVolumes(AudioBus.Music, m_musicVolume * m_generalVolumen);
	}

	public void UpdateMusicVolume(float volume) {
		if(volume < 0 || volume > 1){
			Debug.LogError("Volume " + volume + " its not in 0-1 boundaries");
			return;
		}
		m_musicVolume = volume;
		if(m_currentBackground != null)
			m_backgroundAudioSource.volume = m_currentBackground.volume * m_musicVolume * m_generalVolumen;
		UpdateActiveEmitterVolumes(AudioBus.Music, m_musicVolume * m_generalVolumen);
	}

	public void UpdateSoundVolume(float volume) {
		if(volume < 0 || volume > 1){
			Debug.LogError("Volume " + volume + " its not in 0-1 boundaries");
			return;
		}

		m_soundVolume = volume;

		UpdateActiveEmitterVolumes(AudioBus.Sfx, m_soundVolume * m_generalVolumen);
	}

	private float GetBusVolume(AudioBus bus) => bus == AudioBus.Music
		? m_musicVolume * m_generalVolumen
		: m_soundVolume * m_generalVolumen;

	private void UpdateActiveEmitterVolumes(AudioBus bus, float volume) {
		if(SoundPool.Instance == null) return;
		foreach(GameObject activeObject in SoundPool.Instance.m_ActiveObjects) {
			if(activeObject == null) continue;
			EmmiterController emitter = activeObject.GetComponent<EmmiterController>();
			if(emitter != null && emitter.IsMusic == (bus == AudioBus.Music))
				emitter.UpdateVolume(volume);
		}
	}

	public float GetGeneralVolume() { return m_generalVolumen; }
	public float GetMusicVolume() 	{ return m_musicVolume; }
	public float GetSoundVolume() 	{ return m_soundVolume; }
	#endregion
}
