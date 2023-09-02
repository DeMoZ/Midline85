using UnityEngine;

[CreateAssetMenu(fileName = nameof(NewspaperInputSo), menuName = "Aa/Configs/" + nameof(NewspaperInputSo), order = 0)]
public class NewspaperInputSo : ScriptableObject
{
    [SerializeField] private float moveSpeedMobile = 5;
    [SerializeField] private float moveSpeedPC = 25;

    [Space] [SerializeField] private float zoomTime = 1f;
    [SerializeField] private float maxZoom = 2.3f;
    
    [Space] [SerializeField] private float scrollSmoothTime = 0.2f;
    [SerializeField] private float scrollSpeed = 700f;

#if UNITY_IOS || UNITY_ANDROID
    public float MoveSpeed => moveSpeedMobile;
#else
    public float MoveSpeed => moveSpeedPC;
#endif

    public float ZoomTime => zoomTime;
    public float MaxZoom => maxZoom;
    public float ScrollSmoothTime => scrollSmoothTime;
    public float ScrollSpeed => scrollSpeed;
}