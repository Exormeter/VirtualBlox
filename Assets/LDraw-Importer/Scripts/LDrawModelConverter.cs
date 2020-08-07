using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace LDraw
{
    public class LDrawModelConverter
    {

        public GameObject ConvertLDrawModel(LDrawModel lDrawModel)
        {
            
            GameObject lDrawGameObject = lDrawModel.CreateMeshGameObject(LDrawConfig.Instance.ScaleMatrix);
            GameObject lDrawBlock = CombineTileMeshes(lDrawGameObject);


            GameObject block = new GameObject("Block");

            
            lDrawBlock.AddComponent<NonConvexMeshCollider>().Calculate();


            LDrawConnectionFactory.FlushFactory();
            
            AddTapFace(lDrawBlock, lDrawModel._ConnectionPoints);
            AddGrooveFace(lDrawBlock, lDrawModel._ConnectionPoints);
            //AddGrooveFacesBoxedConnections(lDrawBlock, lDrawModel._ConnectionPoints);

            lDrawBlock.transform.Rotate(new Vector3(0, 0, -180));
            lDrawBlock.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            


            lDrawBlock.transform.SetParent(block.transform);

            Connector[] connectors = lDrawBlock.GetComponentsInChildren<Connector>();
            foreach (Connector connector in connectors)
            {
                connector.gameObject.transform.SetParent(block.transform);
            }

            BoxCollider[] colliders = lDrawBlock.GetComponents<BoxCollider>();
            foreach (BoxCollider collider in colliders)
            {
                BoxCollider copyCollider = block.AddComponent<BoxCollider>();
                copyCollider.center = collider.center;
                copyCollider.size = collider.size;
                copyCollider.center *= 0.4f;
                copyCollider.size *= 0.38f;
                copyCollider.center = new Vector3(copyCollider.center.x, copyCollider.center.y * -1, copyCollider.center.z);
                UnityEngine.Object.DestroyImmediate(collider);
            }

            UnityEngine.Object.DestroyImmediate(lDrawGameObject);
            return block;
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
            combinedBlock.tag = "MeshHolder";
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

        /*private void AddTapFaces(GameObject newBlock, List<LDrawAbstractConnectionPoint> connectionPoints)
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
        }*/

        private void AddTapFace(GameObject newBlock, List<LDrawAbstractConnectionPoint> connectionPoints)
        {
            List<LDrawAbstractConnectionPoint> tapConnectionPoints = connectionPoints.FindAll(connectionPoint => connectionPoint is LDrawAbstractTapConnection);

            if (tapConnectionPoints.Count == 0)
            {
                return;
            }

            
            GameObject brickFace = new GameObject("Taps");

            brickFace.transform.SetParent(newBlock.transform);

            foreach (LDrawAbstractConnectionPoint connectionPoint in tapConnectionPoints)
            {
                
                connectionPoint.GenerateColliderPositions(brickFace);
              

            brickFace.AddComponent<TapHandler>();
            }
        }

        /*private void AddGrooveFaces(GameObject newBlock, List<LDrawAbstractConnectionPoint> connectionPoints)
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
        }*/

        private void AddGrooveFace(GameObject newBlock, List<LDrawAbstractConnectionPoint> connectionPoints)
        {
            List<LDrawAbstractConnectionPoint> grooveConnectionPoints = connectionPoints.FindAll(connectionPoint => connectionPoint is LDrawAbstractGrooveConnection);

            if (grooveConnectionPoints.Count == 0)
            {
                return;
            }

            

            
            GameObject brickFace = new GameObject("Grooves");

            brickFace.transform.SetParent(newBlock.transform);

            foreach (LDrawAbstractConnectionPoint connectionPoint in grooveConnectionPoints)
            {
                connectionPoint.GenerateColliderPositions(brickFace);
            }

            brickFace.AddComponent<GrooveHandler>();
            
        }

        /*private void AddGrooveFacesBoxedConnections(GameObject newBlock, List<LDrawAbstractConnectionPoint> connectionPoints)
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
        }*/

        private Mesh ScaleMesh(Mesh mesh)
        {
            float ScaleX = 0.4f;
            float ScaleY = 0.4f;
            float ScaleZ = 0.4f;
            bool RecalculateNormals = false;
            Vector3[] _baseVertices = mesh.vertices;

            
            var vertices = new Vector3[_baseVertices.Length];
            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = _baseVertices[i];
                vertex.x = vertex.x * ScaleX;
                vertex.y = vertex.y * ScaleY;
                vertex.z = vertex.z * ScaleZ;
                vertices[i] = vertex;
            }
            mesh.vertices = vertices;
            if (RecalculateNormals)
            {
                mesh.RecalculateNormals();
            }
            
            mesh.RecalculateBounds();
            return mesh;
        }
    }

    
        

}
