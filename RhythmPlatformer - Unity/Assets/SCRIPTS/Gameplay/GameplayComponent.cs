using Interfaces;
using UnityEngine;

namespace Gameplay
{
    public abstract class GameplayComponent : MonoBehaviour, IUpdatable
    {
        public virtual UpdateType UpdateType => UpdateType.GamePlay;

        protected virtual void OnEnable() => (this as IUpdatable).RegisterUpdatable(true);
        protected virtual void OnDisable() => (this as IUpdatable).DeregisterUpdatable(true);

        /// <summary>
        /// GameplayComponent CustomUpdates are called in UpdateManager.FixedUpdate
        /// </summary>
        public abstract void CustomUpdate();
    }
}
