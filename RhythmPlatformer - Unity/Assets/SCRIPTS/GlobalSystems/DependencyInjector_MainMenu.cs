using Interfaces_and_Enums;

namespace GlobalSystems
{
    public class DependencyInjector_MainMenu : DependencyInjector
    {
        protected override SceneType GetSceneType() => SceneType.MainMenu;
        protected override UpdateType GetUpdateType() => UpdateType.MenuTransition;
        
        public override void Init(GameStateManager in_gameStateManager)
        {
            // TODO: dependency injection here
            
            base.Init(in_gameStateManager);
        }
    }
}
