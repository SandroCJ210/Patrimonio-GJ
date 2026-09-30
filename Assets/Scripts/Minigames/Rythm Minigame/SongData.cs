using System;
using UnityEngine;
using System.Collections.Generic;
public enum BeatLane
{
    Left,
    Center,
    Right
}

public enum DifficultyConfig
{
    Easy,
    Medium,
    Hard
}

public enum NoteType
{
    Tap,
    Hold
}

[Serializable]
public struct NoteData
{
    public int tick;
    public BeatLane Lane;
    public NoteType noteType;
    public int durationTicks; //only works if holded
}

[CreateAssetMenu]
public class SongData : ScriptableObject
{
    public AudioClip backgroundClip;
    public AudioClip mainClip;

    [Range(0f, 1f)] public float backgroundVolume = 1f;
    [Range(0f, 1f)] public float mainVolume = 1f;
    [Range(0, 1f)] public float minimumMainPerformanceVolume = 0.5f;

    public int bpm = 90;
    public float songOffset;
    public int beatsPerMeasure = 4;
    public int ticksPerBeat = 4;
    
    // This variable indicates the time between the ChartNote appears and the ChartNote should be played
    public float timeToPlayBeat;
    public List<NoteData> notes;
    
    public DifficultyConfig difficulty;
    
    

}
