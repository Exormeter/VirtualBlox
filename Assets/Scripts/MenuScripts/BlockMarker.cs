using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{
    public class BlockMarker : MonoBehaviour
    {
        public MenuManager MenuManager;
        public SteamVR_Input_Sources handInput;
        public SteamVR_Action_Vector2 touchPadPosition;
        public SteamVR_Action_Boolean touchPadButton;
        public BoxDrawer BoxDrawer;


        private bool startedPulling;
        private Vector3 startPullPosition;


       
        void Start()
        {

        }

        
        void Update()
        {
            if(BoxDrawer.CurrentlyUsed == null || BoxDrawer.CurrentlyUsed == this)
            {

                if (startedPulling)
                {
                    BoxDrawer.DrawCube(startPullPosition, gameObject.transform.position);
                }

                else if (MenuManager.CurrentMenuState == MenuState.BOTH_CLOSED && touchPadButton.GetStateDown(handInput))
                {

                    Debug.Log("Start Pulling");
                    startPullPosition = gameObject.transform.position;
                    startedPulling = true;
                    MenuManager.CurrentMenuState = MenuState.DONT_OPEN;
                    BoxDrawer.CurrentlyUsed = this;
                }

                if (MenuManager.CurrentMenuState == MenuState.BOTH_CLOSED && touchPadButton.GetStateUp(handInput))
                {
                    Debug.Log("Stop Pulling");
                    startedPulling = false;
                    MenuManager.CurrentMenuState = MenuState.BOTH_CLOSED;
                }

            }
            
        }
    }
}
