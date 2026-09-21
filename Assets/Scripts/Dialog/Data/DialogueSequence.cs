using UnityEngine;

[CreateAssetMenu(fileName = "DialogueSequence", menuName = "Story/DialogueSequence")]
public class DialogueSequence : ScriptableObject
{
    public NarrationLine[] lines;
}
