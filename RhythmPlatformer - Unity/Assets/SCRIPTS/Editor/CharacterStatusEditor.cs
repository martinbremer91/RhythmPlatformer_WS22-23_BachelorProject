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

            if (GUI.changed) 
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
            
            // DrawDirectionalInputCircle();
        }

        private void DrawDirectionalInputCircle()
        {
            // GUI.DrawTexture(new Rect(new Vector2(50, 50), new Vector2(100, 100)), 
            //     unitCircleTexture);
        }
    }
}
