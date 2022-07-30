using System.Threading.Tasks;
using Systems;
using UnityEngine;

namespace Gameplay
{
    public class CharacterStateController : GameplayComponent
    {
        #region REFERENCES

        private static CharacterSpriteController spriteController;
        [SerializeField] private CharacterMovement characterMovement;

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
        public static bool FacingLeft
        {
            get => _facingLeft;
            private set
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

        private const float runTurnWindow = .1f;
        
        public static bool NearWall_L;
        public static bool NearWall_R;

        //private static bool canWallCling = true;

        // private float wallClingMaxDuration;
        // private static float wallClingTimer;
        
        #endregion

        private void Start()
        {
            spriteController = ReferenceManager.Instance.CharacterSpriteController;
            // wallClingMaxDuration = ReferenceManager.Instance.MovementConfigs.WallClingMaxDuration;
        }

        public override void OnUpdate()
        { 
            HandleInputStateChange();
            ApplyStateMovement();
        }

        private static void SetCharacterState(CharacterState value)
        {
            if (_currentCharacterState == value)
                return;
            
            ChangeIntoState(value);
            ChangeOutOfState();
            
            _currentCharacterState = value;
        }

        private static void ChangeIntoState(CharacterState value)
        {
            switch (value)
            {
                case CharacterState.Run:
                    CheckFacingOrientation();
                    break;
                case CharacterState.Fall:
                    CharacterMovement.FallVelocity = new(CharacterMovement.CharacterVelocity.x, 0);
                    break;
            }
        }

        private static void ChangeOutOfState()
        {
            switch (_currentCharacterState)
            {
                case CharacterState.Run:
                    ResetRunCurveTrackerAsync();
                    CharacterMovement.RunVelocity = 0;
                    break;
            }
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
                    NearWall_L = enter;
                    if (enter && CharacterMovement.CharacterVelocity.x < 0)
                    {
                        SetWalledState(false);
                        CharacterMovement.CancelHorizontalVelocity();
                    }
                    break;
                case CharacterCollisionChecks.CollisionCheck.RWall:
                    NearWall_R = enter;
                    if (enter && CharacterMovement.CharacterVelocity.x > 0)
                    {
                        SetWalledState(true);
                        CharacterMovement.CancelHorizontalVelocity();
                    }
                    break;
            }
        }

        private void HandleInputStateChange()
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Idle:
                    HandleIdle();
                    break;
                case CharacterState.Run:
                    if (CharacterInput.InputState.DirectionalInput.y > -.5f)
                        CurrentCharacterState =
                            Mathf.Abs(CharacterInput.InputState.DirectionalInput.x) > .5f
                                ? CharacterState.Run
                                : CharacterState.Idle;
                    else
                        CurrentCharacterState = CharacterState.Crouch;
                    break;
                case CharacterState.Crouch:
                    if (CharacterInput.InputState.DirectionalInput.y > -.5f)
                        CurrentCharacterState = 
                            Mathf.Abs(CharacterInput.InputState.DirectionalInput.x) > .5f ? 
                                CharacterState.Run : CharacterState.Idle;
                    break;
                case CharacterState.Land:
                    if (CharacterMovement.CharacterVelocity.x == 0)
                        CurrentCharacterState = 
                            CharacterInput.InputState.DirectionalInput.y <= -.5f ? 
                                CharacterState.Crouch : CharacterState.Idle;
                    break;
                case CharacterState.Rise:
                    break;
                case CharacterState.Fall:
                    break;
                case CharacterState.WallCling:
                    break;
                case CharacterState.WallSlide:
                    break;
            }
            
            if (NearWall_L || NearWall_R)
                NearWallChecks();

            void HandleIdle()
            {
                if (CharacterInput.InputState.DirectionalInput.y < -.5f)
                {
                    CurrentCharacterState = CharacterState.Crouch;
                    return;
                }

                if (Mathf.Abs(CharacterInput.InputState.DirectionalInput.x) > .5f)
                {
                    CurrentCharacterState = CharacterState.Run;
                    return;
                }
                
                CheckFacingOrientation();
            }
            
            void NearWallChecks()
            {
                bool holdTowardsWall_L = NearWall_L && CharacterInput.InputState.DirectionalInput.x < -.5f;
                bool holdTowardsWall_R = NearWall_R && CharacterInput.InputState.DirectionalInput.x > .5f;
            
                if (Airborne && holdTowardsWall_L)
                    SetWalledState(false);
                if (Airborne && holdTowardsWall_R)
                    SetWalledState(true);
            
                if (Walled && CharacterMovement.CharacterVelocity.y <= 0 && !holdTowardsWall_L && !holdTowardsWall_R)
                    CurrentCharacterState = CharacterState.Fall;
            }
        }
        
        private void ApplyStateMovement()
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Idle:
                    break;
                case CharacterState.Run:
                    Vector2 runTracker = CharacterMovement.RunCurveTracker;
                    if (runTracker.x < runTracker.y || CharacterMovement.RunVelocity == 0)
                    {
                        CharacterMovement.RunCurveTracker.x += Time.deltaTime;
                        characterMovement.Run();
                    }
                    break;
                case CharacterState.Crouch:
                    break;
                case CharacterState.Land:
                    // temp
                    CharacterMovement.CancelHorizontalVelocity();
                    break;
                case CharacterState.Rise:
                    // drag
                    //drift
                    break;
                case CharacterState.Fall:
                    Vector2 fallTracker = CharacterMovement.FallCurveTracker;
                    if (fallTracker.x < fallTracker.y)
                    {
                        CharacterMovement.FallCurveTracker.x += Time.deltaTime;
                        characterMovement.Fall();
                    }
                    //drift
                    break;
                case CharacterState.WallCling:
                    //wallClingTimer += Time.deltaTime;
                    break;
                case CharacterState.WallSlide:
                    break;
            }
        }

        private static void CheckFacingOrientation()
        {
            if (FacingLeft && CharacterInput.InputState.DirectionalInput.x > .1f)
                FacingLeft = false;
            if (!FacingLeft && CharacterInput.InputState.DirectionalInput.x < -.1f)
                FacingLeft = true;
        }

        /// <summary>
        /// Async function to create time-frame for reversing run direction without losing speed ("dash dancing")
        /// </summary>
        private static async void ResetRunCurveTrackerAsync()
        {
            float runTimer = runTurnWindow;

            while (runTimer > 0)
            {
                await Task.Yield();
                
                runTimer -= Time.deltaTime;
                if (CurrentCharacterState == CharacterState.Run)
                    return;
            }
            
            CharacterMovement.RunCurveTracker.x = 0;
        }

        /// <summary>
        /// Checks y velocity and input direction. Sets state to WallCling, WallSlide, or does nothing.
        /// </summary>
        /// <param name="rightWall"></param>
        private void SetWalledState(bool rightWall)
        {
            if (CharacterMovement.CharacterVelocity.y != 0)
                CurrentCharacterState = CharacterState.WallSlide;
            else if (rightWall && CharacterInput.InputState.DirectionalInput.x > .5f || 
                     !rightWall && CharacterInput.InputState.DirectionalInput.x < -.5f)
                CurrentCharacterState = CharacterState.WallCling;
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
    }
}
