using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TelekineticTrainer : TrainerContext {

	[Header("Telekinetic Trainer Options")]
	public bool releaseAnimationsTraining = false;
	public bool releaseAnimationsPrediction = false;
	public bool RotateGroupTraining = false;
	public bool RotateGroupPredicting = false;
	public bool colorWhileFocused = false;

	private FloatingGroup floatingGroup;
	private HeadLock headLockScript;
	private bool interrupt = false;

	public bool UseVoiceOverTraining = true;
	public bool UseVoiceOverPrediction = true;

	public UnityEvent MidTrain;

	protected override void Awake()
	{
		base.Awake();
		headLockScript = GetComponent<HeadLock>();
		floatingGroup = GetComponentInChildren<FloatingGroup>();
		if (NoHeadsetDebugMode)
			currentState = TrainState.ReadyToPredict;
	}

	public FadeRoutine DemoInstruction;
	public IEnumerator demoRoutine()
	{
		TrainerTag focused = TagObjects[2];
		floatingGroup.gatherGroup();
		yield return new WaitForSecondsRealtime(floatingGroup.gatherTimeInSeconds);
		DemoInstruction.SetText("To Calibrate Neurable, Focus on the Indicated Object.");
		DemoInstruction.PulseFade(3);
		yield return new WaitForSecondsRealtime(.5f);

		focused.Focus();
		yield return new WaitForSecondsRealtime(2f);
		yield return new WaitForSeconds(focused.gameObject.GetComponentInParent<FloatingItem>().focusTime);

		DemoInstruction.SetText("Each Object will flash in sequence.\nFocus only on your Target.");
		DemoInstruction.PulseFade(4);
		yield return new WaitForSecondsRealtime(4f);

		float initDuration = TagObjects[0].flashDuration;
		float demoDuration = .5f;
		float demoDuration_Focus = 3.0f;
		foreach (TrainerTag t in TagObjects)
		{
			t.flashDuration = demoDuration;
		}
		TagObjects[2].flashDuration = demoDuration_Focus;

		TagObjects[0].Animate();
		yield return new WaitForSecondsRealtime(1.1f * demoDuration);
		TagObjects[1].Animate();
		yield return new WaitForSecondsRealtime(1.1f * demoDuration);
		TagObjects[3].Animate();
		yield return new WaitForSecondsRealtime(1.1f * demoDuration);
		TagObjects[2].Animate();
		DemoInstruction.SetText("Count each time the Focused Target flashes.");
		DemoInstruction.PulseFade(demoDuration_Focus);
		yield return new WaitForSecondsRealtime(demoDuration_Focus + 1f);
		DemoInstruction.SetText("Try to Count all 6 flashes.");
		DemoInstruction.PulseFade(3);
		foreach (TrainerTag t in TagObjects)
		{
			t.flashDuration = initDuration;
		}
		yield return new WaitForSecondsRealtime(2f);
		for (int i = 2; i <= numSequencesForTraining; i++)
		{
			shuffleGroups(focused.NeurableID);
			foreach (TrainerTag t in TagObjects)
			{
				t.Animate();
				yield return new WaitForSecondsRealtime(t.flashDuration + flashDelay);
			}
		}

		focused.Unfocus();
		focused.Trigger();
		yield return new WaitForSecondsRealtime(.5f);
		floatingGroup.releaseGroup();
		DemoInstruction.SetText("Get Ready to Calibrate.");
		DemoInstruction.PulseFade(2);
		yield return new WaitForSecondsRealtime(4f);
	}

	// Training Procedure Triggered by Parent
	protected override IEnumerator trainingCoroutine()
	{
		User.User.ClearEvents();
		didTrain = true;
		currentState = TrainState.Training;
		floatingGroup.RotateGroup = RotateGroupTraining;

		if (UseVoiceOverTraining)
		{
			yield return demoRoutine();
			yield return new WaitForSecondsRealtime(3f);
		}


		if (lockToHead) {
			headLockScript.lockToHead();
		}
		yield return new WaitForSecondsRealtime(1.5f);
		for (trialNumber = 1; trialNumber <= trainingOrder.Length; trialNumber++) {
			TrainerTag focused = TagObjects[trainingOrder[trialNumber - 1]];
			floatingGroup.gatherGroup();
			if (releaseAnimationsTraining || trialNumber == 1)
			yield return new WaitForSecondsRealtime(floatingGroup.gatherTimeInSeconds);
			yield return new WaitForSecondsRealtime(.5f);

			focused.Focus();
			yield return new WaitForSeconds(focused.gameObject.GetComponentInParent<FloatingItem>().focusTime);
			yield return new WaitForSecondsRealtime(.5f);
			for (sequenceNumber = 1; sequenceNumber <= numSequencesForTraining; sequenceNumber++) {
				shuffleGroups(focused.NeurableID);
				foreach (TrainerTag[] tempGroup in groups) {
					yield return flashGroup(tempGroup, focused.NeurableID);
					yield return new WaitForSecondsRealtime(flashDelay);
				}
			}
			focused.Unfocus();
			if (releaseAnimationsTraining)
			{
				focused.Trigger();
				yield return new WaitForSecondsRealtime(.5f);
				floatingGroup.releaseGroup();
				yield return new WaitForSecondsRealtime(4f);
			}
		}
		yield return new WaitForSeconds(2f);
		MidTrain.Invoke();
		TrainModel();
		UseVoiceOverTraining = false;
		floatingGroup.releaseGroup();
	}

	// Prediction Test Procedure triggered by parent
	protected override IEnumerator predictionCoroutine()
	{
		//instructionsString = "Use your Telekinesis!";
		floatingGroup.RotateGroup = RotateGroupPredicting;
		if (lockToHead) {
			headLockScript.lockToHead();
		}
		for (trialNumber = 1; trialNumber <= numPredictions; trialNumber++)
		{
			floatingGroup.gatherGroup();
			if (releaseAnimationsPrediction || trialNumber == 1)
			{
				User.User.ClearEvents();
				yield return new WaitForSecondsRealtime(floatingGroup.gatherTimeInSeconds);
			}
			yield return new WaitForSecondsRealtime(.5f);

			yield return runPredictionTrial();
			if (releaseAnimationsPrediction)
			{
				yield return new WaitForSecondsRealtime(2f);
				floatingGroup.releaseGroup();
				yield return new WaitForSecondsRealtime(3f);
			}
		}
		currentState = TrainState.ReadyToPredict;
		UseVoiceOverPrediction = false;
		floatingGroup.releaseGroup();
	}

	protected IEnumerator runPredictionTrial()
	{
		for (sequenceNumber = 1; sequenceNumber <= numSequencesForTraining; sequenceNumber++)
		{
			if (!interrupt)
			{
				shuffleGroups();
				foreach (TrainerTag[] tempGroup in groups)
				{
					yield return flashGroup(tempGroup);
					yield return new WaitForSecondsRealtime(flashDelay);
				}
			}
		}
		interrupt = false;
	}

	public override void addFooter()
	{
		base.addFooter();

		File.AppendAllText(filename, "Tested Scene: Telekinetic Trainer\n");
		if (didTrain)
			File.AppendAllText(filename, "Release Animations For Trainer: " + releaseAnimationsTraining + "\n");

		//else if (didPredict)
		//File.AppendAllText(filename, "Release Animations For Prediction: " + releaseAnimationsPrediction + "\n");

		File.AppendAllText(filename, "Headlocked: " + lockToHead + "\n");
		File.AppendAllText(filename, "Group Rotation: " + RotateGroupTraining + "\n");
		File.AppendAllText(filename, "Individual Rotation: " + floatingGroup.GetComponentInChildren<FloatingItem>().rotationOn + "\n");
		File.AppendAllText(filename, "Group Radius: " + floatingGroup.groupRadius + "\n");
		File.AppendAllText(filename, "---Notes From Operator---:\n" + notes + ":\n");
	}
}
