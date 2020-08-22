using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace LDraw
{
    public class LDrawConnectionFactory
    {
        static readonly IDictionary<string, Func<GameObject, object>> connectionOptions = new Dictionary<string, Func<GameObject, object>>
        {
            ["stud"] = (pos) => new LDrawStud(pos),
            ["stud2"] = (pos) => new LDrawStud2(pos),
            ["stud3"] = (pos) => new LDrawStud3(pos),
            ["stud4a"] = (pos) => new LDrawStud4a(pos),
            ["stud2a"] = (pos) => new LDrawStud2a(pos),
            ["stud4f2w"] = (pos) => new LDrawStud4f2w(pos),
            ["stud4"] = (pos) => new LDrawStud4(pos),
            ["stud6"] = (pos) => new LDrawStud6(pos),
            ["stud15"] = (pos) => new LDrawStud15(pos),
            ["box5"] = (pos) => new LDrawBox5(pos),
            ["box4"] = (pos) => new LDrawBox4(pos),
            //["box3"] = (pos) => new LDrawBox3(pos),
            ["stud10"] = (pos) => new LDrawStud10(pos)
        };

        static Dictionary<string, LDrawBlockSpecificConnection> DictBlockspecificConnections = new Dictionary<string, LDrawBlockSpecificConnection>();

        private static List<LDrawAbstractConnectionPoint> connectionPoints = new List<LDrawAbstractConnectionPoint>();

        public static LDrawAbstractConnectionPoint ContructConnectionObject(string className, GameObject position)
        {
            if (className.StartsWith("box") && className.Length >= 4)
            {
                className = className.Substring(0,4);
            }
            if (!connectionOptions.TryGetValue(className.ToLower(), out var connectionConstructor)) {
                return null;
            }
            LDrawAbstractConnectionPoint newConnection = (LDrawAbstractConnectionPoint)connectionConstructor(position);
            connectionPoints.Add(newConnection);
            return newConnection;
        }

        public static LDrawBlockSpecificConnection GetBlockSpecificConnection(string blockName)
        {
            if(DictBlockspecificConnections.Count == 0)
            {
                ParseConnectionJSON();
            }

            if (DictBlockspecificConnections.ContainsKey(blockName))
            {
                return DictBlockspecificConnections[blockName];
            }
            return null;
            
        }

        public static void ParseConnectionJSON()
        {
            TextAsset JSONFile = Resources.Load<TextAsset>("ConnectionsPlacement");
            JSONClass jsonClass = JsonUtility.FromJson<JSONClass>(JSONFile.text);
            
            for(int i = 0; i < jsonClass.connectionPointsArray.Length; i++)
            {
                if (!DictBlockspecificConnections.ContainsKey(jsonClass.connectionPointsArray[i].name))
                {
                    DictBlockspecificConnections.Add(jsonClass.connectionPointsArray[i].name, new LDrawBlockSpecificConnection(jsonClass.connectionPointsArray[i]));
                }
                
            }
        }


        public static bool IsBetween(float min, float max, float number)
        {
            number = Math.Abs(number);
            return (number >= min && number <= max);
        }

        public static void FlushFactory()
        {
            connectionPoints.ForEach(connectionPoint => connectionPoint.AddConnectionPoint(connectionPoints));
            connectionPoints.Clear();
        }
    }

    [Serializable]
    public class JSONClass
    {
        public ConnectionPoint[] connectionPointsArray;
    }

    [Serializable]
    public class ConnectionPoint
    {
        public string name;
        public bool deleteGrooves;
        public SerializableVector3[] Taps;
        public SerializableVector3[] Grooves;
    }
}

