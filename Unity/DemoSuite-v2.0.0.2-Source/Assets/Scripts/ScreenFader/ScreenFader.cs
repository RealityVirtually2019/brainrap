// This is free and unencumbered software released into the public domain.
// For more information, please refer to <http://unlicense.org/>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader instance;
    private const float kConstDefaultFadeTime = 2.0f;

    [Header("Material Fade")]
    public Color fadeColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    public Material fadeMaterial = null;


	[Header("Fade At Start")]
	public bool fadeInOnInit = true;
	public float delayFadeInInit = 1.0f;
	public float timeFadeInInit = 5.0f;

    public bool lastFadeIn = true;
    private List<ScreenFadeControl> fadeControls = new List<ScreenFadeControl>();


    #region Unity
    private void Awake()
    {
        instance = this;
		foreach (Camera c in Camera.allCameras) {
			var fadeControl = c.gameObject.GetComponent<ScreenFadeControl>();
			if (fadeControl == null) fadeControl = c.gameObject.AddComponent<ScreenFadeControl>();
			fadeControl.fadeMaterial = fadeMaterial;
			fadeControls.Add(fadeControl);
		}
	}

    private void Start()
    {
		if (fadeInOnInit) {
			FadeOut(0);
			LeanTween.delayedCall(delayFadeInInit, () =>
			{
				FadeIn(timeFadeInInit);
			});
		}
	}
    #endregion
    #region Public
    public void FadeIn(float fadeTime = kConstDefaultFadeTime)
    {
        Fade(true, fadeTime);
    }

    public void FadeOut(float fadeTime = kConstDefaultFadeTime)
    {
        Fade(false, fadeTime);
    }
    #endregion

    void SetFadersEnabled(bool value)
    {
        foreach (ScreenFadeControl fadeControl in fadeControls)
        {
            if (fadeControl)
                fadeControl.enabled = value;

        }
    }

    private IEnumerator coFadeOut(float fadeTime = kConstDefaultFadeTime)
    {
		// Derived from OVRScreenFade
		float elapsedTime = 0.0f;
        Color color = fadeColor;
        color.a = 0.0f;
        fadeMaterial.color = color;
		SetFadersEnabled(true);
        while (elapsedTime < fadeTime)
        {
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeTime);
            fadeMaterial.color = color;
        }
        color.a = 1.0f;
        fadeMaterial.color = color;     
    }

    private IEnumerator coFadeIn(float fadeTime = kConstDefaultFadeTime)
    {
		float elapsedTime = 0.0f;
        Color color = fadeColor;
		color.a = 1f;
		fadeMaterial.color = color;
		SetFadersEnabled(true);
        while (elapsedTime < fadeTime)
        {
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            color.a = 1.0f - Mathf.Clamp01(elapsedTime / fadeTime);
            fadeMaterial.color = color;
        }
        fadeMaterial.color = color;
        color.a = 0.0f;
        SetFadersEnabled(false);
    }
    private void Fade(bool fadeIn, float fadeTime = kConstDefaultFadeTime)
    {
		
        if (lastFadeIn != fadeIn)
        {
			lastFadeIn = fadeIn;
			StartCoroutine(DoFade(fadeIn, fadeTime));
        }
    }

    private IEnumerator DoFade(bool fadeIn = false, float fadeTime = kConstDefaultFadeTime)
    {
		//// Clean up from last fade
		//foreach (ScreenFadeControl fadeControl in fadeControls) {
		//	Destroy(fadeControl);
		//}
		//fadeControls.Clear();

		//// Find all cameras and add fade material to them (initially disabled)
		//foreach (Camera c in Camera.allCameras) {
		//	var fadeControl = c.gameObject.GetComponent<ScreenFadeControl>();
		//	if (fadeControl == null) fadeControl = c.gameObject.AddComponent<ScreenFadeControl>();
		//	fadeControl.fadeMaterial = fadeMaterial;
		//	fadeControls.Add(fadeControl);
		//}
		//// Do fade
		if (fadeIn) {
            yield return StartCoroutine(coFadeIn(fadeTime));
		} else {
            yield return StartCoroutine(coFadeOut(fadeTime));
		}

		
    }
}
