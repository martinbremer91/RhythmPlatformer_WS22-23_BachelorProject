using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VisualsData", menuName = "Custom/VisualsData")]
public class VisualsData : ScriptableObject
{
    [Header("CHARACTER VISUALS DATA")]
    public List<LabeledColor> CharacterColors;

    public float SilhouetteMatMaxDistance;
    public Vector2 SilhouetteMatProximityAlphaRange;

    [Space(10)]
    [Header("COMPANION VISUALS DATA")]
    public List<LabeledColor> CompanionColors;

    [Space(10)]
    [Header("INTERACTABLES VISUALS DATA")]
    public List<LabeledColor> InteractablesColors;

    public LabeledColor GetColorByLabel(LabeledColor[] in_colors, string in_label) =>
        in_colors.FirstOrDefault(c => c.Label == in_label);
}
