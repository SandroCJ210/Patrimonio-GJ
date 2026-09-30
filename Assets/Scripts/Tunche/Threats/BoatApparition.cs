using UnityEngine;

public class BoatApparition : TuncheEvent
{
    public enum ApparitionKind { TuncheReal, Illusion }

    [SerializeField] PlayerStateController player;
    [SerializeField] SpriteRenderer visual;
    [SerializeField] Sprite realSprite;
    [SerializeField] Sprite illusionSprite;
    [SerializeField] Color realTint = new Color(1f, .62f, .62f, 1f);
    [SerializeField] Color illusionTint = new Color(.65f, .9f, 1f, 1f);
    [SerializeField] float reactionWindow = 3f;

    [SerializeField] ApparitionSpawner spawner;

    [SerializeField] AudioSource apparitionAudio;

    ApparitionKind kind;
    float deadline;

    public void Activate(ApparitionKind kind)
    {
        if (apparitionAudio != null)
            apparitionAudio.Play();
        this.kind = kind;
        BeginEvent();

        if (spawner != null && visual != null)
            spawner.PlaceRandomly(visual.transform);

        if (visual != null)
        {
            visual.sprite = kind == ApparitionKind.TuncheReal ? realSprite : illusionSprite;
            visual.color = kind == ApparitionKind.TuncheReal ? realTint : illusionTint;
            visual.enabled = true;
        }

        // Mientras la figura está presente se bloquea el agachado.
        if (player != null) player.SetDuckBlocked(true);

        deadline = Time.time + reactionWindow;

    }

    void OnEnable()
    {
        if (player != null) player.CrossUsed += OnCrossUsed;
    }

    void OnDisable()
    {
        if (player != null) player.CrossUsed -= OnCrossUsed;
    }

    void OnCrossUsed()
    {
        if (!Active) return;
        Resolve(true);
    }

    void Update()
    {
        if (!Active) return;
        if (Time.time >= deadline)
            Resolve(kind == ApparitionKind.Illusion);
    }

    protected override void OnResolved(bool survived)
    {
        if (visual != null) visual.enabled = false;
        if (player != null) player.SetDuckBlocked(false);

        if (apparitionAudio != null)
            apparitionAudio.Stop();
    }

    public override void ForceReset()
    {
        base.ForceReset();
        if (visual != null) visual.enabled = false;
        if (player != null) player.SetDuckBlocked(false);

        if (apparitionAudio != null)
            apparitionAudio.Stop();
    }
}
