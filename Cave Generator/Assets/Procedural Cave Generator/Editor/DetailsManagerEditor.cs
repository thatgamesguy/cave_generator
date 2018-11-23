using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;

namespace CaveGenerator
{
	[CustomEditor (typeof(DetailsManager))]
	public class DetailsManagerEditor : Editor
	{
		private DetailsManager _target;

		// Use this for initialization
		void Awake ()
		{
			_target = (DetailsManager)target;
		}
	
		public override void OnInspectorGUI ()
		{
			DrawDefaultInspector ();
			
			EditorGUILayout.HelpBox ("Make sure you generate the environment before adding details.", MessageType.Info);

			GUILayout.BeginHorizontal ();

			if (GUILayout.Button ("Generate")) {
				if (_target.ReGenerateDetails ()) {
					EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
				}
			}
			
			if (GUILayout.Button ("Delete")) {
				if (_target.DestroyDetails ())
					EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
			}
			
			GUILayout.EndHorizontal ();
		}
	}
}
