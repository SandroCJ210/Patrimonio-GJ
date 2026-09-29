using System;
using UnityEngine;

public abstract class TuncheEvent : MonoBehaviour
{
    public event Action<bool> Resolved;

    protected bool Active { get; private set; }
    public bool IsActive => Active;
    bool resolved;

    protected void BeginEvent()
    {
        resolved = false;
        Active = true;
    }

    protected void Resolve(bool survived)
    {
        if (resolved) return;
        resolved = true;
        Active = false;
        OnResolved(survived);
        Resolved?.Invoke(survived);
    }

    protected virtual void OnResolved(bool survived) { }

    public virtual void ForceReset()
    {
        Active = false;
        resolved = false;
    }
}
