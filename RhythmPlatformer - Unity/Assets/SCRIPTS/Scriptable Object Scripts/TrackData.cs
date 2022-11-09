using System.Linq;
using UnityEngine;

namespace Scriptable_Object_Scripts
{
    [CreateAssetMenu(fileName = "NewTrackData", menuName = "Custom/TrackData")]
    public class TrackData : ScriptableObject
    {
        public AudioClip Clip;
        
        public int BPM = 100;
        public int Meter = 4;
        public int IntroBars;
        public int TailBars;
        [Space]
        public int[] EventBeats;

        private void Awake()
        {
            if (EventBeats == null || !EventBeats.Any())
                EventBeats = new[] {1};
        }
    }
}
