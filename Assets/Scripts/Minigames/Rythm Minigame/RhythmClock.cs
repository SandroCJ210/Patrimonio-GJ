using System;
using Unity.VisualScripting;
using UnityEngine;

public enum ClockState
{
    Stopped,
    Running,
    Paused
}
public class RhythmClock : MonoBehaviour
{
    private double _scheduleDpsStart;

    private double _songTime;
    private double _sampleRealTime;

    private double _acceptInputsSinceRealtime;

    private double _bufferDuration;
    private bool _ownsListenerPause;

    private const double CorrectionRate = 0.02;
    
    public ClockState State { get; private set; } = ClockState.Stopped;
    
    public double SongTime => _songTime;
    public double ScheduleDpsStart => _scheduleDpsStart;
    
    public bool IsRunning => State == ClockState.Running;

    public void Configure(double scheduledDspStart)
    {
        if (double.IsNaN(scheduledDspStart) || double.IsInfinity(scheduledDspStart))
        {
            throw new ArgumentOutOfRangeException(nameof(scheduledDspStart));
        }

        double currentDsp = AudioSettings.dspTime;
        if (scheduledDspStart <= currentDsp)
        {
            throw new ArgumentOutOfRangeException(
                nameof(scheduledDspStart), "El inicio programado debe estar en el futuro.");
        }

        if (AudioListener.pause && !_ownsListenerPause)
        {
            throw new InvalidOperationException(
                "Otro sistema tiene pausado el audio global.");
        }

        Stop();
        
        AudioSettings.GetDSPBufferSize(out int bufferLength, out _);
        
        int sampleRate = AudioSettings.outputSampleRate;
        
        _bufferDuration = (double) bufferLength / sampleRate * 1.0;
        
        _sampleRealTime = Time.realtimeSinceStartupAsDouble;

        _scheduleDpsStart = scheduledDspStart;

        _songTime = currentDsp - scheduledDspStart;
        _acceptInputsSinceRealtime = _sampleRealTime;
        
        State = ClockState.Running;
    }

    public void UpdateClock()
    {
        if (State != ClockState.Running)
        {
            return;
        }
        
        double currentRealTime = Time.realtimeSinceStartupAsDouble;
        
        double elapsed = currentRealTime - _sampleRealTime;

        if (elapsed <= 0.0) return;

        double predictedSongTime = _songTime + elapsed;
        
        double dspSongTime = AudioSettings.dspTime - _scheduleDpsStart;
        
        double error = predictedSongTime - dspSongTime;

        double excessError = 0.0;

        if (error > _bufferDuration)
        {
            excessError = error - _bufferDuration;
        }
        else if (error < -_bufferDuration)
        {
            excessError = error + _bufferDuration;    
        }
        
        double maxCorrection = CorrectionRate * elapsed;
        
        double correction = Math.Max(-maxCorrection, Math.Min(maxCorrection, excessError));
        
        // Positive error means the realtime estimate is ahead of DSP; subtract
        // the bounded correction so the clock slows down instead of drifting further.
        _songTime = Math.Max(_songTime, predictedSongTime - correction);
        
        _sampleRealTime = currentRealTime;
    }

    public bool TryGetSongAtRealTime(double inputRealTime, out double inputSongTime)
    {
        inputSongTime = 0.0;
        
        if (State != ClockState.Running)
            return false;

        if (double.IsNaN(inputRealTime) || double.IsInfinity(inputRealTime))
        {
            return false;
        }
        
        if (inputRealTime < _acceptInputsSinceRealtime) return false;
        
        // InputAction timestamps the original event. It can predate our most
        // recent clock sample; extrapolating backward preserves that timing.
        inputSongTime = _songTime + (inputRealTime - _sampleRealTime);

        return true;

    }

    public void Pause()
    {
        if (State != ClockState.Running)
            return;

        UpdateClock();

        AudioListener.pause = true;
        _ownsListenerPause = true;

        State = ClockState.Paused;
    }

    public void Resume()
    {
        if (State != ClockState.Paused)
            return;

        AudioListener.pause = false;
        _ownsListenerPause = false;

        double realtimeNow =
            Time.realtimeSinceStartupAsDouble;
            
        _sampleRealTime = realtimeNow;
        _acceptInputsSinceRealtime = realtimeNow;

        State = ClockState.Running;
    }

    public void Stop()
    {
        if (_ownsListenerPause)
        {
            AudioListener.pause = false;
            _ownsListenerPause = false;
        }

        _scheduleDpsStart = 0.0;
        _songTime = 0.0;
        _sampleRealTime = 0.0;
        _acceptInputsSinceRealtime = 0.0;
        _bufferDuration = 0.0;

        State = ClockState.Stopped;
    }
}
