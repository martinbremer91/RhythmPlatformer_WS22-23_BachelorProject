using UnityEngine;

namespace Debug_and_Tools
{
    public class CameraBoundsTool : MonoBehaviour
    {
        [SerializeField] private TextAsset _currentLevelCameraBoundsJson;

        #region INSPECTOR BUTTONS

        public void SaveGameObjectPositionsAsPoints()
        {
            
            
            // check if _currentLevelCameraBoundsJson exists. If so, overwrite. If not, create and assign field
        }

        public void CreateGameObjectsFromPoints()
        {
            if (_currentLevelCameraBoundsJson == null)
            {
                Debug.LogWarning("Pass in JSON to generate Camera Bounds GameObjects");
                return;
            }
            
            
        }

        #endregion
    }
}
