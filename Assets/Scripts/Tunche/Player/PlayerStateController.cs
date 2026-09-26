using System;
using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    [SerializeField] private PlayerHands hands;

    [Header("Agacharse")]
    [SerializeField] private float duckDuration = 2f;
    [SerializeField] private float duckCooldownTime = 4f;

    [Header("Cruz")]
    [SerializeField] private float crossDuration = 1.2f;
    [SerializeField] private float crossCooldownTime = 6f;

    // Maquina de estados
    public PlayerStateMachine Machine { get; private set; }

    // Representación visual
    public PlayerHands Hands => hands;

    // Estados del jugador
    public IdleState Idle { get; private set; }
    public DuckState Duck { get; private set; }
    public CrossState Cross { get; private set; }

    // Temporizadores
    public Cooldown DuckCooldown { get; private set; }
    public Cooldown CrossCooldown { get; private set; }

    // Control de acciones
    public bool InputEnabled { get; set; } = true;
    public bool DuckAllowed { get; set; } = true;

    // Estado actual
    public bool IsIdle => Machine.Current == Idle;
    public bool IsDucking => Machine.Current == Duck;

    // Bloqueo de camara
    public bool CameraLocked => !IsIdle;

    // Disponibilidad de las acciones
    public bool CanDuck => DuckAllowed && DuckCooldown.IsReady;
    public bool CrossReady => CrossCooldown.IsReady;

    // Eventos para comunicar las acciones
    public event Action DuckStarted;
    public event Action CrossUsed;

    void Awake()
    {
        // Crear temporizadores
        DuckCooldown = new Cooldown(duckCooldownTime);
        CrossCooldown = new Cooldown(crossCooldownTime);

        // Crear estados
        Idle = new IdleState(this);
        Duck = new DuckState(this, duckDuration, DuckCooldown);
        Cross = new CrossState(this, crossDuration, CrossCooldown);

        // Crear maquina de estados
        Machine = new PlayerStateMachine();
        /*  
         Prueba de estados funcionales 
        Machine.StateChanged += state =>
        {
            Debug.Log($"Estado actual: {state.GetType().Name}");
        };
        */
        // Establecer el estado inicial
        Machine.ChangeTo(Idle);
    }

    void Update()
    {
        Machine.Tick();
    }

    public void NotifyDuckStarted()
    {
        DuckStarted?.Invoke();
    }

    public void NotifyCrossUsed()
    {
        CrossUsed?.Invoke();
    }
}