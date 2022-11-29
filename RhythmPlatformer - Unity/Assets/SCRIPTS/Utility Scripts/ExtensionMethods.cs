using UnityEngine;

public static class ExtensionMethods
{
    public static float InverseLerp(this Vector3 vectorA, Vector3 vectorB, Vector3 value)
    {
        Vector3 AB = vectorB - vectorA;
        Vector3 AV = value - vectorA;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    public static float InverseLerp(this Vector2 vectorA, Vector3 vectorB, Vector3 value) {
        Vector3 AB = vectorB - (Vector3)vectorA;
        Vector3 AV = value - (Vector3)vectorA;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }
}
