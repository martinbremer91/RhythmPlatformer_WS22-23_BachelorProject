using System;
using Gameplay;
using UnityEngine;
using UnityEditor;

namespace Editor
{
    public class CharacterStatusEditor : EditorWindow
    {
        private static CharacterStatusEditor Instance;

        private Texture2D unitCircleTexture;

        private readonly Rect infoRect = new(new Vector2(5, 5), new Vector2(400, 400));

        #region INITIALIZATION

        [MenuItem("Debug/Character Status", true)]
        private static bool OpenWindowValidate() => Instance == null;

        [MenuItem("Debug/Character Status")]
        private static void OpenWindow()
        {
            CharacterStatusEditor window = CreateWindow<CharacterStatusEditor>();
            window.titleContent = new GUIContent("Character Status");
        }

        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Close();
                return;
            }
            
            LoadResources();
        }

        private void LoadResources()
        {
            unitCircleTexture = 
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SCRIPTS/Editor/Editor Resources/unit_circle.png");
        }
        
        private void OnDisable() => Instance = null;

        #endregion

        private void OnGUI()
        {
            Draw();
            Repaint();
        }

        private void Draw()
        {
            // Draw info box
            GUI.Box(infoRect, String.Empty);
            
            // Draw Character State
            GUI.Label(new Rect (infoRect.position + new Vector2(5, 5), new Vector2(300, 20)), 
                "Character State: " + 
                Enum.GetName(typeof(CharacterState), CharacterStateController.CurrentCharacterState));
            
            // Draw Velocities
            GUI.Label(new Rect (infoRect.position + new Vector2(5, 25), new Vector2(300, 20)), 
                "Character Velocity: " + CharacterMovement.CharacterVelocity);
            GUI.Label(new Rect (infoRect.position + new Vector2(25, 40), new Vector2(300, 20)), 
                "Run Velocity: " + CharacterMovement.RunVelocity);
            GUI.Label(new Rect (infoRect.position + new Vector2(25, 55), new Vector2(300, 20)), 
                "Fall Velocity: " + CharacterMovement.FallVelocity);
            GUI.Label(new Rect (infoRect.position + new Vector2(25, 70), new Vector2(300, 20)), 
                "Land Velocity: " + CharacterMovement.LandVelocity);
            
            // DrawDirectionalInputCircle();
        }

        private void DrawDirectionalInputCircle()
        {
            // GUI.DrawTexture(new Rect(new Vector2(50, 50), new Vector2(100, 100)), 
            //     unitCircleTexture);
        }
    }
}
