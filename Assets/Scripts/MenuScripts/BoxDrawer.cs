using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem {
    public class BoxDrawer : MonoBehaviour
    {
        //Obere Plain
        private GameObject pointATop;
        private GameObject pointBTop;
        private GameObject pointCTop;
        private GameObject pointDTop;

        //Untere Plain
        private GameObject pointABottom;
        private GameObject pointBBottom;
        private GameObject pointCBottom;
        private GameObject pointDBottom;

        public Vector3 StartPosition;

        private List<LineRenderer> lineRendererList = new List<LineRenderer>();
        public Material LineMaterial;
        public GameObject ContainerLineRenderer;
        public GameObject ContainerCornerGrabber;
        public GameObject CornerGraber;

        void Start()
        {
            for(int i = 0; i < 12; i++)
            {
                GameObject lineRendererContainer = new GameObject("LineRenderContainer");
                lineRendererContainer.transform.SetParent(transform);
                LineRenderer lineRenderer = lineRendererContainer.AddComponent<LineRenderer>();
                lineRendererList.Add(lineRenderer);
                lineRenderer.enabled = false;
                lineRenderer.transform.SetParent(ContainerLineRenderer.transform);
                ContainerLineRenderer.SetActive(false);
            }

            //Top Corners
            pointATop = Instantiate(CornerGraber, transform);
            pointBTop = Instantiate(CornerGraber, transform);
            pointCTop = Instantiate(CornerGraber, transform);
            pointDTop = Instantiate(CornerGraber, transform);

            //Bottom Corners
            pointABottom = Instantiate(CornerGraber, transform);
            pointBBottom = Instantiate(CornerGraber, transform);
            pointCBottom = Instantiate(CornerGraber, transform);
            pointDBottom = Instantiate(CornerGraber, transform);

            //Sides are named after a six sided die where One is on Top, Six on Bottom and 5 is facing toward you

            //Top Side
            List<GameObject> sideOne = new List<GameObject>()
            {
                pointATop,
                pointBTop,
                pointCTop,
                pointDTop
            };

            //Bottom Side
            List<GameObject> sideSix = new List<GameObject>()
            {
                pointABottom,
                pointBBottom,
                pointCBottom,
                pointDBottom,
            };

            //Left Side
            List<GameObject> sideThree = new List<GameObject>()
            {
                pointATop,
                pointDTop,
                pointDBottom,
                pointABottom,
            };

            //Right Side
            List<GameObject> sideFour = new List<GameObject>()
            {
                pointBTop,
                pointCTop,
                pointBBottom,
                pointCBottom
            };

            //Front Side
            List<GameObject> sideFive = new List<GameObject>()
            {
                pointCTop,
                pointDTop,
                pointCBottom,
                pointDBottom
            };

            //Back Side
            List<GameObject> sideTwo = new List<GameObject>()
            {
                pointATop,
                pointBTop,
                pointABottom,
                pointBBottom
            };

            pointATop.GetComponent<CornerDrag>().SetDependingSides(sideTwo, sideOne, sideThree);
            pointBTop.GetComponent<CornerDrag>().SetDependingSides(sideTwo, sideOne, sideFour);
            pointCTop.GetComponent<CornerDrag>().SetDependingSides(sideFive, sideOne, sideFour);
            pointDTop.GetComponent<CornerDrag>().SetDependingSides(sideFive, sideOne, sideThree);

            pointABottom.GetComponent<CornerDrag>().SetDependingSides(sideTwo, sideSix, sideThree);
            pointBBottom.GetComponent<CornerDrag>().SetDependingSides(sideTwo, sideSix, sideFour);
            pointCBottom.GetComponent<CornerDrag>().SetDependingSides(sideFive, sideSix, sideFour);
            pointDBottom.GetComponent<CornerDrag>().SetDependingSides(sideFive, sideSix, sideThree);

        }


        public void DrawCube(Vector3 endPosition)
        {
            SetCubePoints(StartPosition, endPosition);
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

            //Case 1: Top A to Bottom C      0 0 0
            if(startPosition.x > endPosition.x && startPosition.y > endPosition.y && startPosition.z > endPosition.z)
            {
                StartTopAEndBottomC(startPosition, endPosition);
            }

            //Bottom C to Top A            1 1 1
            else if (startPosition.x < endPosition.x && startPosition.y < endPosition.y && startPosition.z < endPosition.z)
            {
                StartTopAEndBottomC(endPosition, startPosition);
            }

            //Bottom A to Top C           0 1 0
            else if (startPosition.x > endPosition.x && startPosition.y < endPosition.y && startPosition.z > endPosition.z)
            {
                StartTopCEndBottomA(startPosition, endPosition);
            }

            //Case 1: Top C to Bottom A    1 0 1 
            else if (startPosition.x < endPosition.x && startPosition.y > endPosition.y && startPosition.z < endPosition.z)
            {
                StartTopCEndBottomA(endPosition, startPosition);
            }

            //Top B to Bottom D             0 0 1
            else if (startPosition.x > endPosition.x && startPosition.y > endPosition.y && startPosition.z < endPosition.z)
            {
                StartTopBEndBottomD(startPosition, endPosition);
            }

            //D Bottom to B Top            1 1 0
            else if (startPosition.x < endPosition.x && startPosition.y < endPosition.y && startPosition.z > endPosition.z)
            {
                StartTopBEndBottomD(endPosition, startPosition);
            }

            //Top D to Bottom B             1 0 0  
            else if (startPosition.x < endPosition.x && startPosition.y > endPosition.y && startPosition.z > endPosition.z)
            {
                StartTopDEndBottomB(startPosition, endPosition);
            }

            // Bottom B to Top D            0 1 1 
            else if (startPosition.x > endPosition.x && startPosition.y < endPosition.y && startPosition.z < endPosition.z)
            {
                StartTopDEndBottomB(endPosition, startPosition);
            }

        }

        public void StartTopAEndBottomC(Vector3 startPosition, Vector3 endPosition)
        {
            pointATop.transform.position = startPosition;
            pointCBottom.transform.position = endPosition;
            //Top Plain Points

            pointBTop.transform.position = new Vector3(startPosition.x, startPosition.y, endPosition.z);
            pointCTop.transform.position = new Vector3(endPosition.x, startPosition.y, endPosition.z);
            pointDTop.transform.position = new Vector3(endPosition.x, startPosition.y, startPosition.z);

            //Bottom Plain Points

            pointABottom.transform.position = new Vector3(startPosition.x, endPosition.y, startPosition.z);
            pointBBottom.transform.position = new Vector3(startPosition.x, endPosition.y, endPosition.z);
            pointDBottom.transform.position = new Vector3(endPosition.x, endPosition.y, startPosition.z);
        }

        public void StartTopCEndBottomA(Vector3 startPosition, Vector3 endPosition)
        {
            pointCTop.transform.position = startPosition;
            pointABottom.transform.position = endPosition;
            //Top Plain Points

            pointATop.transform.position = new Vector3(endPosition.x, startPosition.y, endPosition.z);
            pointBTop.transform.position = new Vector3(endPosition.x, startPosition.y, startPosition.z);
            pointDTop.transform.position = new Vector3(startPosition.x, startPosition.y, endPosition.z);

            //Bottom Plain Points

            pointBBottom.transform.position = new Vector3(endPosition.x, endPosition.y, startPosition.z);
            pointCBottom.transform.position = new Vector3(startPosition.x, endPosition.y, startPosition.z);
            pointDBottom.transform.position = new Vector3(startPosition.x, endPosition.y, endPosition.z);
        }

        public void StartTopBEndBottomD(Vector3 startPosition, Vector3 endPosition)
        {
            pointBTop.transform.position = startPosition;
            pointDBottom.transform.position = endPosition;

            //Top Plain Points

            pointATop.transform.position = new Vector3(startPosition.x, startPosition.y, endPosition.z);
            pointCTop.transform.position = new Vector3(endPosition.x, startPosition.y, startPosition.z);
            pointDTop.transform.position = new Vector3(endPosition.x, startPosition.y, endPosition.z);

            //Bottom Plain Points

            pointABottom.transform.position = new Vector3(startPosition.x, endPosition.y, endPosition.z);
            pointBBottom.transform.position = new Vector3(startPosition.x, endPosition.y, startPosition.z);
            pointCBottom.transform.position = new Vector3(endPosition.x, endPosition.y, startPosition.z);
        }

        public void StartTopDEndBottomB(Vector3 startPosition, Vector3 endPosition)
        {
            pointDTop.transform.position = startPosition;
            pointBBottom.transform.position = endPosition;

            //Top Plain Points

            pointATop.transform.position = new Vector3(endPosition.x, startPosition.y, startPosition.z);
            pointBTop.transform.position = new Vector3(endPosition.x, startPosition.y, endPosition.z);
            pointCTop.transform.position = new Vector3(startPosition.x, startPosition.y, endPosition.z);

            //Bottom Plain Points

            pointABottom.transform.position = new Vector3(endPosition.x, endPosition.y, startPosition.z);
            pointCBottom.transform.position = new Vector3(startPosition.x, endPosition.y, endPosition.z);
            pointDBottom.transform.position = new Vector3(startPosition.x, endPosition.y, startPosition.z);
        }

        public void EditCube(CornerDrag cornerDrag)
        {
            cornerDrag.UpdateCorners();
            SendMessage("OnUpdateCollider", SendMessageOptions.DontRequireReceiver);
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



        public void DrawLine(GameObject startPoint, GameObject endPoint, LineRenderer lineRenderer)
        {
            lineRenderer.material = LineMaterial;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPoint.transform.position);
            lineRenderer.SetPosition(1, endPoint.transform.position);
            lineRenderer.enabled = true;
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
        }

        public void RemoveBox()
        {
            ContainerLineRenderer.SetActive(false);
            //ContainerCornerGrabber.SetActive(false);
        }

        public void ActivateBox()
        {
            ContainerLineRenderer.SetActive(true);
            //ContainerCornerGrabber.SetActive(true);
        }

        public void SetStartPosition(Vector3 position)
        {
            StartPosition = position;
        }

        public Collider ConfigureBoxCollider(BoxCollider collider)
        {
            Vector3 colliderCenter =  new Vector3();
            
            colliderCenter.x = ((pointATop.transform.position + pointBTop.transform.position) / 2).x;
            colliderCenter.y = ((pointATop.transform.position + pointABottom.transform.position) / 2).y;
            colliderCenter.z = ((pointATop.transform.position + pointDTop.transform.position) / 2).z;
            

            collider.center = transform.InverseTransformPoint(colliderCenter);

            Vector3 colliderBounds = new Vector3();
            colliderBounds.x = Vector3.Distance(pointATop.transform.position, pointBTop.transform.position);
            colliderBounds.y = Vector3.Distance(pointATop.transform.position, pointABottom.transform.position);
            colliderBounds.z = Vector3.Distance(pointATop.transform.position, pointDTop.transform.position);

            collider.size = colliderBounds;
            return collider;
        }
    }
}