using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public Transform LeftHand;
	public Transform RightHand;

	public Transform LeftElbow;
	public Transform RightElbow;

	public float HandMoveSpeed;
	public float MinMoveSpeed;

	public float MaxHandDistance;
	public float StretchDistance;

	public bool LeftHandIsStretching;
	public bool RightHandIsStretching;

	public float LeftArmStrength;
	public float RightArmStrength;

	public bool LeftHandGripping;
	public bool RightHandGripping;

	public bool CanGrab = true;
	public float FallResetSpeed;
	private Vector3 startPosLHand;
	private Vector3 startPosRHand;
	private Vector3 startPos;

	private void Start()
	{
		startPos = transform.position;
		startPosLHand = LeftHand.position;
		startPosRHand = RightHand.position;
	}

	private void Update()
	{
		MoveHands();
		LiftPlayer();
	}

	private void MoveHands()
	{
		// Get input for left hand (WASD controls)
		var leftHorizontal = Input.GetAxis("HorizontalLeft");
		var leftVertical = Input.GetAxis("VerticalLeft");
		Vector2 leftDirection = new Vector2(leftHorizontal, leftVertical).normalized;

		// Get input for right hand (arrow key controls)
		var rightHorizontal = Input.GetAxis("HorizontalRight");
		var rightVertical = Input.GetAxis("VerticalRight");
		Vector2 rightDirection = new Vector2(rightHorizontal, rightVertical).normalized;

		MoveHand(LeftHand, LeftElbow, leftDirection, true);
		UpdateArmStrength(LeftHand, LeftElbow, true);

		MoveHand(RightHand, RightElbow, rightDirection, false);
		UpdateArmStrength(RightHand, RightElbow, false);
	}

	private void MoveHand(Transform hand, Transform elbow, Vector2 direction, bool isLeftHand)
	{
		// don't move hands if they're gripping
		if (isLeftHand)
		{
			if (LeftHandGripping && LeftHand.GetComponent<HandManager>().IsOnHold) return;
		}
		else
		{
			if (RightHandGripping && RightHand.GetComponent<HandManager>().IsOnHold) return;
		}
		// Calculate the target position based on input
		Vector2 targetPosition = (Vector2)hand.position + direction * HandMoveSpeed * Time.deltaTime;

		// Calculate the distance from the elbow to the target position
		var distanceFromElbow = Vector2.Distance(elbow.position, targetPosition);

		// Check if user is stretching
		var currentMoveSpeed = HandMoveSpeed;
		if (distanceFromElbow >= StretchDistance)
		{
			// Slow down the movement as the hand gets farther from the elbow
			currentMoveSpeed = Mathf.Max(MinMoveSpeed, HandMoveSpeed * (1f - (distanceFromElbow - StretchDistance) / (MaxHandDistance - StretchDistance)));

			switch (isLeftHand)
			{
				case true:
					LeftHandIsStretching = true;
					break;

				case false:
					RightHandIsStretching = true;
					break;
			}
		}
		else
		{
			switch (isLeftHand)
			{
				case true:
					LeftHandIsStretching = false;
					break;

				case false:
					RightHandIsStretching = false;
					break;
			}
		}

		// If the hand is within the max distance, move it with the calculated speed
		if (distanceFromElbow < MaxHandDistance)
		{
			// Apply the movement speed adjustment
			targetPosition = (Vector2)hand.position + direction * currentMoveSpeed * Time.deltaTime;
			hand.position = targetPosition;  // Move the hand towards the target position
		}

		// Otherwise, constrain the hand to the max distance from the elbow
		else
		{
			Vector2 directionToElbow = (targetPosition - (Vector2)elbow.position).normalized;
			targetPosition = (Vector2)elbow.position + directionToElbow * MaxHandDistance;
			hand.position = targetPosition;
		}
	}

	/*
	 * Updates the arm strength by mapping it to the amount of distance from the elbow/aka stretch amount
	 */

	private void UpdateArmStrength(Transform hand, Transform elbow, bool isLeftHand)
	{
		var distanceFromElbow = Vector2.Distance(elbow.position, hand.position);
		// float mappedValue = ((value - minValue) / (maxValue - minValue)) * (NewMax - NewMin) + NewMin;
		var mappedValue = ((distanceFromElbow - 0.0f) / (MaxHandDistance - 0.0f)) * (1.0f - 0.0f) + 0.0f;
		mappedValue = 1.0f - mappedValue;
		switch (isLeftHand)
		{
			case true:
				LeftArmStrength = mappedValue;
				break;

			case false:
				RightArmStrength = mappedValue;
				break;
		}
	}

	public void HandFallenOff(bool isLeftHand)
	{
		switch (isLeftHand)
		{
			case true:
				if (RightHand.GetComponent<HandManager>().IsOnHold && RightHand.GetComponent<HandManager>().IsGripping)
				{
					Debug.Log("Right hand currently gripping, player hasn't fallen yet");
				}
				else
				{
					PlayerHasFallen();
				}
				break;

			case false:
				if (LeftHand.GetComponent<HandManager>().IsOnHold && LeftHand.GetComponent<HandManager>().IsGripping)
				{
					Debug.Log("Left hand currently gripping, player hasn't fallen yet");
				}
				else
				{
					PlayerHasFallen();
				}
				break;
		}
	}

	public void PlayerHasFallen()
	{
		StartCoroutine(ResetPlayerPosition());
		Debug.Log("Player fell");
	}

	private IEnumerator ResetPlayerPosition()
	{
		// Disable grabbing
		CanGrab = false;

		// Move player quickly to starting position
		float elapsedTime = 0f;
		Vector3 currentPosition = transform.position;
		Vector3 currentPositionL = LeftHand.transform.position;
		Vector3 currentPositionR = RightHand.transform.position;

		while (elapsedTime < 1f)
		{
			// Interpolate position back to starting point
			transform.position = Vector3.Lerp(currentPosition, startPos, elapsedTime * FallResetSpeed);
			RightHand.transform.position = Vector3.Lerp(currentPositionR, startPosRHand, elapsedTime * FallResetSpeed);
			LeftHand.transform.position = Vector3.Lerp(currentPositionL, startPosLHand, elapsedTime * FallResetSpeed);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		// Ensure exact starting position
		transform.position = startPos;
		RightHand.transform.position = startPosRHand;
		LeftHand.transform.position = startPosLHand;

		// Enable grabbing after a short delay
		yield return new WaitForSeconds(0.5f);
		CanGrab = true;
	}

	public void LiftPlayer()
	{
		// Only execute if both hands are gripping
		if (!LeftHand.GetComponent<HandManager>().IsOnHold || !RightHand.GetComponent<HandManager>().IsOnHold) return;

		// Set a threshold for distance to stop lifting and the height difference to consider hands "even"
		float stopDistanceThreshold = 2f;
		float evenLevelThreshold = 0.1f;

		// Check if the spacebar is held down
		if (Input.GetKey(KeyCode.Space))
		{
			// Calculate the vertical difference between hands
			float handHeightDifference = Mathf.Abs(RightHand.position.y - LeftHand.position.y);

			// Do nothing if hands are close to the same height
			if (handHeightDifference <= evenLevelThreshold)
			{
				Debug.Log("Hands are at an even level; not lifting.");
				return;
			}

			// Determine which hand is higher
			Transform targetHand = RightHand.position.y >= LeftHand.position.y ? RightHand : LeftHand;

			// Calculate the distance between the player and the target hand
			float distanceToTargetHand = Vector2.Distance(transform.position, targetHand.position);

			// Stop applying force if the player is within the stop distance threshold
			if (distanceToTargetHand <= stopDistanceThreshold)
			{
				Debug.Log("Player is close to the target hand; stopping lift.");
				return;
			}

			// If not within the stop distance, apply a force toward the target hand
			Rigidbody2D rg = GetComponent<Rigidbody2D>();
			rg.constraints = RigidbodyConstraints2D.None;
			rg.constraints = RigidbodyConstraints2D.FreezeRotation;

			// Calculate the direction towards the target hand
			Vector2 direction = (targetHand.position - transform.position).normalized;

			// Apply an upward force towards the target hand
			float liftForce = 5f;
			rg.AddForce(direction * liftForce, ForceMode2D.Force);

			Debug.Log("Player lifting towards the higher hand.");
		}

		// When the spacebar is released, freeze the player’s position
		if (Input.GetKeyUp(KeyCode.Space))
		{
			Rigidbody2D rg = GetComponent<Rigidbody2D>();
			rg.constraints = RigidbodyConstraints2D.FreezeAll;
		}
	}
}