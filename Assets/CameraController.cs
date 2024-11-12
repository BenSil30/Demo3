using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public GameObject PlayerObject;

	// Start is called before the first frame update
	private void Start()
	{
	}

	// Update is called once per frame
	private void Update()
	{
		transform.position = new Vector3(PlayerObject.transform.position.x, PlayerObject.transform.position.y, transform.position.z);
	}
}