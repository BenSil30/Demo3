using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
	public Transform elbow;            // Reference to the corresponding elbow
	public LineRenderer lineRenderer;  // The LineRenderer component attached to the hand
	public Material normalMaterial;    // Material when the hand is within normal distance
	public Material stretchedMaterial; // Material when the hand is stretched beyond a certain distance
	public Material maxDistanceMaterial; // Material when the hand is at the maximum distance

	public PlayerController playerController;
	public float maxHandDistance; // Maximum distance the hand can be from the elbow
	public float StretchedDist; // Threshold distance where material changes

	private void Start()
	{
		// Ensure the LineRenderer component is attached
		if (lineRenderer == null)
		{
			lineRenderer = GetComponent<LineRenderer>();
		}

		// Initialize LineRenderer settings
		SetUpLineRenderer();

		maxHandDistance = playerController.MaxHandDistance;
		StretchedDist = playerController.StretchDistance;
	}

	private void Update()
	{
		// Update the LineRenderer to draw the line between elbow and hand
		UpdateLineRenderer();

		// Update material based on the distance
		UpdateMaterialBasedOnDistance();
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

	private void UpdateMaterialBasedOnDistance()
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
}