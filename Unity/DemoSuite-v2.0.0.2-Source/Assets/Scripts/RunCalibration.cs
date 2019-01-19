using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunCalibration : MonoBehaviour {
	TrainerContext trainer;
	private TrainerContext Trainer
	{
		get
		{
			if (trainer == null)
				trainer = FindObjectOfType<TrainerContext>();
			if (trainer == null)
			{
				throw new MissingComponentException("No Trainer Found");
			}
			return trainer;
		}
	}

	public void RunTrain()
	{
		Trainer.startTraining();
	}
	public void RunPredict()
	{
		Trainer.startPrediction();
	}

}
