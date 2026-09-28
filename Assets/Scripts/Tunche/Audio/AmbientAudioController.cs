using System;
using System.Collections;
using UnityEngine;


public class AmbientAudioController : MonoBehaviour
{
    [Serializable]
    public class DuckableSource
    {
        public AudioSource source;
        public AudioLowPassFilter lowPass; // opcional

        [NonSerialized] public float normalVolume;
        [NonSerialized] public float normalCutoff = 22000f;
    }

    [SerializeField] PlayerStateController player;

    [Header("Fuentes que se amortiguan al agacharse (arrastra las que quieras)")]
    [SerializeField] DuckableSource[] sources;

    [Header("Amortiguación durante el agachado")]
    [SerializeField, Range(0f, 1f)] float duckedVolume01 = 0.12f;
    [SerializeField] float duckedCutoffHz = 700f;
    [SerializeField] float duckTransitionDuration = 0.4f;
    [SerializeField] float restoreTransitionDuration = 0.35f;

    Coroutine transitionRoutine;

    void Awake()
    {
        if (sources == null) return;
        foreach (var s in sources)
        {
            if (s.source != null) s.normalVolume = s.source.volume;
            if (s.lowPass != null) s.normalCutoff = s.lowPass.cutoffFrequency;
        }
    }

    void OnEnable()
    {
        if (player != null)
        {
            player.DuckStarted += HandleDuckStarted;
            player.DuckEnded += HandleDuckEnded;
        }
    }

    void OnDisable()
    {
        if (player != null)
        {
            player.DuckStarted -= HandleDuckStarted;
            player.DuckEnded -= HandleDuckEnded;
        }
    }

    void HandleDuckStarted() => StartTransition(ducked: true, duckTransitionDuration);
    void HandleDuckEnded() => StartTransition(ducked: false, restoreTransitionDuration);

    void StartTransition(bool ducked, float duration)
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(Transition(ducked, duration));
    }

    IEnumerator Transition(bool ducked, float duration)
    {
        if (sources == null || sources.Length == 0) yield break;

        var startVolumes = new float[sources.Length];
        var startCutoffs = new float[sources.Length];
        for (int i = 0; i < sources.Length; i++)
        {
            startVolumes[i] = sources[i].source != null ? sources[i].source.volume : 0f;
            startCutoffs[i] = sources[i].lowPass != null ? sources[i].lowPass.cutoffFrequency : 22000f;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);
            ApplyStep(ducked, p, startVolumes, startCutoffs);
            yield return null;
        }

        ApplyStep(ducked, 1f, startVolumes, startCutoffs);
        transitionRoutine = null;
    }

    void ApplyStep(bool ducked, float p, float[] startVolumes, float[] startCutoffs)
    {
        for (int i = 0; i < sources.Length; i++)
        {
            var s = sources[i];
            float targetVolume = ducked ? s.normalVolume * duckedVolume01 : s.normalVolume;
            float targetCutoff = ducked ? duckedCutoffHz : s.normalCutoff;

            if (s.source != null) s.source.volume = Mathf.Lerp(startVolumes[i], targetVolume, p);
            if (s.lowPass != null) s.lowPass.cutoffFrequency = Mathf.Lerp(startCutoffs[i], targetCutoff, p);
        }
    }

    public void ResetToNormal()
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = null;
        if (sources == null) return;
        foreach (var s in sources)
        {
            if (s.source != null) s.source.volume = s.normalVolume;
            if (s.lowPass != null) s.lowPass.cutoffFrequency = s.normalCutoff;
        }
    }
}
