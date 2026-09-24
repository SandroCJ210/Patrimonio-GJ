using System;
public class PlayerStateMachine
{
    public PlayerState Current { get; private set; }

    public event Action<PlayerState> StateChanged;

    public void ChangeTo(PlayerState next)
    {
        if (next == null || next == Current)
            return;

        Current?.Exit();

        Current = next;

        Current.Enter();

        StateChanged?.Invoke(Current);
    }

    public void Tick()
    {
        Current?.Tick();
    }
}