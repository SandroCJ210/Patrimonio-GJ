using UnityEngine;

public class PlayButton : MonoBehaviour
{
    [SerializeField] GameObject menuScreen;
    [SerializeField] GameObject mapScreen;
    [SerializeField] GameObject dialogScreen;
    [SerializeField] LinePlayer player;
    [SerializeField] DialogueSequence sequence;

    void Start()
    {
        dialogScreen = CoreManager.I.DialogScreen;
        player = CoreManager.I.Lines;

        if (dialogScreen.transform.childCount > 0)
        {
            dialogScreen.transform.GetChild(0).gameObject.SetActive(false);
            dialogScreen.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

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