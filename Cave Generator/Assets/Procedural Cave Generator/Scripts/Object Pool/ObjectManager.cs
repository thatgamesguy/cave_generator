using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

namespace CaveGenerator
{
	[RequireComponent (typeof(ObjectPool))]
	public class ObjectManager : MonoBehaviour
	{
		public ObjectPool pool;

		protected List<GameObject> objects = new List<GameObject> ();

		private static ObjectManager _instance;
		
		public static ObjectManager instance {
			get {
						
				if (!_instance) {
					_instance = GameObject.FindObjectOfType<ObjectManager> ();
				}

				return _instance;
			}
		}



		public GameObject GetObject (GameObject prefab, Vector2 position, Quaternion rotation, bool onlyPooled)
		{
		
			GameObject obj = null;
			
			if (Application.isPlaying) {

				obj = pool.GetObjectForType (prefab.name, onlyPooled);
			
				if (obj) {
					obj.transform.position = position;
					obj.transform.rotation = rotation;
					obj.SetActive (true);
				
					objects.Add (obj);
				} 
			} else {
#if UNITY_EDITOR
				obj = (GameObject)Editor.Instantiate (prefab, position, rotation);
#endif
			}
			
			return obj;
		}

		public GameObject GetObject (GameObject prefab, Vector2 position)
		{
			return GetObject (prefab, position, Quaternion.identity, false);
		}

		public GameObject GetObject (GameObject prefab, Vector2 position, bool onlyPooledObjects)
		{
			return GetObject (prefab, position, Quaternion.identity, onlyPooledObjects);
		}

		public void RemoveObject (GameObject obj)
		{
			pool.PoolObject (obj);
					
			objects.Remove (obj);

		}


		public void RemoveObjects ()
		{
			for (int i = 0; i < objects.Count; i++) {
				pool.PoolObject (objects [i]);
		
			}
			
			objects.Clear ();
		}

	}
}
