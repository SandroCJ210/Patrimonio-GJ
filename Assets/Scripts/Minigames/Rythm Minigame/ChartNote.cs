using UnityEngine;

public enum NoteState
{
    Pending,
    Active,
    Resolved
}

public enum NoteScore
{
    Perfect,
    Incredible,
    Good,
    Miss,
    None
}
public sealed class ChartNote 
{
    public NoteData Data { get; }
    public double TargetSongTime { get; }
    
    public NoteState State { get; private set;  }
    public NoteScore Score { get; private set; }

    public ChartNote(NoteData data, double targetSongTime)
    {
        Data = data;
        TargetSongTime = targetSongTime;
        State = NoteState.Pending;
        Score = NoteScore.None;
    }

    public void ActivateNote()
    {
        if (State != NoteState.Pending)
        {
            return;
        }
        
        State = NoteState.Active;
    }

    public void Resolve(NoteScore score)
    {
        if (State == NoteState.Resolved)
        {
            return;
        }
        
        Score = score;
        State = NoteState.Resolved;
    }
    
}
