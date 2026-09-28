public class PlayerStateMachine
{
    public PlayerState Current { get; private set; }

    public void ChangeState(PlayerState next)
    {
        if (next == null || next == Current) return;
        Current?.Exit();
        Current = next;
        Current.Enter();
    }

    public void ForceState(PlayerState next)
    {
        Current?.Exit();
        Current = next;
        Current?.Enter();
    }

    public void Tick(float deltaTime) => Current?.Tick(deltaTime);
}
