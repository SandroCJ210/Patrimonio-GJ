using UnityEngine;

public class Cooldown
{
    private readonly float duration;
    private float readyTime;

    public Cooldown(float duration)
    {
        this.duration = duration;
    }

    public bool IsReady => Time.time >= readyTime;

    public void Start()
    {
        readyTime = Time.time + duration;
    }
}