using UnityEngine;
using System.Collections;


namespace CaveGenerator
{
	[RequireComponent (typeof(SpriteRenderer))]
	public class DemoCameraBackground : MonoBehaviour
	{
		private SpriteRenderer _renderer;

		void Awake ()
		{
			_renderer = GetComponent<SpriteRenderer> ();
		}


		public void UpdateBackground (Sprite sprite)
		{
			_renderer.sprite = sprite;
		}

	}
}
