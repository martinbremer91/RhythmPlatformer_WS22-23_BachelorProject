using Systems;

namespace Gameplay
{
    public class CharacterStateController : GameplayComponent
    {
        #region REFERENCES

        private static CharacterSpriteController spriteController;

        #endregion
        
        #region VARIABLES

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
        
        private static bool canTurn => CharacterState.CanTurn.HasFlag(CurrentCharacterState);
        private static bool canCrouch => CharacterState.CanCrouch.HasFlag(CurrentCharacterState);

        #endregion

        private void Start()
        {
            spriteController = ReferenceManager.Instance.CharacterSpriteController;
        }

        public override void OnUpdate()
        {
            CurrentStateUpdate();
            ExecuteCurrentStateFunctions();
        }
        
        private static void SetCharacterState(CharacterState value) => _currentCharacterState = value;

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
                        CurrentCharacterState = CharacterState.Land;
                    break;
                case CharacterCollisionChecks.CollisionCheck.Ceiling:
                    break;
                case CharacterCollisionChecks.CollisionCheck.LWall:
                    
                    break;
                case CharacterCollisionChecks.CollisionCheck.RWall:
                    break;
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
        Crouch = 2,
        Land = 3,
        Rise = 4,
        Fall = 5,
        WallCling = 6,
        WallSlide = 7,
        Grounded = Idle | Run | Crouch | Land,
        Airborne = Rise | Fall,
        Walled = WallCling | WallSlide,
        Static = Idle | Crouch | Land | WallCling,
        CanTurn = Idle | Run | Crouch,
        CanCrouch = Idle | Run
    }
}
