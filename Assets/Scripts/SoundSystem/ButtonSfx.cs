using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSfx : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private string clickSfxName;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(PlayClickSfx);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayClickSfx);
    }

    private void PlayClickSfx()
    {
        AudioManager.TryPlay(clickSfxName);
    }
}
