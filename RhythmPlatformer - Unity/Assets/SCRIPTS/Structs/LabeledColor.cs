using UnityEngine;

[System.Serializable]
public struct LabeledColor
{
    public string Label;
    public Color Color;
    [ColorUsage(true, true)] public Color HDRColor;
}