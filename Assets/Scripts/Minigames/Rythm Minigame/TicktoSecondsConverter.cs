using System;
using UnityEngine;

public sealed class TicktoSecondsConverter 
{
    public int Bpm { get; private set; }
    public float SongOffset { get; private set; }
    public int TicksPerBeat { get; private set; }

    public float SecondsPerBeat => (float) 60.0 / Bpm;
    public float SecondsPerTick => SecondsPerBeat / TicksPerBeat;

    public TicktoSecondsConverter(SongData song) : this(song.bpm, song.songOffset, song.ticksPerBeat)
    {
        
    }

    public TicktoSecondsConverter(int bpm, float songOffset, int ticksPerBeat)
    {
        if (bpm <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bpm), "Los bpm deben de ser un numero valido");
        }

        if (ticksPerBeat <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ticksPerBeat), "Los tickperbeat deben de ser un numero valido");
        }
        
        Bpm = bpm;
        SongOffset = songOffset;
        TicksPerBeat = ticksPerBeat;
    }

    public float TickToSeconds(int tick)
    {
        return SongOffset + tick * SecondsPerTick;
    }

    public double SecondsToTick(float seconds)
    {
        return (seconds - SongOffset) / SecondsPerTick;
    }

    public int SecondsToNearestTick(float seconds)
    {
        double tickPos = SecondsToTick(seconds);
        
        return (int)Math.Round(tickPos, MidpointRounding.AwayFromZero);
    }

    public float TickIntervalToSeconds(float duration)
    {
        if (duration <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "La duración debe de ser mayor que 0");
        }
        return duration * SecondsPerTick;
    }
    
    
    
}
