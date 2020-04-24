using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace LDraw
{
    public class LDrawModelConverter
    {

        public void ConvertLDrawModel(LDrawModel lDrawModel)
        {
            GameObject lDrawGameObject = lDrawModel.CreateMeshGameObject(LDrawConfig.Instance.ScaleMatrix);
            GameObject newBlock = CombineTileMeshes(lDrawGameObject);

            LDrawConnectionFactory.FlushFactory();
            //lDrawGameObject.transform.LocalReflect(Vector3.up);
            //newBlock.transform.LocalReflect(Vector3.up);

            GameObject block = new GameObject("NormalBlock");
            
            block.AddComponent<Rigidbody>();
            block.AddComponent<BlockRotator>();
            block.AddComponent<AttachFloorHandler>();
            block.AddComponent<AttachHandHandler>().Debug = true;
            block.AddComponent<BlockCommunication>();

            AddTapFaces(newBlock, lDrawModel._ConnectionPoints);
            AddGrooveFaces(newBlock, lDrawModel._ConnectionPoints);
            AddGrooveFacesBoxedConnections(newBlock, lDrawModel._ConnectionPoints);

            newBlock.transform.Rotate(new Vector3(0, 0, -180));
            newBlock.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            newBlock.transform.SetParent(block.transform);

            for(int i = 0; i < newBlock.transform.childCount; i++)
            {
                newBlock.transform.GetChild(i).SetParent(block.transform);
            }
        }


        private GameObject CombineTileMeshes(GameObject container)
        {
            MeshFilter[] meshFilters = container.GetComponentsInChildren<MeshFilter>();

            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            GameObject combinedBlock = new GameObject("Block");
            combinedBlock.AddComponent(typeof(MeshFilter));
            combinedBlock.AddComponent(typeof(MeshRenderer));
            combinedBlock.GetComponent<MeshFilter>().mesh = new Mesh();

            //Edit mode
            combinedBlock.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);

            //Mesh mesh = new Mesh();
            //mesh.CombineMeshes(combine);

            //float quality = 0.1f;
            //var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
            //meshSimplifier.Initialize(mesh);
            //meshSimplifier.SimplifyMesh(quality);
            //combinedBlock.GetComponent<MeshFilter>().mesh = meshSimplifier.ToMesh();




            //Destroy(container);
            return combinedBlock;

        }

        private void AddTapFaces(GameObject newBlock, List<LDrawAbstractConnectionPoint> connectionPoints)
        {
            
            List<LDrawAbstractConnectionPoint> tapConnectionPoints = connectionPoints.FindAll(connectionPoint => connectionPoint is LDrawAbstractTapConnection);

            if (tapConnectionPoints.Count == 0)
            {
                return;
            }

            List<Plane> brickPlanes = new List<Plane>();
            foreach (LDrawAbstractConnectionPoint connectionPoint in tapConnectionPoints)
            {
                if (!brickPlanes.Exists(plane => Math.Abs(plane.GetDistanceToPoint(connectionPoint.ConnectorPosition.transform.position)) < 0.001))
                {
                    brickPlanes.Add(new Plane(-connectionPoint.ConnectorPosition.transform.up, connectionPoint.ConnectorPosition.transform.position));
                }
            }

            foreach (Plane plane in brickPlanes)
            {
                GameObject brickFace = new GameObject("TapFace");

                brickFace.transform.SetParent(newBlock.transform);

                foreach (LDrawAbstractConnectionPoint connectionPoint in tapConnectionPoints)
                {
                    if (Math.Abs(plane.GetDistanceToPoint(connectionPoint.ConnectorPosition.transform.position)) < 0.0001)
                    {
                        connectionPoint.GenerateColliderPositions(brickFace);
                    }
                }
                brickFace.AddComponent<TapHandler>();
            }
        }

        private void AddGrooveFaces(GameObject newBlock, List<LDrawAbstractConnectionPoint> connectionPoints)
        {
            List<LDrawAbstractConnectionPoint> grooveConnectionPoints = connectionPoints.FindAll(connectionPoint => connectionPoint is LDrawAbstractGrooveConnection);

            if(grooveConnectionPoints.Count == 0)
            {
                return;
            }

            List<Plane> brickPlanes = new List<Plane>();
            foreach (LDrawAbstractConnectionPoint connectionPoint in grooveConnectionPoints)
            {

                if (!brickPlanes.Exists(plane =>Math.Abs(plane.GetDistanceToPoint(connectionPoint.ConnectorPosition.transform.GetChild(0).position)) < 0.001))
                {
                    brickPlanes.Add(new Plane(-connectionPoint.ConnectorPosition.transform.up, connectionPoint.ConnectorPosition.transform.GetChild(0).position));
                }
            }

            foreach (Plane plane in brickPlanes)
            {
                GameObject brickFace = new GameObject("GrooveFace");

                brickFace.transform.SetParent(newBlock.transform);

                foreach (LDrawAbstractConnectionPoint connectionPoint in grooveConnectionPoints)
                {
                    
                    if (Math.Abs(plane.GetDistanceToPoint(connectionPoint.ConnectorPosition.transform.GetChild(0).position)) < 0.0001)
                    {
                        connectionPoint.GenerateColliderPositions(brickFace);
                    }
                }

                brickFace.AddComponent<GrooveHandler>();
            }
        }

        private void AddGrooveFacesBoxedConnections(GameObject newBlock, List<LDrawAbstractConnectionPoint> connectionPoints)
        {
            List<LDrawAbstractBoxConnector> boxConnectionPoints = new List<LDrawAbstractBoxConnector>();

            foreach(LDrawAbstractConnectionPoint connectorPoint in connectionPoints)
            {
                if(connectorPoint is LDrawAbstractBoxConnector)
                {
                    boxConnectionPoints.Add((LDrawAbstractBoxConnector)connectorPoint);
                }
            }

            if (boxConnectionPoints.Count == 0)
            {
                return;
            }

            List<Plane> brickPlanes = new List<Plane>();
            foreach (LDrawAbstractBoxConnector connectionPoint in boxConnectionPoints)
            {

                if (!brickPlanes.Exists(plane => Math.Abs(plane.GetDistanceToPoint(connectionPoint.ConnectorPosition.transform.GetChild(0).position)) < 0.001)
                    && connectionPoint.IsValideConnectionPoint())
                {
                    brickPlanes.Add(new Plane(-connectionPoint.ConnectorPosition.transform.up, connectionPoint.ConnectorPosition.transform.GetChild(0).position));
                }
            }

            foreach (Plane plane in brickPlanes)
            {
                GameObject brickFace = new GameObject("GrooveFace");

                brickFace.transform.SetParent(newBlock.transform);

                foreach (LDrawAbstractBoxConnector connectionPoint in boxConnectionPoints)
                {

                    if (System.Math.Abs(plane.GetDistanceToPoint(connectionPoint.ConnectorPosition.transform.GetChild(0).position)) < 0.0001)
                    {
                        connectionPoint.GenerateColliderPositions(brickFace);
                    }
                }

                brickFace.AddComponent<GrooveHandler>();
            }

        }


    }

    
        

}
