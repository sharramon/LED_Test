using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class RaycastSelectable : MonoBehaviour
{
    public UnityEvent onHoverEnter;
    public UnityEvent onHoverExit;
    public UnityEvent onSelect;
    public UnityEvent onUnselect;

    private bool _isHovered = false;
    private bool _isSelected = false;

    public void OnHoverEnter()
    {
        if (!_isHovered)
        {
            _isHovered = true;
            onHoverEnter.Invoke();
        }
    }

    public void OnHoverExit()
    {
        if (_isHovered)
        {
            _isHovered = false;
            onHoverExit.Invoke();
        }
    }

    public void OnSelect()
    {
        if (!_isSelected)
        {
            _isHovered = false;
            _isSelected = true;
            onSelect.Invoke();
        }
    }

    public void OnUnselect()
    {
        if (_isSelected)
        {
            _isSelected = false;
            onUnselect.Invoke();
        }
    }
}