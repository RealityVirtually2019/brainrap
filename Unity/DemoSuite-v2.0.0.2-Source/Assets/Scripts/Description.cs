using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Description : MonoBehaviour {
	public struct PanelText
	{
		public string SceneName;
		public string Title;
		public string Description;
	}
	public Text currentTitle;
	public Text currentDescription;

	private Dictionary<int, PanelText> TextPanels;
	
	void addTitlesAndDescriptions(){
		TextPanels = new Dictionary<int, PanelText>();

		PanelText General;
		General.SceneName = "PersistentScene";
		General.Title = "Neurable Tech Demo Suite";
		General.Description = "<i>Welcome!</i> Here we have a suite of demo scenes to help you get started building with Neurable! " +
			"to start: Import a <b>trained model</b> using the <b>import key</b> Once you have a model, " +
			"look at the <b>left TV</b> and use the <b>Vive controller</b> to select a scene using the <b>trigger button</b>.";
		TextPanels[0] = General;

		PanelText Spatial;
		Spatial.SceneName = "SpatialUI";
		Spatial.Title = "Neurable w/ Spatial UI";
		Spatial.Description = "One of the most basic ways to use Neurable is a controller replacement for Menu navigation.\n" +
			"Neurable can easily be added to basic Unity Buttons, extending your existing projects.";
		TextPanels[1] = Spatial;

		PanelText Diegetic;
		Diegetic.SceneName = "DiegeticUI";
		Diegetic.Title = "Neurable w/ Diegetic UI";
		Diegetic.Description = "This scene shows you how one might use Neurable to chain together different intreractions.\n" +
			"Note how selecting a target pulls up an action prompt.\n" +
			"This modality can be applied to navigating diegetic menus, seamlessly moving from UI interaction to in world action.";
		TextPanels[2] = Diegetic;

		PanelText Single;
		Single.SceneName = "SingleSelection";
		Single.Title = "Neurable Single Selection";
		Single.Description = "This scene shows you Neurable's ability to only select a single object when you actually want it.";

		PanelText Calibrate;
		Calibrate.SceneName = "Trainer";
		Calibrate.Title = "Neurable Calibration";
		Calibrate.Description = "First, calibrate Neurable's selection system.\n\nFocus your attention on the Indicated Object.\n" +
			"This Object will flash 4 times, try to count all 4 flashes.\n" +
			"Repeat with the next Indicated Object.\n\nAlternatively, Import a previously Trained Model with the Import button below.";
		TextPanels[3] = Calibrate;

		PanelText HighDensity;
		HighDensity.SceneName = "HighDensitySelection";
		HighDensity.Title = "Neurable High Density Selection";
		HighDensity.Description = "Neurable can help users select objects among a crowd.\n" +
			"Here, we direct flashing based on the Users eye movements, but final selections are made using the Brain.\n" +
			"This allows users to activate intended targets in clustered or distant areas.";
		TextPanels[4] = HighDensity;

		PanelText AffectiveUI;
		AffectiveUI.SceneName = "AffectiveSliders";
		AffectiveUI.Title = "Neurable Affective State";
		AffectiveUI.Description = "Neurable can Detect User Emotional State.\n" +
			"Overall Arousal can be organized into 4 main categories: Stress, Attention, Calm, and Fatigue.\n" +
			"The Overall Arousal is an indicator of where along that spectrum the user most likely falls";
		TextPanels[5] = AffectiveUI;

		PanelText FixationUI;
		FixationUI.SceneName = "Fixation";
		FixationUI.Title = "Neurable Fixation Engine";
		FixationUI.Description = "Neurable can be used to analyze a user's gaze.\n" +
			"By raycasting out, the Fixation Engine will record Fixation Events that can later\n" +
			"be used to reference Affective State.\n" +
			"Press 'Tab' during this scene to output a CSV file of all fixation events.";
		TextPanels[6] = FixationUI;
	}

	public void updateTitleAndDescription(string scene)
	{
		if (TextPanels == null) addTitlesAndDescriptions();
		foreach(PanelText Scene in TextPanels.Values)
		{
			if (Scene.SceneName == scene)
			{
				currentTitle.text = Scene.Title;
				currentDescription.text = Scene.Description;
			}
		}
	}
	public void updateTitleAndDescription(int scene)
	{
		if (TextPanels == null) addTitlesAndDescriptions();
		currentTitle.text = TextPanels[scene].Title;
		currentDescription.text = TextPanels[scene].Description;
	}
}
