using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewColorPalette", menuName = "Custom/Color Palette")]
public class ColorPalette : ScriptableObject
{
    public List<LabeledColor> Colors;

    public LabeledColor GetColorByLabel(string in_label) =>
        Colors.FirstOrDefault(c => c.Label == in_label);
}
