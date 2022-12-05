using System.Threading.Tasks;
using UnityEngine;

namespace Utility_Scripts
{
    public class MusicUtilities
    {
        private static int _volumeFadeID;
        private static int _loPassFadeID;

        public async void FadeVolumeAsync(AudioSource[] in_audioSources, float in_duration, float in_targetVolume)
        {
            _volumeFadeID++;
            int currentID = _volumeFadeID;

            float currentTime = 0;
            float startCutoff = in_audioSources[0].volume;
    
            while (currentID == _volumeFadeID && currentTime <= in_duration && Time.deltaTime > 0)
            {
                currentTime += Time.deltaTime;
                float cutoffFrequency = Mathf.Lerp(startCutoff, in_targetVolume, currentTime / in_duration);
                
                foreach (AudioSource source in in_audioSources)
                    source.volume = cutoffFrequency;
                
                await Task.Yield();
            }
        }
        
        public async void FadeLowPassFilterAsync(AudioLowPassFilter[] in_lowPassArray, float in_duration, 
            float in_targetCutoff, bool in_disable = false)
        {
            _loPassFadeID++;
            int currentID = _loPassFadeID;

            foreach (AudioLowPassFilter lowPass in in_lowPassArray)
                lowPass.enabled = true;

            float currentTime = 0;
            float startCutoff = in_lowPassArray[0].cutoffFrequency;
    
            while (currentID == _loPassFadeID && currentTime <= in_duration && Time.deltaTime > 0)
            {
                currentTime += Time.deltaTime;
                float cutoffFrequency = Mathf.Lerp(startCutoff, in_targetCutoff, currentTime / in_duration);
                
                foreach (AudioLowPassFilter loPass in in_lowPassArray)
                    loPass.cutoffFrequency = cutoffFrequency;
                
                await Task.Yield();
            }

            if (currentID == _loPassFadeID && in_disable)
                foreach (AudioLowPassFilter lowPass in in_lowPassArray)
                    lowPass.enabled = false;
        }
    }
}
