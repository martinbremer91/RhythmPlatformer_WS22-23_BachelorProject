using Interfaces;
using UnityEngine;

namespace Gameplay
{
    public class CharacterController : GameplayComponent, IUpdatable
    {
        #region REFERENCES

        [SerializeField] private CharacterSpriteController spriteController;

        #endregion
        
        public override void OnUpdate()
        {
            DetectKeyboardInput();
        }

        private void DetectKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.A))
                HandleHorizontalInput(true);
            if (Input.GetKeyDown(KeyCode.D))
                HandleHorizontalInput(false);
        }

        private void HandleHorizontalInput(bool faceLeft, float axisValue = 1)
        {
            if (spriteController.characterFacingLeft != faceLeft)
                spriteController.SetCharacterOrientation(faceLeft);
            
            
        }
    }
}
