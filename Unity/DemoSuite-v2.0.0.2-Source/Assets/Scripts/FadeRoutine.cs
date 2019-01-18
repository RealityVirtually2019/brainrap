using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeRoutine : MonoBehaviour {

	public Text TextToFade;
	public AnimationCurve Pulse;
	private Color baseColor;
	private void Start()
	{
		baseColor = TextToFade.color;
		SetOpacity(0);
	}
	private void SetOpacity(float a)
	{
		Color c = baseColor;
		c.a = a;
		TextToFade.color = c;
	}
	Coroutine pulser;
	public void PulseFade(float peakTime = 0)
	{
		if (pulser != null) StopCoroutine(pulser);
		pulser = StartCoroutine(PulseRoutine(peakTime));
	}

	protected IEnumerator PulseRoutine(float peakTime = 0)
	{
		yield return PulseUp_R();
		yield return new WaitForSeconds(peakTime);
		yield return PulseDown_R();
	}

	public void PulseUP()
	{
		if (pulser != null) StopCoroutine(pulser);
		pulser = StartCoroutine(PulseUp_R());
	}
	protected IEnumerator PulseUp_R()
	{
		float timer = 0;
		SetOpacity(0);
		while (timer < Pulse.keys[Pulse.length - 1].time)
		{
			SetOpacity(Pulse.Evaluate(timer));
			timer += Time.deltaTime;
			yield return null;
		}
		yield return null;
	}
	public void PulseDown()
	{
		if (pulser != null) StopCoroutine(pulser);
		pulser = StartCoroutine(PulseDown_R());
	}
	protected IEnumerator PulseDown_R()
	{
		float timer = Pulse.keys[Pulse.length - 1].time;
		while (timer > 0)
		{
			SetOpacity(Pulse.Evaluate(timer));
			timer -= Time.deltaTime;
			yield return null;
		}
		SetOpacity(0);
		yield return null;
	}

	public void SetText(string txt)
	{
		TextToFade.text = txt;
	}
}
