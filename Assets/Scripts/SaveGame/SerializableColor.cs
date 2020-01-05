using UnityEngine;
using System;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
	/// <summary>
	/// Since unity doesn't flag the Quaternion as serializable, we
	/// need to create our own version. This one will automatically convert
	/// between Quaternion and SerializableQuaternion
	/// </summary>
	[System.Serializable]
	public struct SerializableColor
	{
		/// <summary>
		/// red component
		/// </summary>
		public float r;

		/// <summary>
		/// green component
		/// </summary>
		public float g;

		/// <summary>
		/// blue component
		/// </summary>
		public float b;

		/// <summary>
		/// alpha component
		/// </summary>
		public float a;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rX"></param>
		/// <param name="rY"></param>
		/// <param name="rZ"></param>
		/// <param name="rW"></param>
		public SerializableColor(float rR, float rG, float rB, float rA)
		{
			r = rR;
			g = rG;
			b = rB;
			a = rA;
		}

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("[{0}, {1}, {2}, {3}]", r, g, b, a);
		}

		/// <summary>
		/// Automatic conversion from SerializableQuaternion to Quaternion
		/// </summary>
		/// <param name="rValue"></param>
		/// <returns></returns>
		public static implicit operator Color(SerializableColor rValue)
		{
			return new Color(rValue.r, rValue.g, rValue.b, rValue.a);
		}

		/// <summary>
		/// Automatic conversion from Quaternion to SerializableQuaternion
		/// </summary>
		/// <param name="rValue"></param>
		/// <returns></returns>
		public static implicit operator SerializableColor(Color rValue)
		{
			return new SerializableColor(rValue.r, rValue.g, rValue.b, rValue.a);
		}
	}
}