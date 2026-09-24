using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogView : MonoBehaviour
{
    [SerializeField] LinePlayer linePlayer;
    [SerializeField] CanvasGroup group;
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] TMP_Text textLabel;
    [SerializeField] RawImage speakerImage;
    [SerializeField] float fadeSpeed = 8f;
    readonly List<float> starts = new();
    float totalWeight;
    int visible;
    float targetAlpha;

    void Awake() => group.alpha = 0f;

    void OnEnable()
    {
        linePlayer.LineStarted += Show;
        linePlayer.LineEnded += Hide;
    }

    void OnDisable()
    {
        linePlayer.LineStarted -= Show;
        linePlayer.LineEnded -= Hide;
    }

    void Show(NarrationLine line)
    {
        bool hasSpeaker = line.speaker != null;
        nameLabel.gameObject.SetActive(hasSpeaker);
        if (hasSpeaker)
        {
            speakerImage.texture = line.speaker.Picture;
            nameLabel.text = line.speaker.Name;
        }

        textLabel.text = line.text;
        textLabel.maxVisibleCharacters = 0;
        textLabel.ForceMeshUpdate();
        BuildWeights();

        visible = 0;
        targetAlpha = 1f;
    }

    void Hide() => targetAlpha = 0f;

    void Update()
    {
        group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        if (targetAlpha == 0f) return;

        float budget = linePlayer.RevealProgress * totalWeight;
        while (visible < starts.Count && starts[visible] < budget)
            visible++;

        textLabel.maxVisibleCharacters = visible;
    }

    void BuildWeights()
    {
        starts.Clear();
        totalWeight = 0f;

        var info = textLabel.textInfo;
        for (int i = 0; i < info.characterCount; i++)
        {
            starts.Add(totalWeight);
            totalWeight += WeightOf(info.characterInfo[i].character);
        }
    }

    static float WeightOf(char c) => c switch
    {
        ',' or ';' or ':'         => 4f,
        '.' or '!' or '?' or '…'  => 8f,
        _                          => 1f
    };
}