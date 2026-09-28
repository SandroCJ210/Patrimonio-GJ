using UnityEngine;

public class Cooldown
{
    readonly float duration;
    float remaining;

    public Cooldown(float duration)
    {
        this.duration = duration;
    }

    public bool IsReady => remaining <= 0f;
    public float Remaining => Mathf.Max(0f, remaining);
    public float Progress01 => duration <= 0f ? 1f : 1f - Mathf.Clamp01(remaining / duration);

    public void Start() => remaining = duration;

    public void Tick(float deltaTime)
    {
        if (remaining > 0f) remaining -= deltaTime;
    }

    public void Reset() => remaining = 0f;
}
