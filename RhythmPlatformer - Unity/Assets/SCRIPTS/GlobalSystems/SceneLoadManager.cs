using System.Collections;
using System.Collections.Generic;
using Interfaces_and_Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlobalSystems
{
    public static class SceneLoadManager
    {
        private static List<IRefreshable> s_refreshables;
        public static List<IRefreshable> s_Refreshables => s_refreshables ??= new();

        public static void RefreshGlobalObjects()
        {
            foreach (IRefreshable refreshable in s_Refreshables)
                refreshable.SceneRefresh();
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
