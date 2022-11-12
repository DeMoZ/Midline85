using UnityEngine;

namespace UI
{
    [CreateAssetMenu]
    public class CursorSet : ScriptableObject
    {
        [SerializeField] private Texture2D cursorTexture;

        public void ApplyCursor()
        {
            Cursor.SetCursor(cursorTexture, Vector2.one * cursorTexture.height / 2, CursorMode.ForceSoftware);
            Cursor.lockState = CursorLockMode.None;
            EnableCursor(false);
        }
        
        public void EnableCursor(bool enable)
        {
            Cursor.visible = enable;
        }
    }
}