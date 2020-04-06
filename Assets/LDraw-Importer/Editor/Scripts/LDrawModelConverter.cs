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


            lDrawGameObject.transform.LocalReflect(Vector3.up);
            newBlock.transform.LocalReflect(Vector3.up);
            List<Plane> brickPlanes = new List<Plane>();
            foreach (GameObject connectionPoint in lDrawModel._ConnectionPoints)
            {
                //Debug.Log(-connectionPoint.transform.up);
                if (!brickPlanes.Exists(plane => plane.normal == -connectionPoint.transform.up))
                {
                    brickPlanes.Add(new Plane(-connectionPoint.transform.up, connectionPoint.transform.position));
                }
                //BoxCollider newCollider = newBlock.AddComponent<BoxCollider>();
                //newCollider.transform.position = connectionPoint.transform.position;
                
            }

            foreach(Plane plane in brickPlanes)
            {
                GameObject brickFace = new GameObject("TapFace");
                
                brickFace.transform.SetParent(newBlock.transform);
                //GameObject brickNormale = new GameObject("BrickFaceNormale");
                //brickNormale.transform.position = plane.normal;
                //brickNormale.transform.SetParent(brickFace.transform);
                

                foreach (GameObject connectionPoint in lDrawModel._ConnectionPoints)
                {
                    if (System.Math.Abs(plane.GetDistanceToPoint(connectionPoint.transform.position)) < 0.0001)
                    {
                        GameObject connectionPointTemp = new GameObject("Collider");
                        connectionPointTemp.AddComponent<TapCollider>();
                        
                        BlockGeometryScript.AddBoxCollider(new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0, 0, 0), true, connectionPointTemp);
                        connectionPointTemp.transform.SetParent(brickFace.transform);
                        connectionPointTemp.transform.LookAt(-connectionPoint.transform.up);
                        connectionPointTemp.transform.Rotate(90, 0, 0);
                        connectionPointTemp.transform.position = connectionPoint.transform.position;
                        
                    }
                }
                brickFace.AddComponent<TapHandler>();
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
            combinedBlock.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);

            //Destroy(container);
            return combinedBlock;

        }
    }

    
        

}
