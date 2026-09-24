using System;
using System.Collections;
using UnityEngine;

public class LinePlayer : MonoBehaviour
{
    [SerializeField] AudioSource voice;

    public event Action<NarrationLine> LineStarted;
    public event Action LineEnded;

    public bool IsPlaying => routine != null;
    public float RevealProgress { get; private set; }

    Coroutine routine;
    bool textVisible;

    public void Play(NarrationLine line, Action onComplete)
    {
        Stop();
        routine = StartCoroutine(Run(line, onComplete));
    }

    public void Stop()
    {
        if (routine == null) return;
        StopCoroutine(routine);
        routine = null;
        voice.Stop();
        EndLine();
    }

    public void RevealAll() => RevealProgress = 1f;

    void OnDisable() => Stop();

    IEnumerator Run(NarrationLine line, Action onComplete)
    {
        RevealProgress = 0f;

        if (line.clip != null)
        {
            voice.clip = line.clip;
            voice.Play();
        }

        textVisible = true;
        LineStarted?.Invoke(line);

        float t = 0f;
        while (t < line.Duration)
        {
            t += Time.deltaTime;
            if (RevealProgress < 1f)
                RevealProgress = Mathf.Clamp01(t / line.RevealDuration);
            yield return null;
        }

        EndLine();
        yield return new WaitForSeconds(line.postDelay);

        routine = null;
        onComplete?.Invoke();
    }

    void EndLine()
    {
        if (!textVisible) return;
        textVisible = false;
        LineEnded?.Invoke();
    }
}