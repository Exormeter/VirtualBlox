using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ConnectionHandler : MonoBehaviour
{

    private void Test()
    {
        //Are the new Blocks collided via the Groove or Taps
        GetComponent<AttachHandHandler>().GrooveOrTapHit(out List<CollisionObject> currentCollisionObjects, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn);

        //Dictionary to hold which Block has collided with how many Groove or Taps
        Dictionary<GameObject, int> blockToTapDict = new Dictionary<GameObject, int>();

        //Wieviele Taps oder Grooves wurden nach dem Rotieren getroffen?
        foreach (CollisionObject collisionObject in currentCollisionObjects)
        {
            if (!blockToTapDict.ContainsKey(collisionObject.CollidedBlock))
            {
                blockToTapDict.Add(collisionObject.CollidedBlock, 1);
            }
            else
            {
                blockToTapDict[collisionObject.CollidedBlock]++;
            }
        }

        //Zu welchem Block muss eine Verbindnung aufgebaut werden mit welcher Stärke?
        foreach (GameObject collidedBlock in blockToTapDict.Keys)
        {
            GetComponent<BlockCommunication>().ConnectBlocks(transform.gameObject, collidedBlock, blockToTapDict[collidedBlock], connectedOn);
        }
    }
}
