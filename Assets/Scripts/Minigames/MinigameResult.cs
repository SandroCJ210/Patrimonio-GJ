public enum MinigameOutcome{
    Skipped,
    Reached,
    Failed
}

public readonly struct MinigameResult{
    public readonly MinigameOutcome FinishedMinigameState;
    public MinigameResult(MinigameOutcome minigameOutcome){
        FinishedMinigameState = minigameOutcome;
    }
}
