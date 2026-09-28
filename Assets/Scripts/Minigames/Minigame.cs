using System;

public interface IMinigame {
    public void BeginMinigame();
    event Action<MinigameResult> Finished;
    event Action<ProgressBeat> Progressed;
}