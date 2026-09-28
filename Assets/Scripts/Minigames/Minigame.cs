using System;

public interface IMinigame {
    public void BeginGame();
    event Action<MinigameResult> Finished;
    event Action<ProgressBeat> Progressed;
}