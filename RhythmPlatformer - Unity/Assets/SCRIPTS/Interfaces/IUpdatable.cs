using System;
using Systems;

namespace Interfaces
{
    public interface IUpdatable
    {
        public UpdateType UpdateType { get; }

        public void OnUpdate();
        public void OnFixedUpdate();

        public void RegisterUpdatable(bool in_fixedUpdate) => 
            UpdateManager.Instance.RegisterUpdatable(this, true);
        public void DeregisterUpdatable(bool in_fixedUpdate) => 
            UpdateManager.Instance.DeregisterUpdatable(this, true);
    }

    [Flags]
    public enum UpdateType
    {
        GamePlay = 1,
        Paused = 2,
        Always = ~0
    }
}
