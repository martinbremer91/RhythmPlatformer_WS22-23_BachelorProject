using GlobalSystems;
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

            bool quitFunction = false;
            SceneLoadManager.SceneUnloaded += QuitFunction;

            while (!CheckQuitFunction() && currentID == _volumeFadeID && currentTime <= in_duration)
            {
                currentTime += Time.deltaTime;
                float cutoffFrequency = Mathf.Lerp(startCutoff, in_targetVolume, currentTime / in_duration);
                
                foreach (AudioSource source in in_audioSources)
                    source.volume = cutoffFrequency;
                
                await Task.Yield();
            }

            bool CheckQuitFunction() => quitFunction || GameStateManager.GameQuitting;

            void QuitFunction()
            {
                SceneLoadManager.SceneUnloaded -= QuitFunction;
                quitFunction = true;
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

            bool quitFunction = false;
            SceneLoadManager.SceneUnloaded += QuitFunction;

            while (!CheckQuitFunction() && currentID == _loPassFadeID && currentTime <= in_duration)
            {
                currentTime += Time.deltaTime;
                float cutoffFrequency = Mathf.Lerp(startCutoff, in_targetCutoff, currentTime / in_duration);
                
                foreach (AudioLowPassFilter loPass in in_lowPassArray)
                    loPass.cutoffFrequency = cutoffFrequency;
                
                await Task.Yield();
            }

            if (!GameStateManager.GameQuitting && currentID == _loPassFadeID && in_disable)
                foreach (AudioLowPassFilter lowPass in in_lowPassArray)
                    lowPass.enabled = false;

            bool CheckQuitFunction() => quitFunction || GameStateManager.GameQuitting;

            void QuitFunction()
            {
                SceneLoadManager.SceneUnloaded -= QuitFunction;
                quitFunction = true;
            }
        }
    }
}
