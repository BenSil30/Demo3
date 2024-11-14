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

	public bool IsOnHold = false;
	public bool IsGripping = false;

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
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			if (IsLeftHand)
			{
				IsGripping = !IsGripping;
				if (!IsGripping) IsOnHold = false;
				playerController.LeftHandGripping = IsGripping;
			}
		}
		if (Input.GetKeyDown(KeyCode.RightShift))
		{
			if (!IsLeftHand)
			{
				IsGripping = !IsGripping;
				if (!IsGripping) IsOnHold = false;
				playerController.RightHandGripping = IsGripping;
			}
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
				if (IsOnHold && CurrentGripStrength <= 0f)
				{
					Debug.Log("Hand has fallen off");
					IsGripping = false;
					IsOnHold = false;
					switch (IsLeftHand)
					{
						case true:
							playerController.HandFallenOff(true);
							playerController.LeftHandGripping = IsGripping;
							break;

						case false:
							playerController.HandFallenOff(false);
							playerController.RightHandGripping = IsGripping;
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

		if (CurrentGripStrength > MaxGripStrength) CurrentGripStrength = MaxGripStrength;
		if (CurrentGripStrength <= 0f) CurrentGripStrength = 0f;
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

	private void OnTriggerStay2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Hold"))
		{
			if (IsGripping)
			{
				Debug.Log("here");
				if (CurrentGripStrength > other.GetComponent<HoldController>().GripNeededForHold)
				{
					other.GetComponent<HoldController>().IsChalked = true;
					IsOnHold = true;

					switch (IsLeftHand)
					{
						//todo: properly store and reset the hold variables after modifying them
						case true:
							if (playerController.LeftHandIsStretching)
							{
								other.GetComponent<HoldController>().GripNeededForHold -=
									other.GetComponent<HoldController>().GripNeededForHold * playerController.LeftArmStrength;
								if (other.GetComponent<HoldController>().GripNeededForHold <= 0f) other.GetComponent<HoldController>().GripNeededForHold = 0f;
							}
							else
							{
								other.GetComponent<HoldController>().GripNeededForHold = other.GetComponent<HoldController>().GripTemp;
							}
							break;

						case false:
							if (playerController.RightHandIsStretching)
							{
								other.GetComponent<HoldController>().GripNeededForHold -=
									other.GetComponent<HoldController>().GripNeededForHold *
									playerController.RightArmStrength;
								if (other.GetComponent<HoldController>().GripNeededForHold <= 0f)
									other.GetComponent<HoldController>().GripNeededForHold = 0f;
							}
							else
							{
								other.GetComponent<HoldController>().GripNeededForHold =
									other.GetComponent<HoldController>().GripTemp;
							}
							break;
					}
				}
			}
			else
			{
				IsOnHold = false;
				Debug.Log("nope here");
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Hold"))
		{
			IsOnHold = false;
			other.GetComponent<HoldController>().GripNeededForHold = other.GetComponent<HoldController>().GripTemp;
		}
	}
}