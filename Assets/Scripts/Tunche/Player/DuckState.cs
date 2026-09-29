using System.Collections;
using UnityEngine;

public class DuckState : PlayerState
{
    Coroutine routine;

    public DuckState(PlayerStateController controller, PlayerStateMachine machine) : base(controller, machine) { }

    public override void Enter()
    {
        Controller.BeginDuckSequence();
        routine = Controller.StartCoroutine(Run());
    }

    public override void Exit()
    {
        if (routine != null) Controller.StopCoroutine(routine);
        routine = null;
    }

    IEnumerator Run()
    {
        var presentation = Controller.DuckPresentation;
        float entry = Controller.DuckEntryDuration;
        float action = Controller.DuckActionDuration;
        float exit = Controller.DuckExitDuration;

        if (presentation != null) presentation.FadeIn(entry);
        yield return new WaitForSeconds(entry);
        if (presentation != null) presentation.RecenterCamera();

        float exitStart = Mathf.Max(entry, action - exit);
        if (exitStart > entry)
            yield return new WaitForSeconds(exitStart - entry);

        if (presentation != null) presentation.FadeOut(exit);
        float remaining = action - exitStart;
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        Controller.EndDuckSequence();
        Machine.ChangeState(Controller.Idle);
    }
}
