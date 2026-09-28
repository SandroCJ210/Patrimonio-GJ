using System;
using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] PlayerHands hands;
    [SerializeField] DuckPresentation duckPresentation;

    [Header("Agachado (Duck)")]
    [SerializeField] float duckEntryDuration = 0.25f;
    [SerializeField] float duckActionDuration = 2f;
    [SerializeField] float duckExitDuration = 0.25f;
    [SerializeField] float duckCooldownDuration = 4f;

    [Header("Cruz (Cross)")]
    [SerializeField] float crossRaiseDuration = 0.25f;
    [SerializeField] float crossHoldDuration = 0.7f;
    [SerializeField] float crossLowerDuration = 0.25f;
    [SerializeField] float crossCooldownDuration = 6f;

    public PlayerHands Hands => hands;
    public DuckPresentation DuckPresentation => duckPresentation;

    public float DuckEntryDuration => duckEntryDuration;
    public float DuckActionDuration => duckActionDuration;
    public float DuckExitDuration => duckExitDuration;

    public float CrossRaiseDuration => crossRaiseDuration;
    public float CrossHoldDuration => crossHoldDuration;
    public float CrossLowerDuration => crossLowerDuration;

    public bool CameraLocked { get; private set; }
    public bool InputEnabled { get; private set; } = true;

    public event Action DuckStarted;
    public event Action DuckEnded;
    public event Action CrossUsed;

    PlayerStateMachine machine;
    Cooldown duckCooldown;
    Cooldown crossCooldown;
    bool duckBlocked;
    bool externalInputLock;

    public IdleState Idle { get; private set; }
    public DuckState Duck { get; private set; }
    public CrossState Cross { get; private set; }

    public bool IsIdle => machine != null && machine.Current == Idle;
    public bool IsDucking => machine != null && machine.Current == Duck;
    public bool CanDuck => IsIdle && !duckBlocked && InputEnabled && duckCooldown.IsReady;
    public bool CanCross => IsIdle && InputEnabled && crossCooldown.IsReady;

    void Awake()
    {
        machine = new PlayerStateMachine();
        duckCooldown = new Cooldown(duckCooldownDuration);
        crossCooldown = new Cooldown(crossCooldownDuration);

        Idle = new IdleState(this, machine);
        Duck = new DuckState(this, machine);
        Cross = new CrossState(this, machine);
    }

    void Start()
    {
        machine.ForceState(Idle);
        if (hands != null) hands.HideCross();
    }

    void Update()
    {
        duckCooldown.Tick(Time.deltaTime);
        crossCooldown.Tick(Time.deltaTime);
        machine.Tick(Time.deltaTime);
    }

    public void SetControlLocks(bool cameraLocked, bool inputEnabled)
    {
        CameraLocked = cameraLocked || externalInputLock;
        InputEnabled = inputEnabled && !externalInputLock;
    }

 
    public void SetDuckBlocked(bool blocked) => duckBlocked = blocked;

    public void Freeze()
    {
        externalInputLock = true;
        StopAllCoroutines();
        if (duckPresentation != null) duckPresentation.SnapHidden();
        if (hands != null) hands.HideCross();
        SetControlLocks(cameraLocked: true, inputEnabled: false);
        machine.ForceState(Idle);
    }

    public void BeginDuckSequence()
    {
        SetControlLocks(cameraLocked: true, inputEnabled: false);
        DuckStarted?.Invoke();
    }

    public void EndDuckSequence()
    {
        SetControlLocks(cameraLocked: false, inputEnabled: true);
        duckCooldown.Start();
        DuckEnded?.Invoke();
    }

    public void NotifyCrossUsed() => CrossUsed?.Invoke();

    public void CrossCooldownStart() => crossCooldown.Start();

   
    public void ResetForNewGame()
    {
        StopAllCoroutines();
        duckCooldown.Reset();
        crossCooldown.Reset();
        duckBlocked = false;
        externalInputLock = false;
        SetControlLocks(cameraLocked: false, inputEnabled: true);
        if (duckPresentation != null) duckPresentation.SnapHidden();
        if (hands != null) hands.HideCross();
        machine.ForceState(Idle);
        DuckEnded?.Invoke();
    }
}
