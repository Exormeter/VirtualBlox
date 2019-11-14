using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem {
    public class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool isSelected = false;
        public void OnPointerEnter(PointerEventData pointerEventData)
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
           
            
            Debug.Log("Cursor Entering " + name + " GameObject");
        }

        //Detect when Cursor leaves the GameObject
        public void OnPointerExit(PointerEventData pointerEventData)
        {
            //Output the following message with the GameObject's name
            Debug.Log("Cursor Exiting " + name + " GameObject");
        }
    }

}

