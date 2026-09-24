using UnityEngine;

public class CrossState : PlayerState
{
    private readonly float duration;
    private readonly Cooldown cooldown;

    private float endTime;

    public CrossState(
        PlayerStateController player,
        float duration,
        Cooldown cooldown
    ) : base(player)
    {
        this.duration = duration;
        this.cooldown = cooldown;
    }

    public override void Enter()
    {
        endTime = Time.time + duration;

        player.Hands.ShowCross();

        player.NotifyCrossUsed();
    }

    public override void Tick()
    {
        if (Time.time >= endTime)
        {
            player.Machine.ChangeTo(player.Idle);
        }
    }

    public override void Exit()
    {
        player.Hands.HideAll();

        cooldown.Start();
    }
}