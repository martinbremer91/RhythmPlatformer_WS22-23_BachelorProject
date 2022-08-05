#if UNITY_EDITOR
using UnityEngine;

namespace Debug_and_Tools
{
    public class GraphVisualizer : MonoBehaviour
    {
        private static GraphVisualizer s_instance;
        public AnimationCurve Plot = new();

        private static bool _isActive;
        
        private void Awake()
        {
            if (s_instance == null)
                s_instance = this;
            else
                Destroy(gameObject);
        }

        public static void PlotValue(float in_value) => s_instance.Plot.AddKey(Time.realtimeSinceStartup, in_value);
    }
}
#endif
