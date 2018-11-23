using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaveGenerator
{
	public class DemoController : MonoBehaviour
	{
		public Camera CameraOne;
		public Light CameraOneLight;
		public Camera CameraTwo;
		public Light CameraTwoLight;
		public DemoCameraBackground background;

        private static readonly float CLOSE_LIGHT_INTENSITY = 0.8f;
        private static readonly float FAR_LIGHT_INTENSITY = 1.5f;

        void Start ()
		{
			GenerateEnvironment ();
		}
	
		// Update is called once per frame
		void Update ()
		{
			if (Input.GetKeyUp (KeyCode.N)) {
	
				GridManager.instance.LoadNextTexturePack ();

				ReGenerateEnvironment ();

				background.UpdateBackground (GridManager.instance.TexturePack.WallMiddle);
			
			} 

			if (Input.GetKeyUp (KeyCode.R)) {
				GridManager.instance.RandomSeed ();
				ReGenerateEnvironment ();
			}

			if (Input.GetKeyUp (KeyCode.C)) {
				SwitchCamera ();
			}
		}

		private void GenerateEnvironment ()
		{
			GridManager.instance.Generate ();
			DetailsManager.instance.Generate ();
		}
		
		private void ReGenerateEnvironment ()
		{
			GridManager.instance.ReGenerate ();
			DetailsManager.instance.ReGenerateDetails ();
		}




		private void SwitchCamera ()
		{
			if (CameraOne.isActiveAndEnabled) {
				CameraOne.enabled = false;
				CameraOneLight.enabled = false;
                CameraOneLight.intensity = CLOSE_LIGHT_INTENSITY;
                CameraTwo.enabled = true;
				CameraTwoLight.enabled = true;
			} else {
				CameraOne.enabled = true;
				CameraOneLight.enabled = true;
                CameraOneLight.intensity = FAR_LIGHT_INTENSITY;
				CameraTwo.enabled = false;
				CameraTwoLight.enabled = false;
			}
		}
	}
}
