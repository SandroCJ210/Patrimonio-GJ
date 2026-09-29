using System;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    private ChartNote _chartNote;

    [SerializeField] private Vector3 _spawnPosition;
    [SerializeField] private Vector3 _targetPosition;

    private double _approachTime;
    
    public ChartNote ChartNote => _chartNote;

    public void Initialize(
        ChartNote chartNote,
        Vector3 spawnPosition,
        Vector3 targetPosition,
        double approachTime
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
