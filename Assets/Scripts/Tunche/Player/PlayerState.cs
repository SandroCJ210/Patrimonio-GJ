public abstract class PlayerState
{
    protected readonly PlayerStateController player;

    protected PlayerState(PlayerStateController player)
    {
        this.player = player;
    }

    public virtual void Enter() { }

    public virtual void Tick() { }

    public virtual void Exit() { }
}