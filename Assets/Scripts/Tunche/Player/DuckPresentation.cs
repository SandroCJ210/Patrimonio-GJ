using System;
using System.Collections;
using UnityEngine;

public class DuckPresentation : MonoBehaviour
{
    [SerializeField] CanvasGroup duckView;
    [SerializeField] CameraMovement cameraMovement;

    Coroutine fadeRoutine;

    void Awake() { SnapHidden(); }

    public void FadeIn(float duration, Action onComplete = null) => StartFade(1f, duration, onComplete);
    public void FadeOut(float duration, Action onComplete = null) => StartFade(0f, duration, onComplete);

    public void RecenterCamera()
    {
        if (cameraMovement != null) cameraMovement.RecenterToFront();
    }

    public void SnapHidden()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = null;
        if (duckView != null) duckView.alpha = 0f;
    }

    void StartFade(float target, float duration, Action onComplete)
    {
        if (duckView == null) { onComplete?.Invoke(); return; }
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Fade(target, duration, onComplete));
    }

    IEnumerator Fade(float target, float duration, Action onComplete)
    {
        float start = duckView != null ? duckView.alpha : target;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);
            if (duckView != null) duckView.alpha = Mathf.Lerp(start, target, p);
            yield return null;
        }

        if (duckView != null) duckView.alpha = target;
        fadeRoutine = null;
        onComplete?.Invoke();
    }
}
