using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadLock : MonoBehaviour {

	public Vector3 EndPosition = new Vector3(0.9745493f, -1.23993f, 2.250668f); // relative to head
	public Vector3 EndRotation = new Vector3(5.563f, -114.535f, 11.99f); // relative to head
	Transform head;

	// Use this for initialization
	void Start () {
		head = GameObject.Find("Head").transform;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void lockToHead()
	{
		transform.SetParent(head);
		StopCoroutine (lockToHeadCoroutine ());
		StartCoroutine(lockToHeadCoroutine());
	}

	IEnumerator lockToHeadCoroutine()
	{
		float StartTime = Time.time;
		float t = 0;
		Vector3 initialPosition = transform.localPosition;
		Quaternion initialRotation = transform.localRotation;
		while (t < 1) {
			t = (Time.time - StartTime) / 1f;
			transform.localPosition = Vector3.Lerp(initialPosition, EndPosition, t);
			transform.localRotation = Quaternion.Lerp(initialRotation, Quaternion.Euler(EndRotation), t);
			yield return null;
		}
		transform.localPosition = EndPosition;
		transform.localEulerAngles = EndRotation;
		yield return null;
	}
}
