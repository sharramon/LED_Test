// SlideOnPoke.cs
using UnityEngine;
using TMPro;
using Oculus.Interaction;
using System.Linq;
using UnityEngine.Events;

[RequireComponent(typeof(PokeInteractable))]
[RequireComponent(typeof(InteractableUnityEventWrapper))]
public class SlideOnPoke : MonoBehaviour
{
    [SerializeField] private float minZ = -0.5f;
    [SerializeField] private float maxZ =  0.5f;
    [SerializeField] private bool invertNormalized = false;
    [SerializeField] private TMP_Text slideValue;
    
    public UnityEvent<float> onSliderValueChanged;

    /// <summary>
    /// Normalized slide position between 0 (at minZ) and 1 (at maxZ), or reversed if invertNormalized is true.
    /// </summary>
    public float NormalizedSlide { get; private set; }

    PokeInteractable _pokeTarget;
    InteractableUnityEventWrapper _eventWrapper;
    bool _isSelected;

    void Awake()
    {
        _pokeTarget   = GetComponent<PokeInteractable>();
        _eventWrapper = GetComponent<InteractableUnityEventWrapper>();
        _eventWrapper.WhenSelect  .AddListener(() => _isSelected = true);
        _eventWrapper.WhenUnselect.AddListener(() => _isSelected = false);
    }

    void Update()
    {
        if (_isSelected)
        {
            NormalizedSlide = SlideAlongZ();
            onSliderValueChanged?.Invoke(NormalizedSlide);
            slideValue.text = NormalizedSlide.ToString();
        }
    }

    float SlideAlongZ()
    {
        var poke         = _pokeTarget.Interactors.First();
        Vector3 worldPos = poke.TouchPoint;
        Vector3 localPos = transform.parent.InverseTransformPoint(worldPos);

        float clampedZ = Mathf.Clamp(localPos.z, minZ, maxZ);
        transform.localPosition = new Vector3(0f, 0f, clampedZ);

        if (Mathf.Approximately(minZ, maxZ))
            return invertNormalized ? 1f : 0f;

        float t = Mathf.InverseLerp(minZ, maxZ, clampedZ);
        float value = invertNormalized ? 1f - t : t;
        return value;
    }
}