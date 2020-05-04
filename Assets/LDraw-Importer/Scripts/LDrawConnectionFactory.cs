using System;
using System.Collections.Generic;
using UnityEngine;

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
            ["box5"] = (pos) => new LDrawBox5(pos),
            ["box4"] = (pos) => new LDrawBox4(pos),
            ["stud10"] = (pos) => new LDrawStud10(pos)
        };

        private static List<LDrawAbstractConnectionPoint> connectionPoints = new List<LDrawAbstractConnectionPoint>();

        public static LDrawAbstractConnectionPoint ContructConnectionObject(string className, GameObject position)
        {
            if (className.StartsWith("box"))
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




        public static bool IsBetween(float min, float max, float number)
        {
            return (number >= min && number <= max);
        }

        public static void FlushFactory()
        {
            connectionPoints.ForEach(connectionPoint => connectionPoint.AddConnectionPoint(connectionPoints));
            connectionPoints.Clear();
        }
    }
}

