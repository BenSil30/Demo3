using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HandManager : MonoBehaviour
{
	public Transform elbow;
	public LineRenderer lineRenderer;
	public Material normalMaterial;
	public Material stretchedMaterial;
	public Material maxDistanceMaterial;

	public PlayerController playerController;
	private float maxHandDistance;
	private float StretchedDist;

	public bool IsOnHold;
	public bool IsGripping;

	public float CurrentGripStrength;
	public float MaxGripStrength;
	public float GripDecayRate;
	public float GripRecoveryRate;
	public float GripInterval;
	private float TimeSinceLastDecay = 0f;

	public bool IsLeftHand;

	private void Start()
	{
		// Ensure the LineRenderer component is attached
		if (lineRenderer == null)
		{
			lineRenderer = GetComponent<LineRenderer>();
		}

		// Initialize LineRenderer settings
		SetUpLineRenderer();

		CurrentGripStrength = MaxGripStrength;
		maxHandDistance = playerController.MaxHandDistance;
		StretchedDist = playerController.StretchDistance;
		IsOnHold = false;
	}

	private void Update()
	{
		// Update the LineRenderer to draw the line between elbow and hand
		UpdateLineRenderer();

		// Update material based on the distance
		UpdateArmColorBasedOnStretchAmount();
		UpdateGripStatus();
		UpdateGripStrength();
		UpdateTintBasedOnGripStrength();
	}

	private void UpdateGripStatus()
	{
		if (!playerController.CanGrab) return;
		switch (IsLeftHand)
		{
			case true:
				// Set IsGripping to true while 'Q' is held down; set to false when 'Q' is not held
				IsGripping = Input.GetKey(KeyCode.LeftShift);
				playerController.LeftHandGripping = IsGripping;
				break;

			default:
				// Set IsGripping to true while 'E' is held down; set to false when 'E' is not held
				IsGripping = Input.GetKey(KeyCode.RightShift);
				playerController.RightHandGripping = IsGripping;
				break;
		}
	}

	private void UpdateGripStrength()
	{
		// Accumulate time since last update
		TimeSinceLastDecay += Time.deltaTime;

		// Check if enough time has passed to update grip strength
		if (TimeSinceLastDecay >= GripInterval)
		{
			if (IsGripping)
			{
				CurrentGripStrength -= GripDecayRate;
				switch (IsLeftHand)
				{
					case true:
						if (playerController.LeftArmStrength <= 0.5) CurrentGripStrength += (playerController.LeftArmStrength * CurrentGripStrength);
						if (CurrentGripStrength >= MaxGripStrength) CurrentGripStrength = MaxGripStrength;
						if (CurrentGripStrength <= 0f) CurrentGripStrength = 0f;
						break;

					case false:
						if (playerController.RightArmStrength <= 0.5) CurrentGripStrength += (playerController.LeftArmStrength * CurrentGripStrength);
						if (CurrentGripStrength >= MaxGripStrength) CurrentGripStrength = MaxGripStrength;
						if (CurrentGripStrength <= 0f) CurrentGripStrength = 0f;
						break;
				}
				if (IsOnHold && CurrentGripStrength <= 0f)
				{
					Debug.Log("Hand has fallen off");
					switch (IsLeftHand)
					{
						case true:
							playerController.HandFallenOff(true);
							break;

						case false:
							playerController.HandFallenOff(false);
							break;
					}
				}
			}
			else
			{
				if (CurrentGripStrength <= MaxGripStrength)
				{
					CurrentGripStrength += GripRecoveryRate;
				}
			}

			TimeSinceLastDecay = 0f;
		}

		switch (IsLeftHand)
		{
			case true:
				playerController.LeftHandGrip = CurrentGripStrength;
				break;

			case false:
				playerController.RightHandGrip = CurrentGripStrength;
				break;
		}
	}

	private void SetUpLineRenderer()
	{
		// Set the number of points the line renderer will have
		lineRenderer.positionCount = 2;
		lineRenderer.startWidth = 0.4f;
		lineRenderer.endWidth = 0.35f;
		lineRenderer.numCapVertices = 2;
		lineRenderer.numCornerVertices = 2;
		lineRenderer.material = normalMaterial;
	}

	private void UpdateLineRenderer()
	{
		if (elbow != null)
		{
			// Set the positions of the line to be from elbow to hand
			lineRenderer.SetPosition(0, elbow.position);  // Start position (elbow)
			lineRenderer.SetPosition(1, transform.position); // End position (hand)
		}
	}

	private void UpdateArmColorBasedOnStretchAmount()
	{
		// Calculate the distance between the elbow and the hand
		float distanceFromElbow = Vector2.Distance(elbow.position, transform.position);

		// Change material based on distance thresholds
		if (distanceFromElbow >= maxHandDistance - .1)
		{
			// If the hand is at or beyond the maximum distance
			lineRenderer.material = maxDistanceMaterial;
		}
		else if (distanceFromElbow >= StretchedDist && distanceFromElbow < maxHandDistance)
		{
			// If the hand is stretched beyond the threshold
			lineRenderer.material = stretchedMaterial;
		}
		else
		{
			// If the hand is within the normal range
			lineRenderer.material = normalMaterial;
		}
	}

	private void UpdateTintBasedOnGripStrength()
	{
		// Calculate the percentage of grip strength
		float gripPercentage = CurrentGripStrength / MaxGripStrength;

		// Map the percentage to a color intensity, making it redder as the grip weakens
		Color tint = new Color(.8f, gripPercentage, gripPercentage);

		// Apply the color to the LineRenderer material
		this.GetComponent<SpriteRenderer>().color = tint;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Hold"))
		{
			if (IsGripping)
			{
				if (CurrentGripStrength > other.GetComponent<HoldController>().GripNeededForHold)
				{
					IsOnHold = true;
				}
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Hold"))
		{
			IsOnHold = false;
		}
	}
}