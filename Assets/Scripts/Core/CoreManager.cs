using UnityEngine;

public class CoreManager : MonoBehaviour
{
    public static CoreManager I { get; private set; }

    [SerializeField] LinePlayer linePlayer;
    [SerializeField] SceneRouter router;
    [SerializeField] GameState state;
    [SerializeField] GameObject dialogScreen;

    public LinePlayer Lines => linePlayer;
    public SceneRouter Router => router;
    public GameState State => state;
    public GameObject DialogScreen => dialogScreen;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }
}
