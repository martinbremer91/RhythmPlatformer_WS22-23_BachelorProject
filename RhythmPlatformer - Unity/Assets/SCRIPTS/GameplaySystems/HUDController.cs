using UnityEngine;
using TMPro;
using Scriptable_Object_Scripts;
using System.Linq;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text _beatDisplayText;
    [SerializeField] private RectTransform _beatLine;

    [SerializeField] private RectTransform[] _leftBeatMarkers;
    [SerializeField] private RectTransform[] _rightBeatMarkers;

    private float _beatLineHalfLength;
    private float _beatMarkerSpeed;

    private float _farthestMarkerPositionX;
    private int[] _beatsAsCountedOutLoud;

    public void InitializeHUD(float in_beatLength, TrackData in_trackData)
    {
        _beatLineHalfLength = Mathf.Abs(_beatLine.rect.x);
        _beatMarkerSpeed = _beatLineHalfLength * .5f / in_beatLength;

        _farthestMarkerPositionX = 
            _beatLine.anchoredPosition.x + 1.5f * _beatLineHalfLength;

        float yPosition = _beatLine.anchoredPosition.y;

        for (int i = 0; i < _leftBeatMarkers.Length; i++)
        {
            _leftBeatMarkers[i].anchoredPosition = 
                new Vector2(_beatLine.anchoredPosition.x - (i + 1) * .5f * _beatLineHalfLength, yPosition);
            
            _rightBeatMarkers[i].anchoredPosition =
                new Vector2(_beatLine.anchoredPosition.x + (i + 1) * .5f * _beatLineHalfLength, yPosition);
        }

        _beatDisplayText.text = in_trackData.Meter.ToString();
        _beatsAsCountedOutLoud = new int[in_trackData.Meter];

        int counter = 1;
        for (int i = 0; i < _beatsAsCountedOutLoud.Length; i++)
        {
            _beatsAsCountedOutLoud[i] = counter;
            
            if (i != 1 && in_trackData.EventBeats.Any(b => b == i + 1))
                counter = 1;
            else
                counter++;
        }
    }

    public void UpdateHUD(int in_beatTracker)
    {
        foreach (RectTransform beatMarker in _leftBeatMarkers)
            MoveBeatMarker(beatMarker, true);
        foreach (RectTransform beatMarker in _rightBeatMarkers)
            MoveBeatMarker(beatMarker, false);

        SetBeatDisplayText();

        void MoveBeatMarker(RectTransform in_beatMarker, bool in_leftMarker)
        {
            in_beatMarker.anchoredPosition +=
                (in_leftMarker ? Vector2.right : Vector2.left) * 
                _beatMarkerSpeed * Time.deltaTime;

            if (in_leftMarker && in_beatMarker.anchoredPosition.x > _beatLine.anchoredPosition.x)
            {
                in_beatMarker.anchoredPosition =
                    new Vector2(-_farthestMarkerPositionX, _beatLine.anchoredPosition.y);
                return;
            }

            if (!in_leftMarker && in_beatMarker.anchoredPosition.x < _beatLine.anchoredPosition.x)
            {
                in_beatMarker.anchoredPosition =
                    new Vector2(_farthestMarkerPositionX, _beatLine.anchoredPosition.y);
            }
        }

        void SetBeatDisplayText()
        {
            if (in_beatTracker < 1)
                return;
            _beatDisplayText.text = _beatsAsCountedOutLoud[in_beatTracker - 1].ToString();
        }
    }
}
