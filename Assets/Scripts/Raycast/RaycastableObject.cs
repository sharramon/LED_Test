using UnityEngine;

[RequireComponent(typeof(RaycastSelectable))]
public class RaycastableObject : MonoBehaviour
{
    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private GameObject renderObject;

    private RaycastSelectable selectable;
    private Renderer objRenderer;
    private Material runtimeMaterial;
    private bool isSelected = false;

    private void Awake()
    {
        selectable = GetComponent<RaycastSelectable>();
        objRenderer = renderObject.GetComponent<Renderer>();

        // Clone the material instance to avoid shared material changes
        runtimeMaterial = new Material(objRenderer.material);
        objRenderer.material = runtimeMaterial;

        SetColor(baseColor);
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        selectable.onHoverEnter.AddListener(OnHoverEnter);
        selectable.onHoverExit.AddListener(OnHoverExit);
        selectable.onSelect.AddListener(OnSelected);
        selectable.onUnselect.AddListener(OnUnselected);
    }

    private void UnsubscribeEvents()
    {
        selectable.onHoverEnter.RemoveListener(OnHoverEnter);
        selectable.onHoverExit.RemoveListener(OnHoverExit);
        selectable.onSelect.RemoveListener(OnSelected);
        selectable.onUnselect.RemoveListener(OnUnselected);
    }

    private void SetColor(Color color)
    {
        if (runtimeMaterial != null)
            runtimeMaterial.color = color;
    }

    private void OnHoverEnter()
    {
        if (!isSelected)
            SetColor(hoverColor);
    }

    private void OnHoverExit()
    {
        if (!isSelected)
            SetColor(baseColor);
    }

    private void OnSelected()
    {
        isSelected = true;
        SetColor(selectedColor);
    }

    private void OnUnselected()
    {
        isSelected = false;
        SetColor(baseColor);
    }
}
