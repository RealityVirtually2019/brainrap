/*
 * Simple script that automatically positions child objects equadistantly roon a circle
 * at groupRadius and rotates them at degreesPerSecond
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingGroup : MonoBehaviour {

	public float groupRadius = .5f;
	public float degreesPerSecond = 5f;
	public float gatherTimeInSeconds = 4f;
	public bool RotateGroup = false;
	public bool isRotating = false;
	public bool randomPositionsTraining = false;
	public bool randomPositionsPrediction = false;

	private float circleDivision;
	private FloatingItem[] children;
	private List<Vector3> initalPositions;
	private List<Vector3> targetPositions;
	private float randomDegrees = 0f;

	private TelekineticTrainer context;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, groupRadius);
    }

	// Determines how many degrees each object should be apart
	private void Awake()
	{
		children = GetComponentsInChildren<FloatingItem>();
		circleDivision = 360f / children.Length;
		context = GameObject.FindObjectOfType<TelekineticTrainer>();
		PopulatePositions();
	}

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	float timer = 0f;
	void Update () {
		if (RotateGroup && isRotating) {
			float degreesMoved = Time.deltaTime * degreesPerSecond;
			for (int i = 0; i < children.Length; i++) {
				Vector3 RotatedPoint = Quaternion.Euler(transform.forward * degreesMoved) * children[i].transform.localPosition; // rotate it
				targetPositions[i] = RotatedPoint;
				children[i].transform.localPosition = RotatedPoint;
			}
		}
		if (gathering)
		{
			for (int i = 0; i < children.Length; i++)
			{
				Vector3 tempPosition = Vector3.Lerp(initalPositions[i], targetPositions[i], timer / gatherTimeInSeconds);
				children[i].transform.localPosition = tempPosition;
			}
			timer += Time.deltaTime;
			if (timer > gatherTimeInSeconds)
			{
				isRotating = RotateGroup;
				timer = 0f;
				gathering = false;
				for (int i = 0; i < transform.childCount; i++)
				{
					children[i].transform.localPosition = targetPositions[i];
				}
			}
		}
	}

	//call to release the floating group
	public void releaseGroup()
	{
		gathering = false;
		isRotating = false;
		for (int i = 0; i < children.Length; i++) {
			children[i].transform.gameObject.GetComponent<FloatingItem>().releaseObject(transform.position);
		}
	}

	void PopulatePositions()
	{
		targetPositions = new List<Vector3>();
		isRotating = false;
		List<int> indicies = new List<int>(); for (int i = 0; i < children.Length; ++i) { indicies.Add(i); }

		for (int i = 0; i < children.Length; i++)
		{
			int index = i;
			if ((context.currentState == TrainerContext.TrainState.Training && randomPositionsTraining) || (context.currentState == TrainerContext.TrainState.Predicting && randomPositionsPrediction))
			{
				index = indicies[Random.Range((int)0, (int)indicies.Count)];
				indicies.Remove(index);
			}
			Vector3 endPosition = new Vector3(Mathf.Cos(Mathf.Deg2Rad * ((circleDivision * index) + randomDegrees)) * groupRadius,
				Mathf.Sin(Mathf.Deg2Rad * ((circleDivision * index) + randomDegrees)) * groupRadius, 0);
			targetPositions.Add(endPosition);
		}
	}

	//call to gather group to float
	bool gathering = false;
	public void gatherGroup()
	{
		// if (targetPositions == null || (context.currentState == TrainerContext.TrainState.Training && randomPositionsTraining) || (context.currentState == TrainerContext.TrainState.Predicting && randomPositionsPrediction))
		PopulatePositions();
		initalPositions = new List<Vector3>();
		for (int i = 0; i < children.Length; ++i) {
			initalPositions.Add(children[i].transform.localPosition);
			children[i].gatherObject();
		}
		isRotating = false;
		gathering = true;
	}

	/*
	 * turns a linear t into a "smoothestStep" line. 
	 * Source: https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
	 * :param t: the linear t you want to transform.
	 */
	float smoothestStep(float t) { return (t * t * t * (t * (6f * t - 15f) + 10f)); }
}
