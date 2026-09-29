using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WhistleAudioController : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip whistleClip;
    [SerializeField] float fadeOutDuration = 0.6f;

    Coroutine fadeRoutine;

    void Awake() { if (source == null) source = GetComponent<AudioSource>(); }

    void OnDisable() { if (source != null) source.Stop(); }

    public void PlayDirectional(float pan)
    {
        if (source == null || whistleClip == null)
        {
            Debug.LogWarning("Asigna el clip del silbido.", this);
            return;
        }

        StopFade();
        source.clip = whistleClip;
        source.panStereo = Mathf.Clamp(pan, -1f, 1f);
        source.spatialBlend = 0f; 
        source.volume = 1f;
        source.loop = true;
        source.Play();
    }

    public void Stop(bool fade)
    {
        StopFade();
        if (source == null) return;

        if (!fade || !source.isPlaying)
        {
            source.Stop();
            return;
        }

        fadeRoutine = StartCoroutine(FadeOut());
    }

    void StopFade()
    {
        if (fadeRoutine == null) return;
        StopCoroutine(fadeRoutine);
        fadeRoutine = null;
    }

    IEnumerator FadeOut()
    {
        float startVolume = source.volume;
        float t = 0f;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, fadeOutDuration <= 0f ? 1f : t / fadeOutDuration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
        fadeRoutine = null;
    }
}
