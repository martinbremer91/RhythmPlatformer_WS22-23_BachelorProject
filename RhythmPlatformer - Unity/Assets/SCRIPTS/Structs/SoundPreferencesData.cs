using System;

namespace Structs
{
    [Serializable]
    public struct SoundPreferencesData
    {
        public SoundPreferencesData(float in_musicVolume, bool in_musicMuted,
            float in_metronomeVolume, bool in_metronomeMuted)
        {
            CurrentMusicVolume = in_musicVolume;
            MusicMuted = in_musicMuted;
            CurrentMetronomeVolume = in_metronomeVolume;
            MetronomeMuted = in_metronomeMuted;
        }
        
        public float CurrentMusicVolume;
        public bool MusicMuted;
        public float CurrentMetronomeVolume;
        public bool MetronomeMuted;
    }
}
