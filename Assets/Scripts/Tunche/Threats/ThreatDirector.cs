using System.Collections;
using UnityEngine;

public class ThreatDirector : MonoBehaviour, IMinigame
{
    [SerializeField] PlayerStateController player;
    [SerializeField] WhistleThreat whistleThreat;
    [SerializeField] BoatApparition apparition;
    [SerializeField] LossScreenController lossScreen;
    [SerializeField] ThreatDialogueBridge dialogue;

    [SerializeField]
    ThreatType[] sequence =
    {
        ThreatType.Whistle,
        ThreatType.Illusion,
        ThreatType.TuncheReal,
        ThreatType.Whistle,
        ThreatType.TuncheReal,
        ThreatType.Whistle,
    };

    [Header("Pausa entre amenazas")]
    [SerializeField] float minPause = 3f;
    [SerializeField] float maxPause = 10f;

    [SerializeField] BeatId startBeat;
    [SerializeField] BeatId[] turnBeats;
    [SerializeField] BeatId wonBeat;

  
    public event System.Action<MinigameResult> Finished;
    public event System.Action<ProgressBeat> Progressed;

    bool waitingForResolution;
    bool lastSurvived;

    Coroutine sequenceRoutine;

    void Awake()
    {
        if (player == null || whistleThreat == null || apparition == null)
            Debug.LogError("ThreatDirector: faltan referencias obligatorias (Player / WhistleThreat / BoatApparition) en el Inspector.", this);
    }

    void OnEnable()
    {
        if (whistleThreat != null) whistleThreat.Resolved += HandleResolved;
        if (apparition != null) apparition.Resolved += HandleResolved;
    }

    void OnDisable()
    {
        if (whistleThreat != null) whistleThreat.Resolved -= HandleResolved;
        if (apparition != null) apparition.Resolved -= HandleResolved;

        if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
        sequenceRoutine = null;
        if (dialogue != null) dialogue.StopDialogue();
        if (whistleThreat != null) whistleThreat.ForceReset();
        if (apparition != null) apparition.ForceReset();
    }

    void Start() => BeginGame();

    void HandleResolved(bool survived)
    {
        lastSurvived = survived;
        waitingForResolution = false;
    }

    public void BeginGame()
    {
        if (player == null || whistleThreat == null || apparition == null) return;

        if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
        if (dialogue != null) dialogue.StopDialogue();
        whistleThreat.ForceReset();
        apparition.ForceReset();
        waitingForResolution = false;
        player.ResetForNewGame();
        sequenceRoutine = StartCoroutine(RunSequence());
    }

    public void Restart()
    {
        if (lossScreen != null) lossScreen.Hide();

        BeginGame();
    }

    IEnumerator RunSequence()
    {
        player.Freeze();
        if (dialogue != null)
            yield return dialogue.PlayIntroduction();
        player.ResetForNewGame();
        RaiseProgress(startBeat);

        for (int i = 0; i < sequence.Length; i++)
        {
            yield return new WaitUntil(() => player.IsIdle);
            yield return new WaitForSeconds(Random.Range(minPause, maxPause));
            yield return new WaitUntil(() => player.IsIdle);

            ThreatType current = sequence[i];
            waitingForResolution = true;

            if (current == ThreatType.Whistle)
                whistleThreat.Activate();
            else
                apparition.Activate(current == ThreatType.TuncheReal
                    ? BoatApparition.ApparitionKind.TuncheReal
                    : BoatApparition.ApparitionKind.Illusion);

            yield return new WaitUntil(() => !waitingForResolution);

            if (!lastSurvived)
            {
                if (dialogue != null) dialogue.StopDialogue();
                player.Freeze();
                if (lossScreen != null) lossScreen.Show();
                sequenceRoutine = null;
                yield break;
            }
            if (turnBeats != null && i < turnBeats.Length) RaiseProgress(turnBeats[i]);
        }

        RaiseProgress(wonBeat);
        player.Freeze();
        if (lossScreen != null) lossScreen.ShowWin();
        sequenceRoutine = null;
        Finished?.Invoke(new MinigameResult(MinigameOutcome.Reached));
    }

    void RaiseProgress(BeatId beat)
    {
        if (beat == null) return;
        Progressed?.Invoke(new ProgressBeat(beat));
    }
}
