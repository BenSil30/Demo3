using TMPro;
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

	public float LeftHandGrip;
	public float RightHandGrip;

	private void Update()
	{
		MoveHands();
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
		UpdateHandGripStrength(LeftHand, LeftElbow, true);

		MoveHand(RightHand, RightElbow, rightDirection, false);
		UpdateArmStrength(RightHand, RightElbow, false);
		UpdateHandGripStrength(RightHand, RightElbow, false);
	}

	/*
	 * Updates the arm strength by mapping it to the amount of distance from the elbow/aka stretch amount
	 */

	private void UpdateArmStrength(Transform hand, Transform elbow, bool isLeftHand)
	{
		var distanceFromElbow = Vector2.Distance(elbow.position, hand.position);
		// float mappedValue = ((value - minValue) / (maxValue - minValue)) * (NewMax - NewMin) + NewMin;
		var mappedValue = ((distanceFromElbow - 0.0f) / (MaxHandDistance - 0.0f)) * (1.0f - 0.0f) + 0.0f;
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

	/*
	 * todo: Updates the hand grip strength by mapping it to TBD, also depends on if it's stretching but not by the amount of stretch
	 */

	private void UpdateHandGripStrength(Transform hand, Transform elbow, bool isLeftHand)
	{
		switch (isLeftHand)
		{
			case true:
				if (LeftHandIsStretching)
				{
					LeftHandGrip = 0.5f;
				}
				else
				{
					LeftHandGrip = 1.0f;
				}
				break;

			case false:
				if (RightHandIsStretching)
				{
					RightHandGrip = .5f;
				}
				else
				{
					RightHandGrip = 1.0f;
				}
				break;
		}
	}

	private void MoveHand(Transform hand, Transform elbow, Vector2 direction, bool isLeftHand)
	{
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

			switch (isLeftHand)
			{
				// Apply side constraint: prevent the left hand from crossing the right side of the left elbow
				case true:
					targetPosition = new Vector2(Mathf.Min(targetPosition.x, elbow.position.x), targetPosition.y);
					break;
				// Apply side constraint: prevent the right hand from crossing the left side of the right elbow
				case false:
					targetPosition = new Vector2(Mathf.Max(targetPosition.x, elbow.position.x), targetPosition.y);
					break;
			}

			hand.position = targetPosition;  // Move the hand towards the target position
		}

		// Otherwise, constrain the hand to the max distance from the elbow
		else
		{
			Vector2 directionToElbow = (targetPosition - (Vector2)elbow.position).normalized;
			targetPosition = (Vector2)elbow.position + directionToElbow * MaxHandDistance;

			switch (isLeftHand)
			{
				// Apply side constraint again on constrained hand position
				case true:
					targetPosition = new Vector2(Mathf.Min(targetPosition.x, elbow.position.x), targetPosition.y);
					LeftArmStrength = 0.0f;
					break;

				case false:
					targetPosition = new Vector2(Mathf.Max(targetPosition.x, elbow.position.x), targetPosition.y);
					RightArmStrength = 0.0f;
					break;
			}

			hand.position = targetPosition;
		}
	}
}