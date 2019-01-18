/*
 * Simple script to rotate any object towards the player camera, at rotationSpeed
 * Note: Assumes NVRPlayer structure,
 * -- If that is not the case, use optionalCameraReference in the editor
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerLockUtility : MonoBehaviour {

	public float rotationSpeed = 30f;
	public Camera OPTIONALCameraReference;

	private Camera playerCamera;

	private void Awake()
	{
		if (OPTIONALCameraReference) {
			playerCamera = OPTIONALCameraReference;
		} else {
			playerCamera = GameObject.Find("Head").GetComponent<Camera>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Rotate) {
			float step = rotationSpeed * Time.deltaTime;
			Vector3 rotationDirection = playerCamera.transform.position - transform.position;
			Quaternion finalRotation = Quaternion.FromToRotation(Vector3.forward, rotationDirection);
      finalRotation = Quaternion.Euler(finalRotation.eulerAngles.x, finalRotation.eulerAngles.y, transform.rotation.z);
      transform.rotation = Quaternion.RotateTowards(transform.rotation, finalRotation, step);
      transform.localEulerAngles = rotationDirection;
		}
  }

	private bool rotationOn = true;
	public bool Rotate
	{
		get
		{
			return rotationOn;
		}
		set
		{
			rotationOn = value;
		}
	}
}
