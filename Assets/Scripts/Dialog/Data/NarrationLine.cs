using System;
using UnityEngine;

[Serializable]
public class NarrationLine
{
    public Speaker speaker;
    public AudioClip clip;
    [TextArea] public string text;
    public float postDelay = 0.4f;
    public float RevealDuration => clip != null
    ? clip.length * 0.9f
    : text.Length / 40f;

    public float Duration => clip != null
        ? clip.length
        : Mathf.Max(RevealDuration + 1f, text.Length / 15f);
}
