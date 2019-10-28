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
    private PointerEventData pointDdata = null;
    // Start is called before the first frame update

    protected override void Awake()
    {
        base.Awake();

        pointDdata = new PointerEventData(eventSystem);
    }

    // Update is called once per frame
    public override void Process()
    {
        pointDdata.Reset();
        pointDdata.position = new Vector2(cameraRay.pixelWidth / 2, cameraRay.pixelHeight / 2);

        eventSystem.RaycastAll(pointDdata, m_RaycastResultCache);
        pointDdata.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        currentObject = pointDdata.pointerCurrentRaycast.gameObject;

        m_RaycastResultCache.Clear();

        HandlePointerExitAndEnter(pointDdata, currentObject);

        if (clickAction.GetStateDown(targetSource))
        {
            ProcessPress(pointDdata);
        }

        if (clickAction.GetStateUp(targetSource))
        {
            ProcessRelease(pointDdata);
        }
    }

    public PointerEventData GetData()
    {
        return pointDdata;
    }

    private void ProcessPress(PointerEventData data)
    {
        data.pointerCurrentRaycast = data.pointerCurrentRaycast;

        GameObject newPointerPress = ExecuteEvents.ExecuteHierarchy(currentObject, data, ExecuteEvents.pointerDownHandler);

        if(newPointerPress == null)
        {
            newPointerPress = 
        }
    }

    private void ProcessRelease(PointerEventData data)
    {

    }
}
