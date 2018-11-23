using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;

namespace CaveGenerator
{
	[CustomEditor (typeof(GridManager))]
	public class GridManagerEditor : Editor
	{
		private GridManager _target;

		void Awake ()
		{
			_target = (GridManager)target;
		}

		public override void OnInspectorGUI ()
		{
			DrawDefaultInspector ();
			
			EditorGUILayout.HelpBox ("Depending on the size of the grid, it may take a while to destroy/create. Please be patient!", MessageType.Info);

			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button ("Generate")) {
				_target.ReGenerate ();
				EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene());
			}

			if (GUILayout.Button ("Delete")) {
				if (_target.DestroyEnvironment ()) {
					EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
				}
			}
			
			GUILayout.EndHorizontal ();
		}
	}
}
