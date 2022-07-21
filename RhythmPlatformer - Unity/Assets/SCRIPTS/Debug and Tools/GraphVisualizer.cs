#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Debug_and_Tools
{
    public class GraphVisualizer : MonoBehaviour
    {
        private static GraphVisualizer Instance;
        public AnimationCurve plot = new AnimationCurve();

        private static bool isActive;
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public static void PlotValue(float value) => Instance.plot.AddKey(Time.realtimeSinceStartup, value);
    }
}
#endif
