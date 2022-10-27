using GlobalSystems;

namespace Interfaces_and_Enums
{
    public interface IRefreshable
    {
        public void SceneRefresh();

        public void RegisterRefreshable() => SceneLoadManager.s_Refreshables.Add(this);
        public void DeregisterRefreshable() => SceneLoadManager.s_Refreshables.Remove(this);
    }
}
