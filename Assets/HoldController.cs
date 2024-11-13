using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldController : MonoBehaviour
{
	public float GripNeededForHold;
	public float GripTemp;
	public float DecayRateMultiplier;
	public bool IsChalked = false;
	public float ChalkGripCoeff;

	// Start is called before the first frame update
	private void Start()
	{
		GripTemp = GripNeededForHold;
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
			Color tint = new Color(.8f, .8f, .8f);
			//GripNeededForHold = ChalkGripCoeff;
		}
	}
}