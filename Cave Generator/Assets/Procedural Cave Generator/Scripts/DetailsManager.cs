using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaveGenerator
{
	[System.Serializable]
	public class DetailsManager : MonoBehaviour
	{
		public int MaxDetails = 60;
		[Range (0, 1)]
		public float
			PlacementChance;
			
		public GameObject Detail;
		public GameObject ParentContainer;

		private List<GameObject> _detailsList = new List<GameObject> ();
		private List<Node> _usedNodes = new List<Node> ();
		
		private int _placementAttempts = 0;
		private static readonly int _maxPlacementAttempts = 50;

		private static DetailsManager _instance;
		public static DetailsManager instance { 
			get { 

				if (!_instance) {
					_instance = GameObject.FindObjectOfType<DetailsManager> ();
				}

				return _instance; 
			}
		}
		
		public bool ReGenerateDetails ()
		{
			if (Utilities.instance.IsDebug)
				Debug.Log ("Destroying old details if present");
			DestroyDetails ();
			return Generate ();
		}

		public bool Generate ()
		{
			if (DetailsGenerated ()) {
				return false;
			}
			
			ParentContainer.transform.localScale = Vector3.one;
			//GridManager.instance.ParentContainer.transform.localScale = Vector3.one;

			var details = GridManager.instance.TexturePack.Details;

			if (details == null || details.Length == 0) {
				Debug.Log ("No details contained in the current texture pack");
				return false;
			}

			if (Utilities.instance.IsDebug)
				Debug.Log ("Placing Details");

			for (int i = 0; i < MaxDetails; i++) {

				if (Random.Range (0f, 1f) > PlacementChance) {
					var index = GetDetailIndex (details.Length);
					
					var detail = details [index];

					_placementAttempts = 0;
					Node node = null;
					do {
						node = GridManager.instance.GetRandomFloorNode ();
			
						if (node == null || ++_placementAttempts > _maxPlacementAttempts) {
							break;
						}

					} while (_usedNodes.Contains (node));

					if (node == null) { 
						Debug.Log ("No floor nodes found.");
						return false;
					}
					
					if (++_placementAttempts > _maxPlacementAttempts) {
						continue;
					}

					_usedNodes.Add (node);
					
					var position = Utilities.instance.GetNodePosition (node) - new Vector2 (0, Utilities.instance.TileSize.Value.y * (0.4f /** GridManager.instance.GridScale.y*/));
					
					var obj = ObjectManager.instance.GetObject (Detail, position);
					
					obj.transform.SetParent (ParentContainer.transform);
					
					UpdateSprite (obj, detail);

					_detailsList.Add (obj);
				}
			}
			
			ParentContainer.transform.localScale = GridManager.instance.GridScale;
			//GridManager.instance.ParentContainer.transform.localScale = GridManager.instance.GridScale;
			return true;

		}

		public bool DestroyDetails ()
		{
			bool destroyed = DetailsGenerated ();
			
			ParentContainer.transform.ClearImmediate ();
			_usedNodes.Clear ();
			
			return destroyed;
		}

		private bool DetailsGenerated ()
		{
			return ParentContainer.transform.childCount > 0;
		}

		private void UpdateSprite (GameObject obj, Sprite sprite)
		{
			var renderer = obj.GetComponent<SpriteRenderer> ();

			if (renderer) {
				renderer.sprite = sprite;
			}
		}

		private int GetDetailIndex (int length)
		{
			return Random.Range (0, length);
		}
	}
}
