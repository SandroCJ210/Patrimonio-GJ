public abstract class PlayerState
{
    protected readonly PlayerStateController Controller;
    protected readonly PlayerStateMachine Machine;

    protected PlayerState(PlayerStateController controller, PlayerStateMachine machine)
    {
        Controller = controller;
        Machine = machine;
    }

    public virtual void Enter() { }
    public virtual void Tick(float deltaTime) { }
    public virtual void Exit() { }
}
