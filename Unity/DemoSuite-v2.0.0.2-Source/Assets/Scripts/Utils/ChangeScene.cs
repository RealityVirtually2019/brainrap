using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Neurable.Interactions;

public class ChangeScene : MonoBehaviour
{
  public static ChangeScene instance;
  public float timeTransition = 3.0f;
  public bool changing = false;
	public bool init = true;
	public GameObject descriptionPanel;

	private ElicitorManager context;

	public enum SCENES { Persistent, Spatial, Diegetic, Trainer, HighDensity, Affective, Fixation };
	private Dictionary<SCENES, string> SceneDict = new Dictionary<SCENES, string>() {
		{SCENES.Persistent, "PersistentScene"},
		{SCENES.Spatial, "SpatialUI"},
		{SCENES.Diegetic, "DiegeticUI"},
		{SCENES.Trainer, "Trainer"},
		{SCENES.HighDensity, "HighDensitySelection"},
		{SCENES.Affective, "AffectiveSliders"},
		{SCENES.Fixation, "Fixation"}
	};

	// [Header("Initialization Options")]
	private bool loadInitialScene = false;
	private SCENES InitialScene = SCENES.Trainer;

	private void Awake()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void Start()
  {
    instance = this;
		if (loadInitialScene && (SceneManager.sceneCount < 2 ))
		{
			LoadScene(timeTransition, InitialScene);
		}
	}

  public void LoadScene(int sceneNum)
	{
		LoadScene(timeTransition, (SCENES)sceneNum);
	}
  public void LoadScene(float time, SCENES scene)
	{
		if (SceneManager.sceneCount > 1) {
			LeanTween.delayedCall(time + .05f, () =>
			{
				for (int i = 1; i < SceneManager.sceneCount; ++i)
				{
					SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i).buildIndex);
				}
			});
		}
		LeanTween.delayedCall(time + 0.15f, () =>
    {
			SceneManager.LoadScene(SceneDict[scene], LoadSceneMode.Additive);
    });
  }

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (init) {
			init = false;
		} else {
			LeanTween.delayedCall(1f, () =>
			{
				if (ScreenFader.instance)
					ScreenFader.instance.FadeIn(timeTransition);
				changing = false;
			});
		}
	}
}
