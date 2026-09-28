using System;
using System.Collections;
using UnityEngine;


public class ThreatDialogueBridge : MonoBehaviour
{
    [Serializable]
    public class BeatDialogue
    {
        public BeatId beat;
        public DialogueSequence sequence;
    }

    [SerializeField] MonoBehaviour minigameSource; 
    [SerializeField] LinePlayer linePlayer;       
    [Tooltip("Se reproduce antes de habilitar al jugador y las amenazas.")]
    [SerializeField] DialogueSequence introduction;
    [SerializeField] BeatDialogue[] beatDialogues;
    [SerializeField] DialogueSequence victorySequence;

    IMinigame minigame;
    Coroutine sequenceRoutine;
    readonly System.Collections.Generic.Queue<DialogueSequence> pending = new();

    public IEnumerator PlayIntroduction()
    {
        StopDialogue();
        if (linePlayer == null && CoreManager.I != null) linePlayer = CoreManager.I.Lines;
        if (linePlayer == null || introduction == null) yield break;
        PlaySequence(introduction);
        yield return new WaitUntil(() => sequenceRoutine == null || !isActiveAndEnabled);
    }

    public void StopDialogue()
    {
        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            if (linePlayer != null) linePlayer.Stop();
        }
        sequenceRoutine = null;
        pending.Clear();
    }

    void Awake()
    {
        minigame = minigameSource as IMinigame;
        if (linePlayer == null && CoreManager.I != null)
            linePlayer = CoreManager.I.Lines;
    }

    void OnEnable()
    {
        if (minigame == null) return;
        minigame.Progressed += OnProgressed;
        minigame.Finished += OnFinished;
    }

    void OnDisable()
    {
        StopDialogue();
        if (minigame == null) return;
        minigame.Progressed -= OnProgressed;
        minigame.Finished -= OnFinished;
    }

    void OnProgressed(ProgressBeat progress)
    {
        var sequence = FindSequence(progress.Id);
        if (sequence != null) PlaySequence(sequence);
    }

    void OnFinished(MinigameResult result)
    {
        if (result.FinishedMinigameState == MinigameOutcome.Reached && victorySequence != null)
            PlaySequence(victorySequence);
    }

    DialogueSequence FindSequence(BeatId id)
    {
        if (id == null || beatDialogues == null) return null;
        foreach (var entry in beatDialogues)
            if (entry != null && entry.beat == id) return entry.sequence;
        return null;
    }

    void PlaySequence(DialogueSequence sequence)
    {
        if (linePlayer == null || sequence == null || sequence.lines == null) return;
        pending.Enqueue(sequence);
        if (sequenceRoutine == null) sequenceRoutine = StartCoroutine(PlayPending());
    }

    IEnumerator PlayPending()
    {
        yield return null;
        while (pending.Count > 0)
        {
            var sequence = pending.Dequeue();
            foreach (var line in sequence.lines)
            {
                if (line == null) continue;
                bool done = false;
                linePlayer.Play(line, () => done = true);
                yield return new WaitUntil(() => done || !linePlayer.IsPlaying);
            }
        }
        sequenceRoutine = null;
    }
}
