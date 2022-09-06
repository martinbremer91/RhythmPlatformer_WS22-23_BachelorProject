using System;
using Debug_and_Tools;
using Systems;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class BeatStatusEditor : EditorWindow
    {
        private static BeatStatusEditor s_instance;

        private BeatManager _beatManager => 
            EditorReferenceCollector.s_Instance.BeatManager;
        
        private readonly Rect _infoRect = new(new Vector2(5, 5), new Vector2(400, 400));
        
        private bool _referencesCheck;
        
        #region INITIALIZATION

        [MenuItem("Debug/Beat Status", true)]
        private static bool OpenWindowValidate() => s_instance == null;

        [MenuItem("Debug/Beat Status")]
        private static void OpenWindow()
        {
            BeatStatusEditor window = CreateWindow<BeatStatusEditor>();
            window.titleContent = new GUIContent("Beat Status");
        }

        private void OnEnable()
        {
            if (s_instance == null)
                s_instance = this;
            else
            {
                Close();
                return;
            }
            
            Init();
        }

        private void Init()
        {
            
        }
        
        private bool CheckReferences()
        {
            if (_referencesCheck)
                return true;

            _referencesCheck =
                EditorReferenceCollector.s_Instance != null && _beatManager != null;

            return _referencesCheck;
        }
        
        private void OnDisable() => s_instance = null;

        #endregion
        
        private void OnGUI()
        {
            if (!CheckReferences())
            {
                GUI.Label(new Rect (_infoRect.position + new Vector2(5, 5), new Vector2(300, 20)), 
                    "Start Game to Refresh");
                return;
            }

            try
            {
                Draw();
                Repaint();
            }
            catch
            {
                _referencesCheck = false;
            }
        }

        private void Draw()
        {
            // Draw info box
            GUI.Box(_infoRect, String.Empty);

            for (int i = 1; i <= _beatManager.TrackData.Meter; i++)
            {
                GUI.Toggle(
                    new Rect(
                        _infoRect.position + new Vector2(5 + 50 * (i - 1), 5), 
                        new Vector2(40, 20)), 
                    _beatManager.BeatTracker == i, 
                    i.ToString());
            }

            GUI.Box(new Rect(_infoRect.position + new Vector2(5, 25), new Vector2(150, 20)),
                "Active Source: " + (_beatManager.ActiveSource == 0 ? "A" : "B"));
        }
    }
}
