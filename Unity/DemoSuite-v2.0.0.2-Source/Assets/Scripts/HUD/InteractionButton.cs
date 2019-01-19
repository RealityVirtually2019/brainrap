using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionButton : MonoBehaviour {

	public enum Interaction {OPEN, CLOSE};
	public Interaction interaction = Interaction.OPEN;
	public Text input; 
	public Image backgroundPanel;

	public void MakeSelection(){
		backgroundPanel.enabled = true;
		if (interaction == Interaction.OPEN){
			input.text = "You opened the chest!";
		}
		else{
			input.text = "You closed the chest!";
		}
	}

	void OnDisable()
	{
		input.text = "";
		backgroundPanel.enabled = false;
	}
}
