using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleCurve : MonoBehaviour {
	public AnimationCurve animCurve;
	public float duration = 0.75f;
	Vector3 initialScale;
	bool state = false;

	private void Awake() {
		initialScale = transform.localScale;
	}

	Coroutine lerp;
	public void Open()
	{
		if (state) return;
		state = true;
		gameObject.SetActive(true);
		if (lerp != null) StopCoroutine(lerp);
		lerp = StartCoroutine(lerpScale(0, 1, true));
	}

	public void Close()
	{
		if (!state) return;
		state = false;
		if (lerp != null) StopCoroutine(lerp);
		if (isActiveAndEnabled)
			lerp = StartCoroutine(lerpScale(1, 0, false));
	}

	float timer = 0;
	IEnumerator lerpScale(float start, float finish, bool activeAtEnd)
	{
		timer = 0;
		Scale(start);
		while (timer < duration)
		{
			Scale(animCurve.Evaluate(timer / duration));
			timer += Time.deltaTime;
			yield return null;
		}
		Scale(finish);
		gameObject.SetActive(activeAtEnd);
		yield return null;
	}

	public void Scale(float factor)
	{
		transform.localScale = initialScale * factor;
	}

	public void OnEnable()
	{
		Open();
	}
	public void OnDisable()
	{
		Close();
	}

}
