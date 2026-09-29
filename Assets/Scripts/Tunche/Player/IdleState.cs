public class IdleState : PlayerState
{
    public IdleState(PlayerStateController controller, PlayerStateMachine machine) : base(controller, machine) { }

    public override void Enter()
    {
        if (Controller.InputEnabled) Controller.SetControlLocks(cameraLocked: false, inputEnabled: true);
    }

    public override void Tick(float deltaTime)
    {
        if (!Controller.InputEnabled) return;
        if (PlayerInput.DuckPressed && Controller.CanDuck)
        {
            Machine.ChangeState(Controller.Duck);
            return;
        }

        if (PlayerInput.CrossPressed && Controller.CanCross)
        {
            Machine.ChangeState(Controller.Cross);
        }
    }
}
