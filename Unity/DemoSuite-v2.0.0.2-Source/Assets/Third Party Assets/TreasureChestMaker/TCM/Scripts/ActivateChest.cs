using UnityEngine;
using System.Collections;

public class ActivateChest : MonoBehaviour {

	public Transform lid, lidOpen, lidClose;  // Lid, Lid open rotation, Lid close rotation
	public float speed = 5f;
	public bool canClose = true;           // Can the chest be closed
	public bool canOpen = true;           // Can the chest be opened
	public AnimationCurve OpenCurve;
	
	[HideInInspector]
	public bool _open = false;							// Is the chest opened
	
	// Rotate the lid to the requested rotation
	IEnumerator RotateLid(Quaternion toRot){
		float t = 0f;
		Quaternion orig = lid.rotation;
		while (t <= 1f && lid.rotation != toRot) {
			lid.rotation = Quaternion.Lerp(orig, toRot, OpenCurve.Evaluate(t));
			t += Time.deltaTime * speed;
			yield return null;
		}
		yield return null;
	}

	public void OpenChest()
	{
		if (canOpen && !_open)
		{
			StartCoroutine(RotateLid(lidOpen.rotation));
			_open = true;
		}
	}
	public void CloseChest()
	{
		if (canClose && _open)
		{
			StartCoroutine(RotateLid(lidClose.rotation));
			_open = false;
		}
	}
}
