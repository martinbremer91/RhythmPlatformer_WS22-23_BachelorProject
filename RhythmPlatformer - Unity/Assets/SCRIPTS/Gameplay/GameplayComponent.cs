using Interfaces;
using UnityEngine;

namespace Gameplay
{
    public abstract class GameplayComponent : MonoBehaviour, IUpdatable
    {
        public virtual UpdateType UpdateType => UpdateType.GamePlay;

        protected virtual void OnEnable() => (this as IUpdatable).RegisterUpdatable(true);
        protected virtual void OnDisable() => (this as IUpdatable).DeregisterUpdatable(true);

        public virtual void OnUpdate() {}
        public abstract void OnFixedUpdate();
    }
}
