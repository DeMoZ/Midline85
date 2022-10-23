using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class KeyboardTextLanguageSwitch : MonoBehaviour
{
    public UnityEvent _left;
    public UnityEvent _right;

    private bool _isSelected;

    void Update()
    {
        if (!Input.anyKeyDown)
            return;

        // Debug.LogWarning(EventSystem.current.currentSelectedGameObject);

        if (EventSystem.current.currentSelectedGameObject != gameObject)
            return;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            Debug.LogWarning("Left");
            _left?.Invoke();
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            Debug.LogWarning("Right");
            _right?.Invoke();
        }
    }
}