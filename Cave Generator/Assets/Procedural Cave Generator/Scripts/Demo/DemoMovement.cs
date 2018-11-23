using UnityEngine;
using System.Collections;

public class DemoMovement : MonoBehaviour
{
	public float MovementSpeed = 1f;

	private Light _light;

	void Awake ()
	{
		_light = GetComponent<Light> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!_light.enabled)
			return;

		var horizontal = Input.GetAxis ("Horizontal");
		var vertical = Input.GetAxis ("Vertical");

		var movement = new Vector2 (horizontal, vertical) * MovementSpeed;

		transform.Translate (movement * Time.deltaTime, Space.World);
	}

}
