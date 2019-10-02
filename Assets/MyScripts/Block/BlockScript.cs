using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{


    public class BlockScript : MonoBehaviour
    {
        private HashSet<BlockContainer> connectedBlocksOnTap = new HashSet<BlockContainer>();
        private HashSet<BlockContainer> connectedBlocksOnGroove= new HashSet<BlockContainer>();
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void addConectedBlockTap(GameObject Block)
        {
            connectedBlocksOnTap.Add(new BlockContainer(Block));
        }

        public void addConectedBlockGroove(GameObject Block)
        {
            connectedBlocksOnGroove.Add(new BlockContainer(Block));
        }

        public void removeConnectedBlockGroove(GameObject Block)
        {
            connectedBlocksOnGroove.Remove(new BlockContainer(Block));
        }

        public void removeConnectedBlockTap(GameObject Block)
        {
            connectedBlocksOnTap.Remove(new BlockContainer(Block));
        }
    }

    public class BlockContainer
    {
        public GameObject BlockRootObject { get; }
        public GrooveHandler GrooveHandler { get; }
        public TapHandler TapHandler { get; }
        public BlockGeometryScript BlockGeometry { get; }

        public BlockContainer(GameObject block)
        {
            BlockRootObject = block;
            GrooveHandler = block.GetComponentInChildren<GrooveHandler>();
            TapHandler = block.GetComponentInChildren<TapHandler>();
            BlockGeometry = block.GetComponent<BlockGeometryScript>();
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                GameObject gameObject = (GameObject) obj;
                return gameObject.GetInstanceID() == BlockRootObject.GetInstanceID();
            }
        }

        public override int GetHashCode()
        {
            return BlockRootObject.GetHashCode();
        }
    }
}
