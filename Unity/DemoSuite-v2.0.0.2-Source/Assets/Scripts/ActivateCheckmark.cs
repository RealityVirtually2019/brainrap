using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ActivateCheckmark : MonoBehaviour {

	public KeyCode shortcutKey = KeyCode.Alpha1;
  public UnityEvent eventKey;
	public Color SelectedColor = Color.blue;
	static private Color InitialColor;
	static private Image previousCheckmark = null;

	// Update is called once per frame
	void Update()
    {
        if (Input.GetKeyDown(shortcutKey))
        {
			activate();
        }
    }

	public void activate()
	{
		if (previousCheckmark != null)
		{
			previousCheckmark.color = InitialColor;
		}
		Image currentCheckmark = GetComponentInChildren<Image>(true);
		InitialColor = currentCheckmark.color;
		currentCheckmark.color = SelectedColor;
		previousCheckmark = currentCheckmark;
		if (eventKey != null)
		{
			eventKey.Invoke();
		}
	}
}
