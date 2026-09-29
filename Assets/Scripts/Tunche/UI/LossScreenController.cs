using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LossScreenController : MonoBehaviour
{
    [SerializeField] CanvasGroup group;
    [SerializeField] Text message;
    [SerializeField] Button replayButton;
    [SerializeField] Button menuButton;
    [SerializeField] ThreatDirector director;
    [SerializeField] string menuSceneName = "Menu";

    void Awake()
    {
        Hide();
        if (replayButton != null) replayButton.onClick.AddListener(OnReplay);
        if (menuButton != null) menuButton.onClick.AddListener(OnMenu);
    }

    public void Show()
    {
        if (message != null) message.text = "EL TUNCHE TE ENCONTRÓ";
        SetVisible(true);
    }

    public void ShowWin()
    {
        if (message != null) message.text = "SOBREVIVISTE A LA NOCHE";
        SetVisible(true);
    }

    public void Hide() => SetVisible(false);

    void SetVisible(bool visible)
    {
        if (group == null) return;
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    void OnReplay()
    {
        if (director != null) director.Restart();
    }

    void OnMenu()
    {
        if (CoreManager.I != null && CoreManager.I.Router != null)
            CoreManager.I.Router.Go(menuSceneName);
        else
            SceneManager.LoadScene(menuSceneName);
    }
}
