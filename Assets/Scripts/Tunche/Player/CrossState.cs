using System.Collections;
using UnityEngine;

public class CrossState : PlayerState
{
    Coroutine routine;

    public CrossState(PlayerStateController controller, PlayerStateMachine machine) : base(controller, machine) { }

    public override void Enter()
    {
        Controller.SetControlLocks(cameraLocked: true, inputEnabled: false);
        routine = Controller.StartCoroutine(Run());
    }

    public override void Exit()
    {
        if (routine != null) Controller.StopCoroutine(routine);
        routine = null;
    }

    IEnumerator Run()
    {
        var hands = Controller.Hands;
        hands.ShowCross();

        yield return new WaitForSeconds(Controller.CrossRaiseDuration);
        Controller.NotifyCrossUsed();

        yield return new WaitForSeconds(Controller.CrossHoldDuration);

        hands.HideCross();

        yield return new WaitForSeconds(Controller.CrossLowerDuration);
        Controller.CrossCooldownStart();

       
        Controller.SetControlLocks(
            cameraLocked: false,
            inputEnabled: true
        );

        Machine.ChangeState(Controller.Idle);
       
    }
}
