using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace LDraw
{
    public class LDrawModelConverter
    {
        private const float GROOVE_OFFSET = 0.1077033f;

        public void ConvertLDrawModel(LDrawModel lDrawModel)
        {
            GameObject lDrawGameObject = lDrawModel.CreateMeshGameObject(LDrawConfig.Instance.ScaleMatrix);
            GameObject newBlock = CombineTileMeshes(lDrawGameObject);


            lDrawGameObject.transform.LocalReflect(Vector3.up);
            newBlock.transform.LocalReflect(Vector3.up);
            AddTapFaces(newBlock, lDrawModel._ConnectionPoints);
            AddGrooveFaces(newBlock, lDrawModel._ConnectionPoints);
            
            

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
            //combinedBlock.GetComponent<MeshFilter>().mesh = new Mesh();
            //combinedBlock.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine);

            float quality = 0.1f;
            var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
            meshSimplifier.Initialize(mesh);
            meshSimplifier.SimplifyMesh(quality);
            combinedBlock.GetComponent<MeshFilter>().mesh = meshSimplifier.ToMesh();




            //Destroy(container);
            return combinedBlock;

        }

        private void AddTapFaces(GameObject newBlock, List<LDrawConnectionPoint> connectionPoints)
        {
            
            List<LDrawConnectionPoint> tapConnectionPoints = connectionPoints.FindAll(connectionPoint => connectionPoint.ConnectionType == LDRawConnectionType.TAP_CONNECTION);

            if (tapConnectionPoints.Count == 0)
            {
                return;
            }

            List<Plane> brickPlanes = new List<Plane>();
            foreach (LDrawConnectionPoint connectionPoint in tapConnectionPoints)
            {
                if (!brickPlanes.Exists(plane => plane.normal == -connectionPoint.ConnectorPosition.transform.up))
                {
                    brickPlanes.Add(new Plane(-connectionPoint.ConnectorPosition.transform.up, connectionPoint.ConnectorPosition.transform.position));
                }
            }

            foreach (Plane plane in brickPlanes)
            {
                GameObject brickFace = new GameObject("TapFace");

                brickFace.transform.SetParent(newBlock.transform);

                foreach (LDrawConnectionPoint connectionPoint in tapConnectionPoints)
                {
                    if (System.Math.Abs(plane.GetDistanceToPoint(connectionPoint.ConnectorPosition.transform.position)) < 0.0001)
                    {
                        GameObject connectionPointTemp = new GameObject("Collider");
                        connectionPointTemp.AddComponent<TapCollider>();

                        BlockGeometryScript.AddBoxCollider(new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0, 0, 0), true, connectionPointTemp);
                        connectionPointTemp.transform.SetParent(brickFace.transform);
                        connectionPointTemp.transform.LookAt(-connectionPoint.ConnectorPosition.transform.up);
                        connectionPointTemp.transform.Rotate(90, 0, 0);
                        connectionPointTemp.transform.position = connectionPoint.ConnectorPosition.transform.position;

                    }
                }
                brickFace.AddComponent<TapHandler>();
            }
        }

        private void AddGrooveFaces(GameObject newBlock, List<LDrawConnectionPoint> connectionPoints)
        {
            List<LDrawConnectionPoint> grooveConnectionPoints = connectionPoints.FindAll(connectionPoint => connectionPoint.ConnectionType == LDRawConnectionType.GROOVE_CONNECTION);

            if(grooveConnectionPoints.Count == 0)
            {
                return;
            }

            List<Plane> brickPlanes = new List<Plane>();
            foreach (LDrawConnectionPoint connectionPoint in grooveConnectionPoints)
            {
                if (!brickPlanes.Exists(plane => plane.normal == -connectionPoint.ConnectorPosition.transform.up))
                {
                    brickPlanes.Add(new Plane(-connectionPoint.ConnectorPosition.transform.up, connectionPoint.ConnectorPosition.transform.GetChild(0).position));
                }
            }

            foreach (Plane plane in brickPlanes)
            {
                GameObject brickFace = new GameObject("GrooveFace");

                brickFace.transform.SetParent(newBlock.transform);

                foreach (LDrawConnectionPoint connectionPoint in grooveConnectionPoints)
                {
                    
                    if (System.Math.Abs(plane.GetDistanceToPoint(connectionPoint.ConnectorPosition.transform.GetChild(0).position)) < 0.0001)
                    {
                        AddGroveCollider(brickFace, connectionPoint, new Vector3(GROOVE_OFFSET,0,0));
                        AddGroveCollider(brickFace, connectionPoint, new Vector3(-GROOVE_OFFSET,0,0));
                        AddGroveCollider(brickFace, connectionPoint, new Vector3(0,0,GROOVE_OFFSET));
                        AddGroveCollider(brickFace, connectionPoint, new Vector3(0,0,-GROOVE_OFFSET));
                    }
                }
                brickFace.AddComponent<GrooveHandler>();
            }
        }

        private void AddGroveCollider(GameObject brickFace, LDrawConnectionPoint connectionPoint, Vector3 offset)
        {

            GameObject connectionPointTemp = new GameObject("Collider");
            connectionPointTemp.AddComponent<GrooveCollider>();
            BlockGeometryScript.AddBoxCollider(new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0, 0, 0), true, connectionPointTemp);
            connectionPointTemp.transform.SetParent(brickFace.transform);
            connectionPointTemp.transform.LookAt(-connectionPoint.ConnectorPosition.transform.up);
            connectionPointTemp.transform.Rotate(90, 0, 0);
            connectionPointTemp.transform.position = connectionPoint.ConnectorPosition.transform.GetChild(0).position;
            connectionPointTemp.transform.position += offset;
        }
    }

    
        

}
