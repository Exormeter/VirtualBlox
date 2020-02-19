using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem {
    public class Teleport : MonoBehaviour
    {
        public GameObject pointer;
        public Player Player;
        public Material material;
        public CheckPlayerHeightLevel CheckPlayerHeight;

        private LineRenderer lineRenderer;
        private float maxDistanceLine = 5f;
        private Collider hittedCollider;
        private bool isTeleporting = false;
        private bool teleportedActive = false;
        private float fadeTime = 0.5f;
        private bool hasPosition;


        // Start is called before the first frame update
        void Start()
        {
            pointer.SetActive(false);
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = material;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.enabled = false;
            
        }

        // Update is called once per frame
        void Update()
        {
            if (teleportedActive)
            {
                hasPosition = UpdatePointer();
            }
            
        }

        private bool UpdatePointer()
        {

            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;
            lineRenderer.SetPosition(0, transform.position);

            int layerMask = ~(1 << 9);

            if (Physics.Raycast(ray, out hit, maxDistanceLine, layerMask))
            {
                hittedCollider = hit.collider;
                pointer.transform.position = hit.point;
                lineRenderer.SetPosition(1, hit.point);
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
                return true;
            }

            Vector3 endPosition = transform.position + (transform.forward * maxDistanceLine);
            pointer.transform.position = endPosition;
            lineRenderer.SetPosition(1, endPosition);
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            
            return false;
        }

        public void TryTeleport()
        {
            teleportedActive = false;
            pointer.SetActive(false);
            lineRenderer.enabled = false;

            if(hittedCollider == null || !hasPosition)
            {
                return;
            }

            Transform cameraRig = Player.transform;
            Vector3 headPosition = CheckPlayerHeight.ApproximatelyFeetPositionXZ();
            Vector3 groundPositon = new Vector3(headPosition.x, cameraRig.position.y, headPosition.z);
            Vector3 targetPosition = new Vector3();

            //Pointer point to Floor
            if (hittedCollider.gameObject.transform.root.tag.Equals("Floor"))
            {
                targetPosition = pointer.transform.position;
            }

            //Pointer pointed on top of Block
            else if (hittedCollider.tag.Equals("TopColliderContainer"))
            {
                targetPosition = pointer.transform.position;
                targetPosition.y += 0.2f;
            }

            //pointer pointed onto side of Block
            else if (hittedCollider.gameObject.transform.root.tag.Equals("Block"))
            {
                GameObject block = hittedCollider.gameObject.transform.root.gameObject;
                targetPosition = block.transform.position + Vector3.up * 20;
                Ray ray = new Ray(targetPosition, Vector3.down);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit))
                {
                    targetPosition = hit.point;
                }
            }


            else
            {
                return;
            }
            
            Vector3 translateVector =  targetPosition - groundPositon;
            StartCoroutine(MovePlayer(cameraRig, translateVector));
        }

        private IEnumerator MovePlayer(Transform player, Vector3 translation)
        {
            isTeleporting = true;

            SteamVR_Fade.Start(Color.black, fadeTime, true);

            yield return new WaitForSeconds(0.5f);
            player.position += translation;
            CheckPlayerHeight.OnTeleport();
            SteamVR_Fade.Start(Color.clear, fadeTime, true);

            isTeleporting = false;
        }

        public void OnActivteTelepoter()
        {
            teleportedActive = true;
            pointer.SetActive(true);
            lineRenderer.enabled = true;
        }
    }
}

