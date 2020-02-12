using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem {
    public class Teleport : MonoBehaviour
    {
        public GameObject pointer;
        public Player Player;
        public Material TeleportPossible;
        public Material TeleportNotPossible;
        public CheckPlayerHeight CheckPlayerHeight;

        private LineRenderer lineRenderer;
        private GameObject hittedBlock;
        private bool isTeleporting = false;
        private bool teleportedActive = false;
        private float fadeTime = 0.5f;
        private bool hasPosition;


        // Start is called before the first frame update
        void Start()
        {
            pointer.SetActive(false);
            lineRenderer = gameObject.AddComponent<LineRenderer>();
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

            if(Physics.Raycast(ray, out hit))
            {
                hittedBlock = hit.collider.gameObject.transform.root.gameObject;
                pointer.transform.position = hit.point;
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, hit.point);
                return true;
            }
            return false;
        }

        public void TryTeleport()
        {
            teleportedActive = false;
            pointer.SetActive(false);
            lineRenderer.enabled = false;

            if(hittedBlock == null)
            {
                return;
            }

            Transform cameraRig = Player.transform;
            Vector3 headPosition = Player.hmdTransform.position;
            Vector3 groundPositon = new Vector3(headPosition.x, cameraRig.position.y, headPosition.z);
            Vector3 targetPosition = new Vector3();

            if (hittedBlock.tag.Equals("Floor"))
            {
                targetPosition = pointer.transform.position;
            }
            else if (hittedBlock.tag.Equals("Block"))
            {
                targetPosition = hittedBlock.transform.position;
                targetPosition.y += 0.2f;
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

