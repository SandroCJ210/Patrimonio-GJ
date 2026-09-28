using UnityEngine;

public class PlayButton : MonoBehaviour
{
    [SerializeField] GameObject menuScreen;
    [SerializeField] GameObject mapScreen;
    [SerializeField] GameObject dialogScreen;
    [SerializeField] LinePlayer player;
    [SerializeField] DialogueSequence sequence;
    int index = -1;

    void Play(DialogueSequence seq)
    {
        sequence = seq;
        index = -1;
        Next();
    }

    void Next()
    {
        index++;
        if (index >= sequence.lines.Length)
        {
            Finish();
            return;
        }
        player.Play(sequence.lines[index], Next);
    }

    void Finish()
    {
        dialogScreen.SetActive(false);
        mapScreen.SetActive(true);
    }

    public void Play()
    {
        menuScreen.SetActive(false);
        dialogScreen.SetActive(true);
        Play(sequence);
    }

}