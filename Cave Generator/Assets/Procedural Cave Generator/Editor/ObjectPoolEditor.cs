using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;

namespace CaveGenerator
{
	[CustomEditor (typeof(ObjectPool))]
	public class ObjectPoolEditor : Editor
	{

		private ObjectPool _target;
	
		void Awake ()
		{
			_target = (ObjectPool)target;
		}
		
		public override void OnInspectorGUI ()
		{
			DrawDefaultInspector ();
			
			if (GUILayout.Button ("Delete Pooled Objects")) {
				if (_target.DestroyPooledObjects ()) {
					EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
				}
			}
		}
	}
}
