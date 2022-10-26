using System.Collections;
using System.Collections.Generic;
using Interfaces_and_Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlobalSystems
{
    public static class SceneLoadManager
    {
        public static readonly List<IRefreshable> Refreshables = new();

        private static void RefreshGlobalObjects()
        {
            foreach (IRefreshable refreshable in Refreshables)
            {
                refreshable.SceneRefresh();
            }
        }

        public static IEnumerator LoadSceneCoroutine(string in_sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(in_sceneName);

            GameStateManager.s_ActiveUpdateType = UpdateType.Nothing;
            
            while (!asyncLoad.isDone)
            {
                // TODO: loading screen logic
                yield return null;
            }

            RefreshGlobalObjects();
        }
    }
}
