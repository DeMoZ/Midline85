using Sirenix.OdinInspector;
using UnityEngine;

namespace UI
{
    public enum CursorType
    {
        Normal,
        CanClick,
        CanDrag,
        Drag,
    }

    [CreateAssetMenu]
    public class CursorSet : ScriptableObject
    {
        [System.Serializable]
        public class Data
        {
            [HorizontalGroup("Data")] [HideLabel] public Texture2D texture = default;
            [HorizontalGroup("Data")] [HideLabel] public Vector2 hotPoint;
        }

        [SerializeField] private Data normal = default;
        [SerializeField] private Data canClick = default;
        [SerializeField] private Data canDrag = default;
        [SerializeField] private Data drag = default;

        [Space]
        [SerializeField] private Data clickPoint = default;
        
        private Texture2D _currentSprite;

        public Sprite ClickPointSprite => TextureToSprite();
        public Vector2 ClickPointOffset => clickPoint.hotPoint;

        private Sprite TextureToSprite()
        {
            return Sprite.Create(clickPoint.texture, 
                new Rect(0, 0, clickPoint.texture.width, clickPoint.texture.height), new Vector2(0.5f, 0.5f));
        }
        
        private void OnEnable()
        {
            ApplyCursor(CursorType.Normal);
        }
        
        public void ApplyCursor()
        {
            ApplyCursor(CursorType.Normal);
            Cursor.lockState = CursorLockMode.None;
            EnableCursor(false);
        }

        public void ApplyCursor(CursorType type)
        {
            _currentSprite = GetCursorSprite(type);
            var spot = GetSpot(type);
            Cursor.SetCursor(_currentSprite, spot, CursorMode.ForceSoftware);
        }

        private Vector2 GetSpot(CursorType type)
        {
            return type switch
            {
                CursorType.Normal => normal.hotPoint,
                CursorType.CanClick => canClick.hotPoint,
                CursorType.CanDrag => canDrag.hotPoint,
                CursorType.Drag => drag.hotPoint,
                _ => normal.hotPoint,
            };
        }

        private Texture2D GetCursorSprite(CursorType type)
        {
            return type switch
            {
                CursorType.Normal => normal.texture,
                CursorType.CanClick => canClick.texture,
                CursorType.CanDrag => canDrag.texture,
                CursorType.Drag => drag.texture,
                _ => normal.texture
            };
        }

        public void EnableCursor(bool enable)
        {
            Cursor.visible = enable;
        }
    }
}