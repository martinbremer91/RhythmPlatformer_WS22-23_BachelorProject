using System;
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

        public static Action SceneLoaded;

        public static void RefreshGlobalObjects()
        {
            foreach (IRefreshable refreshable in s_Refreshables)
                refreshable.SceneRefresh();
        }

        public static IEnumerator LoadSceneCoroutine(string in_sceneName, UiManager in_uiManager)
        {
            bool loadCompleted = false;
            in_uiManager.LoadingScreen.SetActive(true);
            GameStateManager.s_Instance.ActiveUpdateType = UpdateType.Nothing;
            SceneLoaded?.Invoke();

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(in_sceneName);
            asyncLoad.completed += OnSceneLoaded;

            while (!loadCompleted)
                yield return null;

            void OnSceneLoaded(AsyncOperation in_asyncOperation) {
                in_asyncOperation.completed -= OnSceneLoaded;
                in_uiManager.LoadingScreen.SetActive(false);
                RefreshGlobalObjects();
                loadCompleted = true;
            }
        }
    }
}
