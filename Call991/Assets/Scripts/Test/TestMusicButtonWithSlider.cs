using UnityEngine;
using UnityEngine.UI;

public class TestMusicButtonWithSlider : MonoBehaviour
{
    [SerializeField] private Button button = default;
    [SerializeField] private Slider slider = default;

    public Button Button => button;
    public Slider Slider => slider;
}
