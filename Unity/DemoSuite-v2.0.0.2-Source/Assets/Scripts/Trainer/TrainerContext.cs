/* an underived context class for the purpose of manual controlling flashing
 * and registration. All of the game loop happens here and is called from trainerUserReady
 */

using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Neurable.Core;

public class TrainerContext : MonoBehaviour
{
	[Header("Notes for data dump")]
	public string notes;

	[Header("Testing Parameters")]
	public float flashDelay = 0.1f;
	public uint numTrainingTrials = 6;
	public int numSequencesForTraining = 5;
	public int numPredictions = 10;
	public bool lockToHead = false;

	[Header("On Completion")]
	public UnityEvent OnTrainSuccess;
	public UnityEvent OnTrainFail;

	[Header("Navigation")]
	public KeyCode KeytoTrain = KeyCode.T;
	public KeyCode KeytoPredict = KeyCode.P;
	public KeyCode KeytoToggleEyetracker = KeyCode.O;

	[Header("Associated Tags (Read Only)")]
	public List<TrainerTag> TagObjects;
	protected List<TrainerTag[]> groups;
	[SerializeField]
	protected int[] trainingOrder;

	[Header("Debug Mode")]
	public bool NoHeadsetDebugMode = false;

	public enum TrainState { NotReady, ReadyToTrain, Training, TrainFail, TrainComplete, ReadyToPredict, Predicting };
	public TrainState currentState = TrainState.NotReady;

	protected bool isAnimating = false;
	protected int trialNumber = 1;
	protected int sequenceNumber = 1;

	[HideInInspector]
	public bool hasQuit = false;

	protected bool didTrain = false;
	protected bool didPredict = false;
	protected string filename = "raw_data.csv";

	protected TrainerTag[] lastFlashed;
	protected System.Random RNG;
  protected float HRES, VRES;

	delegate void UpdatePositionCast(bool forceRegistration = false);
	UpdatePositionCast UpdatePositions;

	#region Awake & Update
	protected virtual void Awake()
	{
		gatherTags();
		trainingOrder = new int[numTrainingTrials];
		RNG = new System.Random();
		for (int i = 0; i < numTrainingTrials; i++)
		{
				trainingOrder[i] = RNG.Next() % TagObjects.Count;
		}
	}

	protected virtual void Start()
	{
		ResetMetadata();
	}

	public virtual void Update()
	{
		if (currentState == TrainState.NotReady && User.Ready)
		{
			currentState = TrainState.ReadyToTrain;
		}
		if ((currentState == TrainState.NotReady && NoHeadsetDebugMode) || 
				((currentState == TrainState.ReadyToTrain && User.User.HasModel())))
		{
			currentState = TrainState.ReadyToPredict;
		}
		if (currentState == TrainState.TrainComplete)
		{
			OnTrainSuccess.Invoke();
			currentState = TrainState.ReadyToPredict;
		}
		if (currentState == TrainState.TrainFail)
		{
			OnTrainFail.Invoke();
			currentState = TrainState.ReadyToTrain;
		}


		if (Input.GetKeyDown(KeytoTrain))
		{
			startTraining();
		}
		if (Input.GetKeyDown(KeytoPredict))
		{
			startPrediction();
		}
		if (user != null && Input.GetKeyDown(KeytoToggleEyetracker))
		{
			user.UseHybridEyeSystem = !user.UseHybridEyeSystem;
		}
	}

	#endregion

	#region User Access

	// Search scene for Users using Tag Specifier
	private NeurableUser user;
	protected NeurableUser User
	{
		get
		{
			if (user == null) user = FindObjectOfType<NeurableUser>();
			if (user == null)
				throw new MissingComponentException("No NeurableUser Found");
			return user;
		}
	}
	protected void ImportModel()
	{
		if (User.ImportModel())
		{
			currentState = TrainState.ReadyToPredict;
		} else {
			Debug.LogError("User failed to import model");
		}
	}
	protected void ExportModel()
	{
		if (!User.ExportModel())
		{
			Debug.LogError("User failed to import model");
		}
	}
	protected void ToggleEyes()
	{
		if (User == null)
		{
			Debug.LogError("User failed to Toggle Eyetracker");
		}
		User.UseHybridEyeSystem = !User.UseHybridEyeSystem;
	}
	#endregion

	#region TrainerTag Handling
	// Search hierarchy for NeurableTag Objects. Only looks through children
	public void gatherTags()
	{
			groups = new List<TrainerTag[]>();
			TrainerTag[] tagobjs = GetComponentsInChildren<TrainerTag>(true);
			addTags(tagobjs);
			createGroups_singleton();
	}

	/*
	 * From Manual Context
	 */
	// Override tag addition to siphon off some data, make tags visible
	public void addTags(TrainerTag[] t)
	{
		if (t.Length > 0)
		{
			if (TagObjects == null)
				TagObjects = new List<TrainerTag>();

			Neurable.API.Tag[] nt = new Neurable.API.Tag[t.Length];
			for (int i = 0; i < t.Length; ++i)
			{
				TrainerTag tobj = t[i];
				tobj.NeurableEnabled = true;
				UpdatePositions += tobj.UpdatePosition;
				if (!TagObjects.Contains(tobj))
					TagObjects.Add(tobj);

				nt[i] = tobj.NeuroTag;
			}
		}
	}

	// Override tag addition to siphon off some data
	public void removeTags(TrainerTag[] t)
	{
		if (TagObjects != null && TagObjects.Count > 0)
		{
			foreach (TrainerTag tobj in t)
			{
				tobj.NeurableEnabled = false;
				UpdatePositions -= tobj.UpdatePosition;
				TagObjects.Remove(tobj);
			}
		}
	}
	#endregion

	#region Group Assignment/Handling
	protected void createGroups_singleton()
	{
			foreach (TrainerTag t in TagObjects)
			{
			TrainerTag[] g = { t };
					groups.Add(g);
			}
	}

	protected void shuffleGroups(int focusedID = -1)
	{
		if (groups == null || groups.Count < 1)
			return;

		//swap each index with a random position
		for(int i = 0; i < groups.Count; i++) {
			int indexSwap = RNG.Next(groups.Count);
			TrainerTag[] temp = groups[i];
			groups[i] = groups[indexSwap];
			groups[indexSwap] = temp;
		}

		//check if focused is in first index
		bool isFocusedFirst = false;
		foreach (TrainerTag _tag in groups[0]) {
			if (_tag.NeurableID == focusedID) {
				isFocusedFirst = true;
				break;
			}
		}
		//and if so, swap it with random index
		if (isFocusedFirst) {
			int indexSwap = RNG.Next(1, groups.Count - 1);
			TrainerTag[] temp = groups[0];
			groups[0] = groups[indexSwap];
			groups[indexSwap] = temp;
		}

		//check if the first group is == to the last group of the previous sequence. if so, swap it to the end.
		if (lastFlashed != null && groupsEqual(lastFlashed, groups[0])) {
			TrainerTag[] temp = groups[0];
			groups[0] = groups[groups.Count-1];
			groups[groups.Count-1] = temp;
		}

		lastFlashed = groups[groups.Count - 1];
	}

	// Checks if 2 groups share tags to avoid double flashing.
	private bool groupsEqual(TrainerTag[] g1, TrainerTag[] g2)
	{
		if (g1.Length != g2.Length)
		{
			return false;
		}

		foreach (TrainerTag t in g1)
		{
			int ID = t.NeurableID;
			bool tagInG2 = false;
			foreach (TrainerTag t2 in g2)
			{
				if (t2.NeurableID == ID)
				{
					tagInG2 = true;
					break;
				}
			}
			if (!tagInG2)
			{
				return false;
			}
		}
		return true;
	}
	#endregion

	#region Metadata
	void ResetMetadata()
	{
		User.Metadata.SetMetadata("Trial", 0, false);
		User.Metadata.SetMetadata("Num_Sequences", numSequencesForTraining, false);
		User.Metadata.SetMetadata("Sequence", 0, false);
		User.Metadata.SetMetadata("Tag", 0, false);
		User.Metadata.SetMetadata("Label", 0, false);
		User.Metadata.SetMetadata("Flash_Event", 0, true);
	}
	#endregion

	#region Flash Logic
	// Trigger the animation for a group of tags and register the event.
	// Param target: if an object is focused in this trial, groups matching this Tag ID will be labelled 1, others 2. If target == -1, label 0
	// Param registerPostFlash: When true, Trigger registration after flash duration has completed.
	protected IEnumerator flashGroup(TrainerTag[] group, int target = -1, bool registerPostFlash = false)
	{
		if (group.Length == 0)
			yield break;

		if (UpdatePositions != null) UpdatePositions();
		var tags = new Neurable.API.Tag[group.Length];
		int i = 0;
		int label = 2;
		foreach (TrainerTag b in group)
		{
			b.Animate();
			if (target != -1)
			{
				if (b.NeurableID == target)
				{
					label = 1;
				}
			}
			else
			{
				label = 0;
			}
			tags[i++] = b.NeuroTag;
		}

		if (registerPostFlash)
		{
			yield return new WaitForSeconds(group[0].flashDuration);
			RegisterGroupFlashEvent(tags, label);
		}
		else
		{
			RegisterGroupFlashEvent(tags, label);
			yield return new WaitForSeconds(group[0].flashDuration);
		}
		ResetMetadata();
	}

	void RegisterGroupFlashEvent(Neurable.API.Tag[] tagGroup, int label)
	{
		string[] group = new string[tagGroup.Length];
		for (int i = 0; i < tagGroup.Length; ++i)
		{
			Neurable.API.Tag t = tagGroup[i];
			group[i] = t.GetPointer().ToString();
		}
		string groupStr = "[" + string.Join(" ", group) + "]";
		User.Metadata.SetMetadata("Trial", trialNumber, false);
		User.Metadata.SetMetadata("Sequence", sequenceNumber, false);
		User.Metadata.SetMetadata("Tag", groupStr, false);
		User.Metadata.SetMetadata("Label", label, false);
		User.Metadata.SetMetadata("Flash_Event", 1, true);

		switch (label)
		{
			case 1:
				User.User.RegisterTrainingEvent(tagGroup, true);
				break;
			case 2:
				User.User.RegisterTrainingEvent(tagGroup, false);
				break;
			case 0:
				User.User.RegisterEvent(tagGroup);
				break;
		}
	}

	//New Functions
	protected IEnumerator RegisterNullEvent(TrainerTag[] group)
	{
		Neurable.API.Tag[] tags = new Neurable.API.Tag[group.Length];
		int i = 0;
		foreach (TrainerTag b in group)
		{
			tags[i++] = b.NeuroTag;
		}
		RegisterGroupFlashEvent(tags, 2);
		yield return new WaitForSeconds(group[0].flashDuration);
		ResetMetadata();
	}
	#endregion

	#region Training
	public void enableTraining() { if (currentState == TrainerContext.TrainState.NotReady) currentState = TrainState.ReadyToTrain; }
	public void startTraining()
	{
		if (currentState == TrainState.Training) return;
		if (currentState != TrainState.ReadyToTrain && currentState != TrainState.ReadyToPredict)
			return;

		User.User.SetCalibrationSequences(numSequencesForTraining);
		currentState = TrainState.Training;
		StartCoroutine(trainingCoroutine());
		HRES = User.GetComponent<NeurableCamera>().hRes;
		VRES = User.GetComponent<NeurableCamera>().vRes;
	}

	protected virtual IEnumerator trainingCoroutine()
	{
		yield return null;
	}

	Thread TrainingThread;
	public virtual void TrainModel()
	{
		if (TrainingThread != null && TrainingThread.ThreadState != ThreadState.Unstarted)
		{
			Debug.LogWarning("Training Thread Occupied, aborting previous call and restarting");
			TrainingThread.Abort();
		}
		TrainingThread = new Thread(ThreadedTrain);
		TrainingThread.Start();
	}

	private void ThreadedTrain()
	{
		if (NoHeadsetDebugMode)
		{
			currentState = TrainState.TrainComplete;
			return;
		}

		if (User == null || !User.User.Calibrate())
		{
			currentState = TrainState.TrainFail;
			return;
		}
		ExportModel();
		currentState = TrainState.TrainComplete;
	}
	#endregion

	#region Prediction Testing
	public void startPrediction()
	{
		if (currentState == TrainState.Predicting) return;
		if (currentState != TrainState.ReadyToPredict) return;
		User.EnableMentalSelect = true;
		currentState = TrainState.Predicting;
		StartCoroutine(predictionCoroutine());
	}

	protected virtual IEnumerator predictionCoroutine()
	{
		yield return null;
	}
	#endregion

	#region Destruction
	// Append context information to the given data file
	public virtual void addFooter()
	{
		if (!hasQuit) {
			hasQuit = true;
			if (TrainingThread != null && TrainingThread.ThreadState == ThreadState.Running) TrainingThread.Abort();

			File.AppendAllText(filename, "Intended Tags:");
			for (int num = 0; num < trainingOrder.Length - 1; num++) {
				int training = trainingOrder[num];
				File.AppendAllText(filename, (TagObjects[training].NeurableID) + ",");
			}
			int training2 = trainingOrder[trainingOrder.Length - 1];
			File.AppendAllText(filename, (TagObjects[training2].NeurableID) + "\n");

			File.AppendAllText(filename, "Resulting Tags:");
			for (int num = 0; num < trainingOrder.Length - 1; num++) {
				int training = trainingOrder[num];
				File.AppendAllText(filename, (TagObjects[training].NeurableID) + ",");
			}
			File.AppendAllText(filename, (TagObjects[training2].NeurableID) + "\n");

			File.AppendAllText(filename, "Tag Information: ");
			for (int num = 0; num < TagObjects.Count-1; num++) {
				Vector2 size = new Vector2(TagObjects[num].ScreenWidth, TagObjects[num].ScreenHeight);
				File.AppendAllText(filename, "[(" + (TagObjects[num].NeurableID) + ")," + TagObjects[num].ProjectedPosition + "," + size + "],");
			}
			Vector2 size2 = new Vector2(TagObjects[TagObjects.Count-1].ScreenWidth, TagObjects[TagObjects.Count-1].ScreenHeight);
			File.AppendAllText(filename, "[(" + (TagObjects[TagObjects.Count-1].NeurableID) + ")," + TagObjects[TagObjects.Count-1].ProjectedPosition + "," + size2 + "]\n");

			File.AppendAllText(filename, "Screen_Width:" + HRES + "\n");
			File.AppendAllText(filename, "Screen_Height:" + VRES + "\n");

			if (didTrain && didPredict) {
				File.AppendAllText(filename, "Testing Type: Training and Prediction\n");
			} else if (didTrain) {
				File.AppendAllText(filename, "Testing Type: Training Only\n");
			} else if (didPredict) {
				File.AppendAllText(filename, "Testing Type: Prediction Only\n " + "Model Imported: " + User.ImportModelPath + "\n");
			}
		}
	}

	void cleanMemory()
	{
			StopAllCoroutines();
			addFooter();
	}

	void OnDestroy()
	{
			cleanMemory();
	}
#endregion
}
