using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaveGenerator
{
	public static class ExtensionMethods
	{

		public static void ClearImmediate (this UnityEngine.Transform transform)
		{
			var children = new List<GameObject> ();
			foreach (Transform child in transform)
				children.Add (child.gameObject);
			children.ForEach (child => GameObject.DestroyImmediate (child));

		}
		
		public static string[] MaskToNames (this LayerMask original)
		{
			var output = new List<string> ();
			
			for (int i = 0; i < 32; ++i) {
				int shifted = 1 << i;
				if ((original & shifted) == shifted) {
					string layerName = LayerMask.LayerToName (i);
					if (!string.IsNullOrEmpty (layerName)) {
						output.Add (layerName);
					}
				}
			}
			return output.ToArray ();
		}
	}
}


