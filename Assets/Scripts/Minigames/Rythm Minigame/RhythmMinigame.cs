using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class RhythmMinigame : MonoBehaviour, IMinigame
{
    public event Action<MinigameResult> Finished;
    public event Action<ProgressBeat> Progressed;
    
    [Header("Song")]
    [SerializeField] private SongData songData;
    [SerializeField] private RhythmClock clock;

    [Header("Presentation")]
    [SerializeField] private NoteObject notePrefab;
    [SerializeField] private Transform noteFather;

    [Tooltip("Orden: Left, Center, Right.")]
    [SerializeField] private Transform[] spawnPoints = new Transform[3];

    [Tooltip("Orden: Left, Center, Right.")]
    [SerializeField] private Transform[] targetPoints = new Transform[3];

    [Header("Input")]
    [Tooltip("Acciones dedicadas al ritmo. Orden: Left, Center, Right.")]
    [SerializeField] private InputActionReference[] laneActions =
        new InputActionReference[3];

    [Header("Judgement — milliseconds")]
    [SerializeField] private float perfectMs = 30f;
    [SerializeField] private float incredibleMs = 50f;
    [SerializeField] private float goodMs = 80f;

    [Tooltip("Positivo compensa pulsaciones registradas tarde.")]
    [SerializeField] private float inputOffsetMs;

    [Header("Debug")]
    [SerializeField] private bool logRhythmDebug = true;

    [Header("Story")]
    [SerializeField] private DialogueSequence openingDialogue;
    [SerializeField] private BeatId completionBeat;

    private readonly Queue<(BeatLane lane, double time)> _inputs = new();
    private readonly Dictionary<ChartNote, NoteObject> _views = new();
    private readonly List<(ChartNote note, double error)> _results = new();

    private ChartNote[] _chart = Array.Empty<ChartNote>();
    private InputAction[] _actions = Array.Empty<InputAction>();

    private int _nextSpawn;
    private int _sessionVersion;
    private bool _playing;
    private bool _openingDialogueComplete;
    private Coroutine _openingDialogueRoutine;
    private double _endTime;
    private EmmiterController _backgroundEmitter;
    private EmmiterController _mainEmitter;
    private readonly Queue<bool> _recentAccuracy = new();
    private const int PerformanceWindow = 8;
    private const float PerformanceFadeSeconds = 2f;
    private float _mainPerformanceVolume = 1f;

    private double GoodWindow => goodMs / 1000.0;
    private double InputOffset => inputOffsetMs / 1000.0;

    private void Start()
    {
        _openingDialogueRoutine = StartCoroutine(PlayOpeningDialogueThenBeginGame());
    }

    private IEnumerator PlayOpeningDialogueThenBeginGame()
    {
        if (openingDialogue == null || openingDialogue.lines == null ||
            openingDialogue.lines.Length == 0)
        {
            Debug.LogError("Asigna un diálogo de apertura con al menos una línea.", this);
            _openingDialogueRoutine = null;
            yield break;
        }

        CoreManager core = CoreManager.I;
        if (core == null || core.Lines == null || core.DialogScreen == null)
        {
            Debug.LogError(
                "No se encontró el CoreManager, LinePlayer o DialogScreen para reproducir el diálogo de apertura.",
                this);
            _openingDialogueRoutine = null;
            yield break;
        }

        LinePlayer linePlayer = core.Lines;
        core.DialogScreen.SetActive(true);

        foreach (NarrationLine line in openingDialogue.lines)
        {
            if (line == null)
                continue;

            bool lineCompleted = false;
            linePlayer.Play(line, () => lineCompleted = true);
            yield return new WaitUntil(() => lineCompleted || !linePlayer.IsPlaying);

            if (!lineCompleted)
            {
                core.DialogScreen.SetActive(false);
                _openingDialogueRoutine = null;
                yield break;
            }
        }

        core.DialogScreen.SetActive(false);
        _openingDialogueComplete = true;
        _openingDialogueRoutine = null;
        BeginGame();
    }
    

    public void BeginGame()
    {
        if (!_openingDialogueComplete)
            return;

        if (!isActiveAndEnabled)
            return;

        StopSession();

        try
        {
            ValidateConfiguration();

            AudioManager audioManager = AudioManager.Instance;
            if (audioManager == null)
                throw new InvalidOperationException("No hay un AudioManager activo en la escena.");

            var converter = new TicktoSecondsConverter(songData);
            var data = new List<NoteData>(songData.notes);
            data.Sort((a, b) => a.tick.CompareTo(b.tick));

            _chart = new ChartNote[data.Count];

            var occupied = new HashSet<(int tick, BeatLane lane)>();
            double lastTarget = 0.0;

            for (int i = 0; i < data.Count; i++)
            {
                NoteData note = data[i];

                if (note.tick < 0 || (int)note.Lane < 0 ||
                    (int)note.Lane >= 3)
                {
                    throw new InvalidOperationException(
                        $"Nota {i}: tick o carril inválido.");
                }

                if (note.noteType != NoteType.Tap)
                {
                    throw new InvalidOperationException(
                        $"Tick {note.tick}: Hold todavía no está implementado.");
                }

                if (!occupied.Add((note.tick, note.Lane)))
                {
                    throw new InvalidOperationException(
                        $"Nota duplicada: tick {note.tick}, carril {note.Lane}.");
                }

                double target = converter.TickToSeconds(note.tick);

                if (!IsFinite(target) || target < 0.0 ||
                    target > songData.backgroundClip.length ||
                    target > songData.mainClip.length)
                {
                    throw new InvalidOperationException(
                        $"Tick {note.tick}: su tiempo queda fuera del audio.");
                }

                _chart[i] = new ChartNote(note, target);
                lastTarget = target;
            }
            
            double leadIn = Math.Max(0.5, songData.timeToPlayBeat);

            audioManager.StopBGM();
            double scheduledDspStart = AudioSettings.dspTime + leadIn;
            _backgroundEmitter = audioManager.PlayScheduled(
                songData.backgroundClip, scheduledDspStart, AudioBus.Music,
                songData.backgroundVolume, retainEmitterUntilStopped: true);
            if (_backgroundEmitter == null)
                throw new InvalidOperationException("No se pudo programar la pista de fondo.");

            _mainEmitter = audioManager.PlayScheduled(
                songData.mainClip, scheduledDspStart, AudioBus.Music,
                songData.mainVolume, retainEmitterUntilStopped: true);
            if (_mainEmitter == null)
                throw new InvalidOperationException("No se pudo programar la pista principal.");

            clock.Configure(scheduledDspStart);

            if (Math.Abs(songData.backgroundClip.length - songData.mainClip.length) > 0.05f)
                Debug.LogWarning("Los stems tienen duraciones distintas; revisa que comiencen y terminen alineados.", this);

            _endTime = Math.Max(
                Math.Max(songData.backgroundClip.length, songData.mainClip.length),
                lastTarget + GoodWindow + Math.Max(0.0, InputOffset));

            _playing = true;
            EnableInput();
        }
        catch (Exception exception)
        {
            StopSession();
            Debug.LogException(exception, this);
        }
    }

    private void Update()
    {
        if (!_playing || !clock.IsRunning)
            return;

        int version = _sessionVersion;

        clock.UpdateClock();
        double songTime = clock.SongTime;
        UpdateMainPerformanceVolume();

        _results.Clear();

        while (_inputs.Count > 0)
        {
            var input = _inputs.Dequeue();

            if (clock.TryGetSongAtRealTime(input.time, out double time))
            {
                if (logRhythmDebug)
                    Debug.Log($"Input {input.lane} en canción {time:F3}s.", this);

                TryHit(input.lane, time - InputOffset);
            }
            else if (logRhythmDebug)
            {
                Debug.LogWarning(
                    $"[Rhythm] Input {input.lane} recibido pero no se pudo convertir a tiempo de canción. " +
                    $"inputRealtime={input.time:F3}s, sampleRealtime={Time.realtimeSinceStartupAsDouble:F3}s.",
                    this);
            }
        }

        double judgementTime = songTime - InputOffset;

        foreach (ChartNote note in _chart)
        {
            if (note.State == NoteState.Resolved)
                continue;

            double error = judgementTime - note.TargetSongTime;

            if (error > GoodWindow)
                Resolve(note, NoteScore.Falla, error);
        }

        SpawnDueNotes(songTime);

        foreach (NoteObject view in _views.Values)
            view.UpdateNote(songTime);
        
        for (int i = 0; i < _results.Count; i++)
        {
            var result = _results[i];

            if (logRhythmDebug)
            {
                Debug.Log(
                    $" {result.note.Score} — {result.note.Data.Lane}, " +
                    $"tick {result.note.Data.tick}, error {result.error * 1000.0:+0.0;-0.0;0.0} ms.",
                    this);
            }
            
            
            if (version != _sessionVersion)
                return;
        }

        if (songTime > _endTime)
            Complete(MinigameOutcome.Reached);
    }

    private void SpawnDueNotes(double songTime)
    {
        while (_nextSpawn < _chart.Length)
        {
            ChartNote note = _chart[_nextSpawn];

            if (note.TargetSongTime >
                songTime + songData.timeToPlayBeat)
                break;

            _nextSpawn++;
            
            if (note.State == NoteState.Resolved)
                continue;

            int lane = (int)note.Data.Lane;

            NoteObject view = Instantiate(notePrefab, noteFather);

            view.Initialize(
                note,
                spawnPoints[lane].position,
                targetPoints[lane].position,
                songData.timeToPlayBeat,
                note.Data.Lane);

            note.ActivateNote();
            _views.Add(note, view);
        }
    }

    private void TryHit(BeatLane lane, double inputSongTime)
    {
        ChartNote candidate = null;
        double candidateError = 0.0;
        double closest = double.PositiveInfinity;

        foreach (ChartNote note in _chart)
        {
            if (note.State == NoteState.Resolved ||
                note.Data.Lane != lane)
                continue;

            double error = inputSongTime - note.TargetSongTime;
            double absoluteError = Math.Abs(error);

            if (absoluteError <= GoodWindow && absoluteError < closest)
            {
                candidate = note;
                candidateError = error;
                closest = absoluteError;
            }
        }

        if (candidate == null)
        {
            if (logRhythmDebug)
                Debug.Log($" {lane}: pulsación sin nota dentro de la ventana Ok.", this);

            return;
        }

        NoteScore score =
            closest <= perfectMs / 1000.0 ? NoteScore.PitriMitri :
            closest <= incredibleMs / 1000.0 ? NoteScore.Bacan :
            NoteScore.Ok;

        Resolve(candidate, score, candidateError);
    }

    private void Resolve(ChartNote note, NoteScore score, double error)
    {
        if (note.State == NoteState.Resolved)
            return;

        note.Resolve(score);
        _recentAccuracy.Enqueue(score != NoteScore.Falla);
        while (_recentAccuracy.Count > PerformanceWindow)
            _recentAccuracy.Dequeue();

        if (_views.TryGetValue(note, out NoteObject view))
        {
            _views.Remove(note);
            Destroy(view.gameObject);
        }

        _results.Add((note, error));
    }

    private void EnableInput()
    {
        _actions = new InputAction[3];

        for (int i = 0; i < _actions.Length; i++)
        {
            _actions[i] = laneActions[i].action;
            _actions[i].performed += OnLanePerformed;
            _actions[i].Enable();
        }
    }

    private void OnLanePerformed(InputAction.CallbackContext context)
    {
        if (!_playing || !clock.IsRunning)
            return;

        for (int i = 0; i < _actions.Length; i++)
        {
            if (_actions[i] != context.action)
                continue;
            
            _inputs.Enqueue(((BeatLane)i, context.time));
            return;
        }
    }

    public void Pause()
    {
        if (!_playing)
            return;

        clock.Pause();
        _inputs.Clear();
    }

    public void Resume()
    {
        if (!_playing)
            return;

        _inputs.Clear();
        clock.Resume();
    }

    public void Skip()
    {
        if (_playing)
            Complete(MinigameOutcome.Skipped);
    }

    private void Complete(MinigameOutcome outcome)
    {
        StopSession();

        if (outcome == MinigameOutcome.Reached && completionBeat != null)
            Progressed?.Invoke(new ProgressBeat(completionBeat));

        Finished?.Invoke(new MinigameResult(outcome));
    }

    private void StopSession()
    {
        _sessionVersion++;
        _playing = false;

        foreach (InputAction action in _actions)
        {
            if (action == null)
                continue;

            action.performed -= OnLanePerformed;
            action.Disable();
        }

        _actions = Array.Empty<InputAction>();

        if (clock != null)
            clock.Stop();

        foreach (NoteObject view in _views.Values)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        _views.Clear();
        _inputs.Clear();
        _results.Clear();
        _recentAccuracy.Clear();
        _mainPerformanceVolume = 1f;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Stop(_backgroundEmitter);
            AudioManager.Instance.Stop(_mainEmitter);
            AudioManager.Instance.ResumeBGM();
        }
        _backgroundEmitter = null;
        _mainEmitter = null;
        _chart = Array.Empty<ChartNote>();
        _nextSpawn = 0;
    }

    private void OnDisable()
    {
        if (_openingDialogueRoutine != null)
        {
            StopCoroutine(_openingDialogueRoutine);
            _openingDialogueRoutine = null;

            if (CoreManager.I != null)
            {
                CoreManager.I.Lines?.Stop();
                if (CoreManager.I.DialogScreen != null)
                    CoreManager.I.DialogScreen.SetActive(false);
            }
        }

        StopSession();
    }

    private void ValidateConfiguration()
    {
        if (songData == null || songData.backgroundClip == null ||
            songData.mainClip == null || clock == null || notePrefab == null)
        {
            throw new InvalidOperationException(
                "Asigna canción con ambos stems, reloj y prefab.");
        }

        if (songData.backgroundClip.loadState != AudioDataLoadState.Loaded ||
            songData.mainClip.loadState != AudioDataLoadState.Loaded)
            throw new InvalidOperationException("Carga ambos clips antes de comenzar el minijuego.");

        if (!IsFinite(songData.minimumMainPerformanceVolume) ||
            songData.minimumMainPerformanceVolume < 0.5f ||
            songData.minimumMainPerformanceVolume > 1f)
            throw new InvalidOperationException("El volumen mínimo del instrumento debe estar entre 0.5 y 1.");

        if (!clock.isActiveAndEnabled)
            throw new InvalidOperationException("Activa RhythmClock.");

        if (songData.notes == null || songData.notes.Count == 0)
            throw new InvalidOperationException("La canción no tiene notas.");

        if (!IsFinite(songData.timeToPlayBeat) ||
            songData.timeToPlayBeat <= 0f ||
            !IsFinite(songData.songOffset))
        {
            throw new InvalidOperationException(
                "Revisa timeToPlayBeat y songOffset.");
        }

        if (!IsFinite(perfectMs) || !IsFinite(incredibleMs) ||
            !IsFinite(goodMs) || !IsFinite(inputOffsetMs) ||
            perfectMs <= 0f || incredibleMs < perfectMs ||
            goodMs < incredibleMs)
        {
            throw new InvalidOperationException(
                "Ventanas requeridas: 0 < PitriMitri <= Bacan <= Ok.");
        }

        if (spawnPoints == null || spawnPoints.Length != 3 ||
            targetPoints == null || targetPoints.Length != 3 ||
            laneActions == null || laneActions.Length != 3)
        {
            throw new InvalidOperationException(
                "Configura tres entradas por arreglo: Left, Center, Right.");
        }

        var uniqueActions = new HashSet<InputAction>();

        for (int i = 0; i < 3; i++)
        {
            if (spawnPoints[i] == null || targetPoints[i] == null ||
                laneActions[i] == null || laneActions[i].action == null)
            {
                throw new InvalidOperationException(
                    $"Faltan referencias del carril {(BeatLane)i}.");
            }

            InputAction action = laneActions[i].action;

            if (action.type != InputActionType.Button ||
                !uniqueActions.Add(action))
            {
                throw new InvalidOperationException(
                    "Cada carril necesita una acción Button diferente.");
            }
        }
    }

    private static bool IsFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private void UpdateMainPerformanceVolume()
    {
        if (_mainEmitter == null) return;

        int correctNotes = 0;
        foreach (bool correct in _recentAccuracy)
            if (correct) correctNotes++;

        float accuracy = _recentAccuracy.Count == 0
            ? 1f
            : (float)correctNotes / _recentAccuracy.Count;
        float target = Mathf.Lerp(songData.minimumMainPerformanceVolume, 1f, accuracy);
        _mainPerformanceVolume = Mathf.MoveTowards(
            _mainPerformanceVolume, target,
            Time.unscaledDeltaTime / PerformanceFadeSeconds);
        _mainEmitter.SetPlaybackMultiplier(_mainPerformanceVolume);
    }
}
