using System.Collections;
using Neurable.Interactions;
using UnityEngine;

public class Floater : MonoBehaviour {

	Rigidbody rb;
	ElicitorManager context;
	public float duration = 1f;
	public float scalar = 1f;
	public AnimationCurve YLift;
	public AnimationCurve YRotation;

	void Awake()
	{
		rb = GetComponentInChildren<Rigidbody>();
		//context = GetComponentInParent<ManualContext>();
	}

	void OnDisable()
	{
		if (levitation != null)
			StopCoroutine(levitation);
	}

	Coroutine levitation;
	public void StartLevitate(){
		if (context == null) context = GetComponentInParent<ElicitorManager>();
		if (context != null)
		{
			context.PauseAnim(duration);
			levitation = StartCoroutine(Levitate());
		}
		else
		{
			Debug.LogError("In Float StartLevitate(), there is no context to be paused");
		}
	}

	IEnumerator Levitate () {
		bool preserveGrav = rb.useGravity;
		Vector3 initialP = transform.localPosition;
		Vector3 initialR = transform.localEulerAngles;
		rb.useGravity = false;
		for (float t = 0.0f; t < duration; t += Time.deltaTime / 1) {
			Vector3 deltaPos = Vector3.zero;
			deltaPos.y = YLift.Evaluate(t / duration) * scalar;
			transform.localPosition = initialP + deltaPos;

			Vector3 deltaRot = Vector3.zero;
			deltaRot.y = YRotation.Evaluate(t / duration) * 360f;
			transform.localEulerAngles = initialR + deltaRot;
			yield return null;
		}
		transform.localPosition = initialP;
		transform.localEulerAngles = initialR;
		rb.useGravity = preserveGrav;
		yield return null;
	}
}
