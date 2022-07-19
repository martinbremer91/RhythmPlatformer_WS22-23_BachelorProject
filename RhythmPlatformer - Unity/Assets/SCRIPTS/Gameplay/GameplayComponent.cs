using Interfaces;
using UnityEngine;

namespace Gameplay
{
    public abstract class GameplayComponent : MonoBehaviour, IUpdatable
    {
        public virtual UpdateType UpdateType => UpdateType.GamePlay;

        protected virtual void OnEnable() => (this as IUpdatable).RegisterUpdatable();
        protected virtual void OnDisable() => (this as IUpdatable).DeregisterUpdatable();

        public virtual void OnUpdate() { }
    }
}
