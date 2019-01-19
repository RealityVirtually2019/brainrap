using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour {

	public Text input; 

	public void MakeSelection(){
		input.text = "You selected:\n" + this.name;
		Invoke("ResetText", 3);
	}

	void OnDisable()
	{
		ResetText();
	}

	private void ResetText()
	{
		input.text = "";
	}
}
