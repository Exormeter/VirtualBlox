using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class BlockGeometryLDrawPart: BlockGeometryScript
    {
        public new void Awake()
        {
            this.mesh = GetComponentInChildren<MeshFilter>().mesh;
            TapContainer = GetComponentInChildren<TapHandler>()?.gameObject;
            GroovesContainer = GetComponentInChildren<GrooveHandler>()?.gameObject;
            BlockIdentifier = BlockIdentifier.BLOCK_LDRAW;
        }
        public void SetWallCollider()
        {
            Collider[] colliders = GetComponents<Collider>();
            base.wallColliderList.AddRange(colliders);
        }

        public void SetLDrawBlockStructure(LDrawBlockStructure lDrawBlockStructure)
        {
            base.BlockStructure = lDrawBlockStructure;
        }

        public override void SetBlockWalkable(bool walkable)
        {
            if (walkable)
            {
                gameObject.layer = WALKABLE_LAYER;
            }
            else
            {
                gameObject.layer = 0;
            }
            
        }
    }
}

