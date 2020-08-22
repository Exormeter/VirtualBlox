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


            lDrawBlock.AddComponent<NonConvexMeshCollider>();


            LDrawConnectionFactory.FlushFactory();


            GameObject tapFace = new GameObject("Taps");
            tapFace.transform.SetParent(lDrawBlock.transform);

            GameObject grooveFace = new GameObject("Grooves");
            grooveFace.transform.SetParent(lDrawBlock.transform);

            AddTapFaceCollider(lDrawBlock, lDrawModel._ConnectionPoints);
            AddGrooveFaceCollider(lDrawBlock, lDrawModel._ConnectionPoints);
            AddSpecificCollider(lDrawBlock, lDrawModel._ConnectionPoints);

            tapFace.AddComponent<TapHandler>();
            grooveFace.AddComponent<GrooveHandler>();


            lDrawBlock.transform.Rotate(new Vector3(0, 0, -180));
            lDrawBlock.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            

            lDrawBlock.transform.SetParent(block.transform);

            Connector[] connectors = lDrawBlock.GetComponentsInChildren<Connector>();
            foreach (Connector connector in connectors)
            {
                connector.gameObject.transform.SetParent(block.transform);
            }

            UnityEngine.Object.DestroyImmediate(lDrawGameObject);
            lDrawBlock.GetComponent<NonConvexMeshCollider>().Calculate();

            BoxCollider[] colliders = lDrawBlock.GetComponents<BoxCollider>();
            foreach (BoxCollider collider in colliders)
            {
                BoxCollider copyCollider = block.AddComponent<BoxCollider>();
                copyCollider.center = block.transform.InverseTransformPoint(lDrawBlock.transform.TransformPoint(collider.center));
                copyCollider.size = collider.size;
                copyCollider.size *= 0.38f;
                UnityEngine.Object.DestroyImmediate(collider);
            }

            //Debug
            //block.transform.Translate(new Vector3(2, 0, 2));
            //block.transform.Rotate(new Vector3(0, 0, 100));
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
            combinedBlock.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);

            //Debug
            //combinedBlock.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Specular"));
            //combinedBlock.GetComponent<MeshRenderer>().material.color = Color.yellow;
            return combinedBlock;
        }

        

        private void AddTapFaceCollider(GameObject block, List<LDrawAbstractConnectionPoint> connectionPoints)
        {
            List<LDrawAbstractConnectionPoint> tapConnectionPoints = connectionPoints.FindAll(connectionPoint => connectionPoint is LDrawAbstractTapConnection);

            if (tapConnectionPoints.Count == 0)
            {
                return;
            }

            foreach (LDrawAbstractConnectionPoint connectionPoint in tapConnectionPoints)
            {
                
                connectionPoint.GenerateColliderPositions(block);
            }
        }


        private void AddGrooveFaceCollider(GameObject block, List<LDrawAbstractConnectionPoint> connectionPoints)
        {
            List<LDrawAbstractConnectionPoint> grooveConnectionPoints = connectionPoints.FindAll(connectionPoint => connectionPoint is LDrawAbstractGrooveConnection || connectionPoint is LDrawAbstractBoxConnector);

            if (grooveConnectionPoints.Count == 0)
            {
                return;
            }

            foreach (LDrawAbstractConnectionPoint connectionPoint in grooveConnectionPoints)
            {
                connectionPoint.GenerateColliderPositions(block);
            }
        }

        private void AddSpecificCollider(GameObject block, List<LDrawAbstractConnectionPoint> connectionPoints)
        {
            List<LDrawAbstractConnectionPoint> specificConnectionPoints = connectionPoints.FindAll(connectionPoint => connectionPoint is LDrawBlockSpecificConnection);

            if (specificConnectionPoints.Count == 0)
            {
                return;
            }

            foreach (LDrawAbstractConnectionPoint connectionPoint in specificConnectionPoints)
            {
                connectionPoint.GenerateColliderPositions(block);
            }
        }



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
