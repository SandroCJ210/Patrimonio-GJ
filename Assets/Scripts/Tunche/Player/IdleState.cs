
public class IdleState : PlayerState
{
    public IdleState(PlayerStateController player) : base(player)
    {
    }

    public override void Tick()
    {
        if (!player.InputEnabled)
            return;

        if (PlayerInput.DuckPressed && player.CanDuck)
        {
            player.Machine.ChangeTo(player.Duck);
        }
        else if (PlayerInput.CrossPressed && player.CrossReady)
        {
            player.Machine.ChangeTo(player.Cross);
        }
    }
}