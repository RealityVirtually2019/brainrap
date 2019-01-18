using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResponseButton : MonoBehaviour {

	public enum Response {FRIENDLY, MEAN};
	public Response response = Response.FRIENDLY;
	public Text input; 
	public Image backgroundPanel;

	public void MakeSelection(){
		backgroundPanel.enabled = true;
		if (response == Response.FRIENDLY){
			input.text = "You are so Kind! The Gold is All Yours!";
		}
		else{
			input.text = "I don't care! Get out of my face!";
		}
	}

	void OnDisable()
	{
		input.text = "";
		backgroundPanel.enabled = false;
	}
}
