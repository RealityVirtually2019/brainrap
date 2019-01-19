using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboutFace : MonoBehaviour {

	private Transform player;
	private Vector3 origpos;
	Vector3 origrot;
	public Vector3 _rotationAdjustment;
	public bool _lockXGimble = false, _lockYGimble = false, _lockZGimble = false;

	// Use this for initialization
	void Start () {
		player = Camera.main.transform;
		origpos = transform.position;
		origrot = transform.localEulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
		if (isActiveAndEnabled) {
			transform.LookAt (player);
			Vector3 worldAdjust = transform.eulerAngles;
			if (_lockXGimble)
			{
				worldAdjust.x = origrot.x + _rotationAdjustment.x;
			}
			else
			{
				worldAdjust.x += _rotationAdjustment.x;
			}

			if (_lockYGimble)
			{
				worldAdjust.y = origrot.y + _rotationAdjustment.y;
			}
			else
			{
				worldAdjust.y += _rotationAdjustment.y;
			}

			if (_lockZGimble)
			{
				worldAdjust.z = origrot.z + _rotationAdjustment.z;
			}
			else
			{
				worldAdjust.z += _rotationAdjustment.z;
			}
			transform.eulerAngles = worldAdjust;
		}
	}

	void OnDisable() {
		transform.position = origpos;
		transform.localEulerAngles = origrot;
	}
}
