using UnityEngine;

[CreateAssetMenu(menuName = "Story/Beat")]
public class BeatId : ScriptableObject { }

public readonly struct ProgressBeat {
    public readonly BeatId Id;
    public ProgressBeat(BeatId id) { Id = id; }
}