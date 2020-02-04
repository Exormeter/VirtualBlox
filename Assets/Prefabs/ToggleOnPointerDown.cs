using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleOnPointerDown : Toggle
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        InternalToggle();
    }

    private void InternalToggle()
    {
        if (!IsActive() || !IsInteractable())
            return;

        isOn = !isOn;
    }
}
