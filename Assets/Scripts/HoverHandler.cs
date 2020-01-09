using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem {
    public class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool isSelected = false;

        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            if(!pointerEventData.pressPosition.Equals(Vector2.zero) && !pointerEventData.dragging )
            {
                ChangeToggleState();
            }
            
        }

        public void OnPointerExit(PointerEventData pointerEventData)
        {
           
        }

        private void ChangeToggleState()
        {
            if (isSelected)
            {
                GetComponent<Toggle>().isOn = false;
            }
            else
            {
                GetComponent<Toggle>().isOn = true;
            }
            isSelected = !isSelected;
        }
    }

}

