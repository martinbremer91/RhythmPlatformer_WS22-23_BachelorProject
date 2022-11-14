using System.Threading.Tasks;
using UnityEngine;

namespace Utility_Scripts
{
    public class MusicUtilities
    {
        public async void FadeVolumeAsync(AudioSource[] in_audioSources, float in_duration, float in_targetVolume)
        {
            float currentTime = 0;
            float startCutoff = in_audioSources[0].volume;
    
            while (currentTime <= in_duration && Time.unscaledDeltaTime > 0)
            {
                currentTime += Time.unscaledDeltaTime;
                float cutoffFrequency = Mathf.Lerp(startCutoff, in_targetVolume, currentTime / in_duration);
                
                foreach (AudioSource source in in_audioSources)
                    source.volume = cutoffFrequency;
                
                await Task.Yield();
            }
        }
        
        public async void FadeLowPassFilterAsync(AudioLowPassFilter[] in_lowPassArray, float in_duration, 
            float in_targetCutoff)
        {
            float currentTime = 0;
            float startCutoff = in_lowPassArray[0].cutoffFrequency;
    
            while (currentTime <= in_duration && Time.unscaledDeltaTime > 0)
            {
                currentTime += Time.unscaledDeltaTime;
                float cutoffFrequency = Mathf.Lerp(startCutoff, in_targetCutoff, currentTime / in_duration);
                
                foreach (AudioLowPassFilter loPass in in_lowPassArray)
                    loPass.cutoffFrequency = cutoffFrequency;
                
                await Task.Yield();
            }
        }
    }
}
