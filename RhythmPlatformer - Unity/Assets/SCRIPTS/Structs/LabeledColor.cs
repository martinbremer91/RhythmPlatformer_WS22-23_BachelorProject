using UnityEngine;

[System.Serializable]
public struct LabeledColor
{
    [ColorUsage(false)] public Color Color;
    [ColorUsage(false, true)] public Color HDRColor;
    public string Label;
}