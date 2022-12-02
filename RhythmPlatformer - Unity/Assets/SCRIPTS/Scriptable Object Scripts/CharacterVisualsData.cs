using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterVisuals", menuName = "Custom/CharacterVisuals")]
public class CharacterVisualsData : ScriptableObject
{
    public List<LabeledColor> Colors;

    public float SilhouetteMatMaxDistance;
    public Vector2 SilhouetteMatProximityAlphaRange;

    public LabeledColor GetColorByLabel(string in_label) =>
        Colors.FirstOrDefault(c => c.Label == in_label);
}
