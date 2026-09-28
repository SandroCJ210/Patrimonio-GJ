using UnityEngine;

public class PlayerHands : MonoBehaviour
{
    [SerializeField] GameObject crossHands;

    void Awake()
    {
        if (crossHands != null) crossHands.SetActive(false);
    }

    public void ShowCross()
    {
        if (crossHands != null) crossHands.SetActive(true);
    }

    public void HideCross()
    {
        if (crossHands != null) crossHands.SetActive(false);
    }
}
