using System;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    private ChartNote _chartNote;

    [SerializeField] private Vector3 _spawnPosition;
    [SerializeField] private Vector3 _targetPosition;
    [SerializeField] private Sprite[] _noteSprites;

    private double _approachTime;
    private Sprite _currentSprite;
    
    public ChartNote ChartNote => _chartNote;

    public void Initialize(
        ChartNote chartNote,
        Vector3 spawnPosition,
        Vector3 targetPosition,
        double approachTime,
        BeatLane beatLane
    )
    {
        if (approachTime < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(approachTime), "The approach time must be greater than zero");
        }
        _chartNote = chartNote;
        _spawnPosition = spawnPosition;
        _targetPosition = targetPosition;
        _approachTime = approachTime;
        
        transform.position = _spawnPosition;
        
        switch (beatLane)
        {
            case BeatLane.Left:
                _currentSprite = _noteSprites[0];
                break;
            case BeatLane.Center:
                _currentSprite = _noteSprites[1];
                break;
            case BeatLane.Right:
                _currentSprite = _noteSprites[2];
                break;
        }
        
        GetComponent<SpriteRenderer>().sprite = _currentSprite;
    }

    public void UpdateNote(double songTime)
    {
        if (_chartNote == null)
        {
            return;
        }
        
        double remainingTime = _chartNote.TargetSongTime - songTime;
        
        float progress = 1f - (float)(remainingTime / _approachTime);
        
        transform.position = Vector3.Lerp(_spawnPosition, _targetPosition, progress);
        
    }
}
