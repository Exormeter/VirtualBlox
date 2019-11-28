using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class VRInputModule : BaseInputModule
{

    public Camera cameraRay;
    public SteamVR_Input_Sources targetSource;
    public SteamVR_Action_Boolean clickAction;

    private GameObject currentObject = null;
    private PointerEventData pointData = null;
    

    protected override void Awake()
    {
        base.Awake();

        pointData = new PointerEventData(eventSystem);
    }

    public override void Process()
    {
        pointData.Reset();
        pointData.position = new Vector2(cameraRay.pixelWidth / 2, cameraRay.pixelHeight / 2);

        eventSystem.RaycastAll(pointData, m_RaycastResultCache);
        pointData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        currentObject = pointData.pointerCurrentRaycast.gameObject;

        m_RaycastResultCache.Clear();

        HandlePointerExitAndEnter(pointData, currentObject);

        if (clickAction.GetStateDown(targetSource))
        {
            ProcessPress(pointData);
        }

        if (clickAction.GetStateUp(targetSource))
        {
            ProcessRelease(pointData);
        }
    }

    public PointerEventData GetData()
    {
        return pointData;
    }

    private void ProcessPress(PointerEventData data)
    {
        data.pointerPressRaycast = data.pointerCurrentRaycast;

        GameObject newPointerPress = ExecuteEvents.ExecuteHierarchy(currentObject, data, ExecuteEvents.pointerDownHandler);

        if (newPointerPress == null)
        {
            newPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject);
        }

        data.pressPosition = data.position;
        data.pointerPress = newPointerPress;
        data.rawPointerPress = currentObject;
    }

    private void ProcessRelease(PointerEventData data)
    {
        ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);

        GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject);

        if(data.pointerPress == pointerUpHandler)
        {
            ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerClickHandler);
        }

        eventSystem.SetSelectedGameObject(null);

        data.pressPosition = Vector2.zero;
        data.pointerPress = null;
        data.rawPointerPress = null;
    }
}
