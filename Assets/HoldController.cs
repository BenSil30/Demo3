using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldController : MonoBehaviour
{
	public float GripNeededForHold;
	public float GripTemp;
	public float ChalkedMultiplier;
	public bool IsChalked = false;

	// Start is called before the first frame update
	private void Start()
	{
		GripTemp = GripNeededForHold;
		ChalkedMultiplier = GripTemp - 1f;
	}

	// Update is called once per frame
	private void Update()
	{
		CheckForChalked();
	}

	public void CheckForChalked()
	{
		if (IsChalked)
		{
			GetComponent<SpriteRenderer>().color = new Color(GetComponent<SpriteRenderer>().color.r,
				GetComponent<SpriteRenderer>().color.g, GetComponent<SpriteRenderer>().color.b,
				.5f);
			GripTemp = ChalkedMultiplier;
		}
	}
}