using Systems;

namespace Gameplay
{
    public class CharacterStateController : GameplayComponent
    {
        #region REFERENCES

        private static CharacterSpriteController spriteController;

        #endregion
        
        #region CHARACTER STATUS

        private static CharacterState _currentCharacterState;
        public static CharacterState CurrentCharacterState
        {
            get => _currentCharacterState;
            private set => SetCharacterState(value);
        }

        public static bool JumpSquat;
        public static bool DashWindup;
        public static bool Dashing;

        private static bool _facingLeft;
        private static bool FacingLeft
        {
            get => _facingLeft;
            set
            {
                if (_facingLeft == value)
                    return;
                
                spriteController.SetCharacterOrientation(value);
                _facingLeft = value;
            }
        }
        
        public static bool Grounded =>
            CurrentCharacterState is CharacterState.Idle or CharacterState.Run or CharacterState.Land
                or CharacterState.Crouch;
        public static bool CanCrouch =>
            CurrentCharacterState is CharacterState.Idle or CharacterState.Run or CharacterState.Land;
        public static bool Airborne => CurrentCharacterState is CharacterState.Rise or CharacterState.Fall;
        public static bool Walled => CurrentCharacterState is CharacterState.WallCling or CharacterState.WallSlide;

        public static bool NearWall_L;
        public static bool NearWall_R;

        #endregion

        private void Start()
        {
            spriteController = ReferenceManager.Instance.CharacterSpriteController;
        }

        public override void OnUpdate()
        {
            // check input and conditions for wall cling (when near wall)
            
            CurrentStateUpdate();
            ExecuteCurrentStateFunctions();
        }

        private static void SetCharacterState(CharacterState value)
        {
            _currentCharacterState = value;
        }

        public void HandleCollisionStateChange(CharacterCollisionChecks.CollisionCheck check, bool enter)
        {
            switch (check)
            {
                case CharacterCollisionChecks.CollisionCheck.Ground:
                    if (!enter)
                        CurrentCharacterState = CharacterMovement.CharacterVelocity.y > 0
                            ? CharacterState.Rise
                            : CharacterState.Fall;
                    else
                    {
                        CurrentCharacterState = CharacterState.Land;
                        CharacterMovement.CancelVerticalVelocity();
                    }
                    break;
                case CharacterCollisionChecks.CollisionCheck.Ceiling:
                    if (enter)
                    {
                        CurrentCharacterState = CharacterState.Fall;
                        CharacterMovement.CancelVerticalVelocity();
                    }
                    break;
                case CharacterCollisionChecks.CollisionCheck.LWall:
                    if (ExitWall())
                        break;
                    if (enter && CharacterMovement.CharacterVelocity.x < 0)
                    {
                        if (CharacterMovement.CharacterVelocity.y != 0)
                            CurrentCharacterState = CharacterState.WallSlide;
                        else if (CharacterInput.InputState.DirectionalInput.x < -.5f)
                            CurrentCharacterState = CharacterState.WallCling;
                        
                        CharacterMovement.CancelHorizontalVelocity();
                    }
                    break;
                case CharacterCollisionChecks.CollisionCheck.RWall:
                    if (ExitWall())
                        break;
                    if (enter && CharacterMovement.CharacterVelocity.x > 0)
                    {
                        if (CharacterMovement.CharacterVelocity.y != 0)
                            CurrentCharacterState = CharacterState.WallSlide;
                        else if (CharacterInput.InputState.DirectionalInput.x > .5f)
                            CurrentCharacterState = CharacterState.WallCling;
                        
                        CharacterMovement.CancelHorizontalVelocity();
                    }
                    break;
            }

            bool ExitWall()
            {
                if (enter || Grounded) 
                    return false;
                
                CurrentCharacterState = CharacterMovement.CharacterVelocity.y <= 0
                    ? CharacterState.Fall
                    : CharacterState.Rise;
                
                return true;
            }
        }
        
        private void CurrentStateUpdate()
        {
            
        }

        private void ExecuteCurrentStateFunctions()
        {
            
        }
    }
    
    public enum CharacterState
    {
        Idle = 0,
        Run = 1,
        Land = 2,
        Crouch = 3,
        Rise = 4,
        Fall = 5,
        WallCling = 6,
        WallSlide = 7
        // Grounded = Idle | Run | Crouch | Land,
        // Airborne = Rise | Fall,
        // Walled = WallCling | WallSlide,
        // CanCrouch = Idle | Run
    }
}
