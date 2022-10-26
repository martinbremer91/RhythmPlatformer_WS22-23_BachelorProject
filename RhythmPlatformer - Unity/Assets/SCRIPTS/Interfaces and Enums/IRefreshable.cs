using GlobalSystems;

namespace Interfaces_and_Enums
{
    public interface IRefreshable
    {
        public void SceneRefresh();

        public void RegisterRefreshable() => SceneLoadManager.Refreshables.Add(this);
        public void DeregisterRefreshable() => SceneLoadManager.Refreshables.Remove(this);
    }
}
