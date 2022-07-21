using System;
using Systems;

namespace Gameplay
{
    public class CharacterStateController : GameplayComponent
    {
        #region VARIABLES

        private static CharacterState _currentCharacterState;
        public static CharacterState CurrentCharacterState
        {
            get => _currentCharacterState;
            private set => SetCharacterState(value);
        }
        
        private static void SetCharacterState(CharacterState value) => _currentCharacterState = value;

        private static bool _facingLeft;
        private static bool FacingLeft
        {
            get => _facingLeft;
            set
            {
                if (_facingLeft == value)
                    return;
                
                ReferenceManager.Instance.CharacterSpriteController.SetCharacterOrientation(value);
                _facingLeft = value;
            }
        }
        
        private static bool canTurn => CharacterState.CanTurn.HasFlag(CurrentCharacterState);
        private static bool canCrouch => CharacterState.CanCrouch.HasFlag(CurrentCharacterState);

        #endregion

        private void Start()
        {
            CurrentCharacterState = CharacterState.Idle;
        }

        public override void OnUpdate()
        {
            CurrentStateUpdate();
            ExecuteCurrentStateFunctions();
        }

        private void CurrentStateUpdate()
        {
            switch (CurrentCharacterState)
            {
                
            }
        }

        private void ExecuteCurrentStateFunctions()
        {
            
        }
    }

    [Flags]
    public enum CharacterState
    {
        Idle = 1,
        Run = 2,
        Crouch = 4,
        Land = 8,
        Rise = 16,
        Fall = 32,
        FastFall = 64,
        DashHorizontal = 128,
        DashDiagonal = 256,
        WallCling = 512,
        WallSlide = 1024,
        JumpSquat = 2048,
        DashWindup = 4096,
        Grounded = Idle | Run | Crouch | Land,
        Airborne = Rise | Fall | FastFall,
        Walled = WallCling | WallSlide,
        Dashing = DashHorizontal | DashDiagonal,
        CanTurn = Idle | Run | Crouch,
        CanCrouch = Idle | Run
    }
}
