using Interfaces;
using UnityEngine;

namespace Gameplay
{
    public class CharacterController : MonoBehaviour, IUpdatable
    {
        #region iUPDATABLE BOILERPLATE
        public UpdateType UpdateType => UpdateType.GamePlay;

        private void OnEnable() => (this as IUpdatable).RegisterUpdatable();
        private void OnDisable() => (this as IUpdatable).DeregisterUpdatable();
        #endregion

        void IUpdatable.OnUpdate()
        {
        
        }
        
        
    }
}
