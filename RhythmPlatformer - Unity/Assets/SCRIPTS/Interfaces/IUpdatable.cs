using System;
using Systems;

namespace Interfaces
{
    public interface IUpdatable
    {
        public UpdateType UpdateType { get; }
        
        public void OnUpdate();

        public void RegisterUpdatable() => UpdateManager.Instance.RegisterUpdatable(this);
        public void DeregisterUpdatable() => UpdateManager.Instance.DeregisterUpdatable(this);
    }

    [Flags]
    public enum UpdateType
    {
        GamePlay = 1,
        Paused = 2,
        Always = ~0
    }
}
