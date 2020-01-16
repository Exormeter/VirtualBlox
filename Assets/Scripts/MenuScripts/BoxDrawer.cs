using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem {
    public class BoxDrawer : MonoBehaviour
    {

        private bool startedPulling = false; 

        //Obere Plain
        private Vector3 pointATop;
        private Vector3 pointBTop;
        private Vector3 pointCTop;
        private Vector3 pointDTop;

        //Untere Plain
        private Vector3 pointABottom;
        private Vector3 pointBBottom;
        private Vector3 pointCBottom;
        private Vector3 pointDBottom;

        private List<LineRenderer> lineRendererList = new List<LineRenderer>();

        public Material LineMaterial;

        [HideInInspector]
        public BlockMarker CurrentlyUsed = null;


        void Start()
        {
            for(int i = 0; i < 12; i++)
            {
                GameObject lineRendererContainer = new GameObject("LineRenderContainer");
                lineRendererContainer.transform.SetParent(transform);
                LineRenderer lineRenderer = lineRendererContainer.AddComponent<LineRenderer>();
                lineRendererList.Add(lineRenderer);
                lineRenderer.enabled = false;
            }
        }


        public void DrawCube(Vector3 startPosition, Vector3 endPosition)
        {

            Debug.Log("Pulling");
            SetCubePoints(startPosition, endPosition);
            DrawLine(pointATop, pointBTop, lineRendererList[0]);
            DrawLine(pointBTop, pointCTop, lineRendererList[1]);
            DrawLine(pointCTop, pointDTop, lineRendererList[2]);
            DrawLine(pointDTop, pointATop, lineRendererList[3]);

            DrawLine(pointABottom, pointBBottom, lineRendererList[4]);
            DrawLine(pointBBottom, pointCBottom, lineRendererList[5]);
            DrawLine(pointCBottom, pointDBottom, lineRendererList[6]);
            DrawLine(pointDBottom, pointABottom, lineRendererList[7]);

            DrawLine(pointATop, pointABottom, lineRendererList[8]);
            DrawLine(pointBTop, pointBBottom, lineRendererList[9]);
            DrawLine(pointCTop, pointCBottom, lineRendererList[10]);
            DrawLine(pointDTop, pointDBottom, lineRendererList[11]);
        }

        


        public void SetCubePoints(Vector3 startPosition, Vector3 endPosition)
        {
            pointATop = startPosition;
            pointCBottom = endPosition;

            //Top Plain Points
            pointBTop.x = pointCBottom.x;
            pointCTop.x = pointCBottom.x;
            pointDTop.x = pointATop.x;

            pointBTop.y = pointATop.y;
            pointCTop.y = pointATop.y;
            pointDTop.y = pointATop.y;

            pointBTop.z = pointATop.z;
            pointCTop.z = pointCBottom.z;
            pointDTop.z = pointCBottom.z;

            //Bottom Plain Points
            pointABottom.x = pointATop.x;
            pointBBottom.x = pointCBottom.x;
            pointDBottom.x = pointATop.x;

            pointABottom.y = pointCBottom.y;
            pointBBottom.y = pointCBottom.y;
            pointDBottom.y = pointCBottom.y;

            pointABottom.z = pointATop.z;
            pointBBottom.z = pointATop.z;
            pointDBottom.z = pointCBottom.z;
        }

        public void DrawLine(Vector3 startPoint, Vector3 endPoint, LineRenderer lineRenderer)
        {
            lineRenderer.material = LineMaterial;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
            lineRenderer.enabled = true;
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
        }

        public void RemoveBox()
        {
            foreach(LineRenderer lineRenderer in lineRendererList)
            {
                lineRenderer.enabled = false;
            }
        }
    }
}