using UnityEngine;
using UnityEngine.EventSystems;

public class MinigameStartButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject target;
    [SerializeField] GameObject partialDialogBox;
    [SerializeField] GameObject fullDialogBox;
    [SerializeField] string minigameSceneName = "Minigame";

    public void OnPointerEnter(PointerEventData e) => target.SetActive(true);
    public void OnPointerExit(PointerEventData e) => target.SetActive(false);

    public void OnClick()
    {
        fullDialogBox.SetActive(false);
        partialDialogBox.SetActive(true);
        CoreManager.I.Router.Go(minigameSceneName);
    }
}
