using System;
using Systems;
using Unity.VisualScripting;

namespace Interfaces
{
    public interface IUpdatable
    {
        public UpdateType UpdateType { get; }

        public void CustomUpdate();

        public void RegisterUpdatable(bool in_fixedUpdate = false) => 
            UpdateManager.Instance.RegisterUpdatable(this, in_fixedUpdate);
        public void DeregisterUpdatable(bool in_fixedUpdate = false) => 
            UpdateManager.Instance.DeregisterUpdatable(this, in_fixedUpdate);
    }

    [Flags]
    public enum UpdateType
    {
        GamePlay = 1,
        Paused = 2,
        Always = ~0
    }
}
