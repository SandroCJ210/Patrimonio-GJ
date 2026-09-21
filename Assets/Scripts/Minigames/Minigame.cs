using System;

public interface IMinigame {
    event Action<MinigameResult> Finished;
    event Action<ProgressBeat> Progressed;
}