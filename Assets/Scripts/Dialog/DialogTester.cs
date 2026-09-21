using UnityEngine;
using UnityEngine.InputSystem;

public class DialogTester : MonoBehaviour
{
    [SerializeField] LinePlayer player;
    [SerializeField] NarrationLine lineA;
    [SerializeField] NarrationLine lineB;

    void OnEnable()
    {
        player.LineStarted += l => Debug.Log($"[{Time.time:F2}] LineStarted: {l.text}");
        player.LineEnded   += () => Debug.Log($"[{Time.time:F2}] LineEnded");
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) player.Play(lineA, () => Debug.Log("A completa"));
        if (kb.digit2Key.wasPressedThisFrame) player.Play(lineB, () => Debug.Log("B completa"));
        if (kb.rKey.wasPressedThisFrame)      player.RevealAll();
        if (kb.sKey.wasPressedThisFrame)      player.Stop();
        if (kb.dKey.wasPressedThisFrame)
            player.gameObject.SetActive(!player.gameObject.activeSelf);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 20), $"IsPlaying: {player.IsPlaying}");
        GUI.Label(new Rect(10, 30, 400, 20), $"Reveal: {player.RevealProgress:F2}");
        GUI.Label(new Rect(10, 50, 400, 20), "1:A  2:B  R:reveal  S:stop  D:toggle voice");
    }
}