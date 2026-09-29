using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class FeedbackUI : StaticInstance<FeedbackUI>
{
    [SerializeField] private Image _feedbackImg;
    [SerializeField] private double _feedbackTimeThreshold = 1.5;
    [Tooltip("Index [0: pitrimiri], [1, Bacan], [2, Ok], [3, Falla]")]
    [SerializeField] private Sprite[] _sprites;

    private const float FadeDurationSeconds = 0.25f;
    
    private double _timeSinceLastUpdate = 0;
    
    Dictionary<NoteScore, Sprite> _spritesDictionary = new Dictionary<NoteScore, Sprite>();
    private void Start()
    {
        _spritesDictionary.Add(NoteScore.PitriMitri, _sprites[0]);
        _spritesDictionary.Add(NoteScore.Bacan, _sprites[1]);
        _spritesDictionary.Add(NoteScore.Ok, _sprites[2]);
        _spritesDictionary.Add(NoteScore.Falla, _sprites[3]);

        SetAlpha(0f);
    }

    public void UpdateFeedback(NoteScore score)
    {
        _feedbackImg.sprite = _spritesDictionary[score];
        SetAlpha(1f);
        _timeSinceLastUpdate = 0;
    }

    private void Update()
    {
        _timeSinceLastUpdate += Time.deltaTime;

        if (_timeSinceLastUpdate <= _feedbackTimeThreshold)
            return;

        float fadeProgress = (float)(
            (_timeSinceLastUpdate - _feedbackTimeThreshold) / FadeDurationSeconds);
        SetAlpha(1f - Mathf.Clamp01(fadeProgress));
    }

    private void SetAlpha(float alpha)
    {
        Color color = _feedbackImg.color;
        color.a = alpha;
        _feedbackImg.color = color;
    }
}
