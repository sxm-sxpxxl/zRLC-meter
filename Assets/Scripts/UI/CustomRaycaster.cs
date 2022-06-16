using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image)), DisallowMultipleComponent]
public sealed class CustomRaycaster : MonoBehaviour, IPointerClickHandler
{
    public event Action<PointerEventData> OnPointerClick = delegate { };

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        OnPointerClick.Invoke(eventData);
    }
}
