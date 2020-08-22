using UnityEngine;
using System.Collections;
using LDraw;
using System.Collections.Generic;
using Valve.VR.InteractionSystem;
using System.Linq;
using System;

namespace LDraw
{
    public class LDrawBlockSpecificConnection: LDrawAbstractConnectionPoint
    {
        public List<Vector3> TapPoints = new List<Vector3>();
        public List<Vector3> GroovePoints = new List<Vector3>();

        public bool DeleteAutomaticGeneratedGrooves = false;
        public LDrawBlockSpecificConnection(ConnectionPoint jsonConnectionPoint) : base(null)
        {
            Name = jsonConnectionPoint.name;
            DeleteAutomaticGeneratedGrooves = jsonConnectionPoint.deleteGrooves;
            foreach (SerializableVector3 serialVector3 in jsonConnectionPoint.Taps)
            {
                TapPoints.Add(serialVector3);
            }

            foreach (SerializableVector3 serialVector3 in jsonConnectionPoint.Grooves)
            {
                GroovePoints.Add(serialVector3);
            }
        }


        public override void GenerateColliderPositions(GameObject block)
        {
            GameObject tapFace = block.transform.Find("Taps").gameObject;

            GameObject grooveFace = block.transform.Find("Grooves").gameObject;

            foreach (Vector3 tapPosition in TapPoints)
            {
                AddTapCollider(tapFace, tapPosition, Vector3.up, new Vector3());
            }

            if (DeleteAutomaticGeneratedGrooves)
            {
                for (int i = grooveFace.transform.childCount - 1; i >= 0; i--)
                {
                    GameObject.Destroy(grooveFace.transform.GetChild(i).gameObject);
                }
            }

            foreach (Vector3 groovePosition in GroovePoints)
            {
                AddGroveCollider(grooveFace, groovePosition, Vector3.down, new Vector3());
            }
        }

        protected void AddGroveCollider(GameObject brickFace, Vector3 position, Vector3 lookAt, Vector3 offset)
        {

            GameObject newConnectionPoint = new GameObject("Collider");

            BlockGeometryScript.AddBoxCollider(new Vector3(0.1f, 0.07f, 0.1f), new Vector3(0, 0, 0), true, newConnectionPoint);
            newConnectionPoint.transform.SetParent(brickFace.transform);
            newConnectionPoint.transform.LookAt(lookAt);
            newConnectionPoint.transform.Rotate(90, 0, 0);
            newConnectionPoint.transform.position = position;
            newConnectionPoint.transform.position += offset;

            for (int i = 0; i < brickFace.transform.childCount; i++)
            {
                GameObject siblingConnectionPoint = brickFace.transform.GetChild(i).gameObject;
                if (siblingConnectionPoint.transform.position == newConnectionPoint.transform.position &&
                    siblingConnectionPoint.GetHashCode() != newConnectionPoint.GetHashCode())
                {
                    UnityEngine.Object.DestroyImmediate(newConnectionPoint);
                    return;
                }
            }

            newConnectionPoint.AddComponent<GrooveCollider>();
        }

        protected void AddTapCollider(GameObject brickFace, Vector3 position, Vector3 lookAt, Vector3 offset)
        {
            GameObject newConnectionPoint = new GameObject("Collider");
            newConnectionPoint.AddComponent<TapCollider>();

            BlockGeometryScript.AddBoxCollider(new Vector3(0.1f, 0.07f, 0.1f), new Vector3(0, 0.01f, 0), true, newConnectionPoint);
            newConnectionPoint.transform.SetParent(brickFace.transform);
            newConnectionPoint.transform.LookAt(lookAt);
            newConnectionPoint.transform.Rotate(90, 0, 0);
            newConnectionPoint.transform.position = position;
        }
    }
}