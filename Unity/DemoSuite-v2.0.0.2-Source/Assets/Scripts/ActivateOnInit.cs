using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActivateOnInit : MonoBehaviour {
	[Header("Used to Start First Scene and Activate proper UI Changes.")]
	public UnityEvent OnStart;
	public bool ActivateInEditor = false;
	// Use this for initialization
	void Start () {
		if (!Application.isEditor || ActivateInEditor)
			OnStart.Invoke();
	}
}
