using UnityEngine;

[System.Serializable]
public struct LabeledColor
{
    public string Label;
    [ColorUsage(false)] public Color Color;
    [ColorUsage(false, true)] public Color HDRColor;
}