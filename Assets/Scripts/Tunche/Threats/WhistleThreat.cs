using UnityEngine;

public class WhistleThreat : TuncheEvent
{
    [SerializeField] PlayerStateController player;
    [SerializeField] Camera viewCamera;
    [SerializeField] WhistleAudioController audioController;
    [SerializeField] SpriteRenderer silhouetteLeft;
    [SerializeField] SpriteRenderer silhouetteRight;
    [SerializeField, Min(1f)] float reactionWindow = 6f;

    [SerializeField, Range(0f, 1f)] float panAmount = 0.8f;

    [SerializeField, Range(0f, 0.49f)] float viewportMarginX = 0.15f;
    [SerializeField, Range(0f, 0.49f)] float viewportMarginY = 0.1f;

    SpriteRenderer currentSilhouette;
    float deadline;
    bool seen;

    public void Activate()
    {
        ForceReset();
        BeginEvent();
        seen = false;
        currentSilhouette = Random.value < 0.5f ? silhouetteLeft : silhouetteRight;
        if (currentSilhouette != null) currentSilhouette.enabled = true;
        if (audioController != null)
            audioController.PlayDirectional(ComputePan());
        deadline = Time.time + reactionWindow;
    }

    void OnEnable() { if (player != null) player.DuckStarted += OnDucked; }
    void OnDisable() { if (player != null) player.DuckStarted -= OnDucked; ForceReset(); }

    void Update()
    {
        if (!Active) return;
        if (Time.time >= deadline) { Resolve(false); return; }
        if (seen || currentSilhouette == null || viewCamera == null || player == null || !player.IsIdle) return;

        Bounds b = currentSilhouette.bounds;
        Vector3 p = viewCamera.WorldToViewportPoint(b.center);
       
        seen = p.z > 0f
            && p.x >= viewportMarginX && p.x <= 1f - viewportMarginX
            && p.y >= viewportMarginY && p.y <= 1f - viewportMarginY;
    }

    void OnDucked()
    {
        if (!Active) return;
        if (seen) Resolve(true);
      
    }

    float ComputePan()
    {
        if (viewCamera == null || currentSilhouette == null)
            return currentSilhouette == silhouetteLeft ? -panAmount : panAmount;

        float viewportX = viewCamera.WorldToViewportPoint(currentSilhouette.bounds.center).x;
        float signed = Mathf.Clamp((viewportX - 0.5f) * 2f, -1f, 1f); // -1 (izquierda) .. 1 (derecha)
        return signed * panAmount;
    }

    protected override void OnResolved(bool survived)
    {
        HideSilhouettes();
        if (audioController != null) audioController.Stop(fade: survived);
    }

    void HideSilhouettes()
    {
        if (silhouetteLeft != null) silhouetteLeft.enabled = false;
        if (silhouetteRight != null) silhouetteRight.enabled = false;
        currentSilhouette = null;
    }

    public override void ForceReset()
    {
        base.ForceReset();
        seen = false;
        HideSilhouettes();
        if (audioController != null) audioController.Stop(fade: false);
    }
}
