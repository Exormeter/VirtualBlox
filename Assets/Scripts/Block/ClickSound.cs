using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class ClickSound : MonoBehaviour
    {

        private AudioSource audioSource;
        private AudioClip blockClickSound;
        // Start is called before the first frame update
        void Start()
        {
            blockClickSound = Resources.Load("LegoConnectSound", typeof(AudioClip)) as AudioClip;
            audioSource = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnBlockAttach()
        {
            audioSource.PlayOneShot(blockClickSound);
        }
    }
}

