using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public Transform LeftHand;
	public Transform RightHand;

	public Transform leftHandLiftLoc;
	public Transform rightHandLiftLoc;

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

	public float LeftHandGrip;
	public float RightHandGrip;

	public bool CanGrab = true;
	public float FallResetSpeed;
	private Vector3 startPos;

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
					break;

				case false:
					targetPosition = new Vector2(Mathf.Max(targetPosition.x, elbow.position.x), targetPosition.y);
					break;
			}

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
					Debug.Log("Right hand currently gripping, player hasn't fallen yet");
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

		while (elapsedTime < 1f)
		{
			// Interpolate position back to starting point
			transform.position = Vector3.Lerp(currentPosition, startPos, elapsedTime * FallResetSpeed);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		// Ensure exact starting position
		transform.position = startPos;

		// Enable grabbing after a short delay
		yield return new WaitForSeconds(0.5f);
		CanGrab = true;
	}

	public void LiftPlayer()
	{
		//if (LeftHand.GetComponent<HandManager>().IsOnHold && RightHand.GetComponent<HandManager>().IsOnHold)
		//{
		if (Input.GetKey(KeyCode.Space))
		{
			Rigidbody2D rg = GetComponent<Rigidbody2D>();
			rg.constraints = RigidbodyConstraints2D.None;
			rg.constraints = RigidbodyConstraints2D.FreezeRotation;

			if (Vector2.Distance(RightHand.position, LeftHand.position) >= 2 * MaxHandDistance) return;
			var reelSpeed = 5f;
			if (RightHand.position.y > LeftHand.position.y)
			{
				var springJointRight = RightHand.GetComponent<SpringJoint2D>();
				var distanceRight = springJointRight.distance - reelSpeed * Time.deltaTime;
				springJointRight.distance = distanceRight;
			}
			else if (LeftHand.position.y > RightHand.position.y)
			{
				var springJointLeft = LeftHand.GetComponent<SpringJoint2D>();
				var distanceLeft = springJointLeft.distance - reelSpeed * Time.deltaTime;
				springJointLeft.distance = distanceLeft;
			}
			Debug.Log("Player lifting");
		}
		else
		{
			Rigidbody2D rg = GetComponent<Rigidbody2D>();
			rg.constraints = RigidbodyConstraints2D.FreezeAll;
		}
		//}
	}
}