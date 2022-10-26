using System.Collections;
using System.Collections.Generic;
using GlobalSystems;
using Interfaces_and_Enums;
using UnityEngine;

namespace GlobalSystems
{
    public class DependencyInjector_MainMenu : DependencyInjector
    {
        protected override SceneType GetSceneType() => SceneType.MainMenu;
        
        public override void Init(GameStateManager in_gameStateManager)
        {
            // TODO: dependency injection here
            
            base.Init(in_gameStateManager);
        }
    }
}
