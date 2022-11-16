using GlobalSystems;

namespace Interfaces_and_Enums
{
    public interface IRefreshable
    {
        /// <summary>
        /// Funciton called whenever a scene is loaded, including on application startup
        /// </summary>
        public void SceneRefresh();

        public void RegisterRefreshable() => SceneLoadManager.s_Refreshables.Add(this);
        public void DeregisterRefreshable() => SceneLoadManager.s_Refreshables.Remove(this);
    }
}
