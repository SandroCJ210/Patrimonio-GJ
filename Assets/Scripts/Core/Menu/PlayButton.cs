using UnityEngine;

public class PlayButton : MonoBehaviour
{
    public string target;
    public void Play() => CoreManager.I.Router.Go(target);
}