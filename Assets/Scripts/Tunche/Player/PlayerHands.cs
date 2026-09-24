using UnityEngine;

public class PlayerHands : MonoBehaviour
{
    [SerializeField] private GameObject duckHands;
    [SerializeField] private GameObject crossHands;

    void Awake()
    {
        HideAll();
    }

    public void ShowDuck()
    {
        Set(true, false);
    }

    public void ShowCross()
    {
        Set(false, true);
    }

    public void HideAll()
    {
        Set(false, false);
    }

    private void Set(bool duck, bool cross)
    {
        if (duckHands != null)
            duckHands.SetActive(duck);

        if (crossHands != null)
            crossHands.SetActive(cross);
    }
}