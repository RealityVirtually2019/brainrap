/*
 * Rotates child game object, by default starts dead until gatherObject is called
 * Every rotationCycleTime, it switches the rotation axis.
 * Every rotationLerpTime, it reverses it's lerp back and forth between minimumSpeed and DegreesPerSecond
 * Note: You must set floatingItem in Unity Editor
 * Note: Will automatically center the child if it has any primitive collider. 
 *	-- If it has multiple colliders or a mesh collider, it will attempt to center the object.
 *	-- If it doesn't work, disable automatic centering in the editor and center it manually on the parent.
 */

 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class FloatingItem : MonoBehaviour {
	[Header("Basic Defaults")]
	public float minimumSpeed = 3f; //default values
	public float degreesPerSecond = 25f;

	private float rotationLerpTime;
	private float rotationCycleTime;
	/*private float minLerpTime = 2f;
	private float maxLerpTime = 4f;
	private float rotationTimeMultiplier = 2f;
	private float rotationTimeAddition = .5f;*/
	private bool lerpOption = true; //Use checkbox to toggle lerp
	private bool rotationSwitchOption = true; //Use checkbox to toggle rotation switching

	[Header("Rotation Specifics")]
	public bool automaticCentering = true; //Use checkbox to toggle automatic centering
	public float automaticCenteringTime = 2f;
	public bool rotationOn = false; //Use checkbox to toggle rotation
	[Header("Focus Specifics")]
	public AnimationCurve FocusRotation;
	public float focusTime = .75f;
	[Header("Selection Specifics")]
	public float selectionTime = 1f;
	public float throwForce = 500f;
	/*private bool explosionOnRelease = true; //Use checkbox to toggle explosionOnRelease
	private float explosionForce = 250f;*/

	[HideInInspector]
	public bool tkOn = false; //Use checkbox to toggle affected by gravity and isKinematic

	private GameObject floatingItem;  

	private float startSpeed; //variables needed for lerps
	private float stopSpeed;
	private bool rotateLeft = true;
	private bool forwardLerp = true;
	private bool shouldRotate = false;

	private bool selected = false;
	[HideInInspector]
	public bool focused = false;

	private float[] randoms;
	private int numItems = 6;

	private Rigidbody childRigid;  //components of the child
	private Transform childTransform;
	private Collider childCollider;

	private Camera playerCamera;
	private TelekineticTrainer context;

	private Vector3 _initialLocalPosition;
	private Quaternion _initialLocalRotation;

	//Sets child components and uses the box collider and scale to center the child on the parent
	private void Awake()
	{
		transform.GetComponent<Rigidbody>().useGravity = false;
		transform.GetComponent<Rigidbody>().isKinematic = true;

		if (transform.childCount == 1) {
			floatingItem = transform.GetChild (0).gameObject;
		}

		rotationLerpTime = Random.Range(2f, 4f);
		rotationCycleTime = rotationLerpTime * 2f + .5f;

		randoms = new float[numItems];
		for (int i = 0; i < numItems; i++) {
			randoms[i] = Random.Range(-1f, 1f);
		}

		childTransform = floatingItem.transform;
		_initialLocalPosition = childTransform.localPosition;
		_initialLocalRotation = childTransform.localRotation;
		childRigid = floatingItem.GetComponent<Rigidbody>();
		childCollider = floatingItem.GetComponent<Collider>();

		if (childRigid && tkOn) {
			childRigid.useGravity = false;
			childRigid.isKinematic = true;
		}
		playerCamera = GameObject.Find("Head").GetComponent<Camera>();
		context = GameObject.FindObjectOfType<TelekineticTrainer>();
	}

	// Use this for initialization
	void Start ()
	{
	}

	// Update is called once per frame
	float lerpTime = 0;
	void Update ()
	{
		if (rotationOn && shouldRotate) {
			if (lerpTime > rotationLerpTime && lerpOption) {
				swapLerpDirections();
				lerpTime = 0;
			}

			if (!lerpOption) {
				lerpTime = rotationLerpTime;
			}

			Vector3 firstRotation = Vector3.Normalize(new Vector3(randoms[0], randoms[1], randoms[2]));
			Vector3 secondRotation = Vector3.Normalize(new Vector3(randoms[3], randoms[4], randoms[5]));


			if (rotateLeft) {
				transform.Rotate(firstRotation, Mathf.Lerp(startSpeed, stopSpeed, lerpTime / rotationLerpTime) * Time.deltaTime, Space.Self);
			} else {
				transform.Rotate(secondRotation, Mathf.Lerp(startSpeed, stopSpeed, lerpTime / rotationLerpTime) * Time.deltaTime, Space.Self);
			}
			lerpTime += Time.deltaTime;
		}
	}

	//called when being gathered into a floating group
	public void gatherObject()
	{
		setTk(true);
		setSelected(false);
		StartCoroutine(gatherObjectCoroutine());
				
	}

	//centers children on itself so they can rotate properly
	IEnumerator gatherObjectCoroutine()
	{
		if (automaticCentering && childCollider && childTransform) {
			Vector3 currentPosition = childTransform.localPosition;
			Quaternion currentRotation = childTransform.localRotation;
			float t = 0;
			while (t <= automaticCenteringTime) {
				childTransform.localPosition = Vector3.Lerp(currentPosition, _initialLocalPosition, t / automaticCenteringTime);
				childTransform.localRotation = Quaternion.Lerp (currentRotation, _initialLocalRotation, t / automaticCenteringTime);
				t += Time.deltaTime;
				yield return null;
			}
			childTransform.localPosition = _initialLocalPosition;
			childTransform.localRotation = _initialLocalRotation;
			yield return null;
		}
		setRotating(true);
	}

	/* 
	 * releases object. called when group releases
	 * :param groupCenter: the center of floating group for explosion force
	 */
	public void releaseObject(Vector3 groupCenter)
	{
		if (!selected) {
			setTk(false);
			setRotating(false);
			childRigid.AddExplosionForce(250f, groupCenter, 0);
		}
	}

	//Swaps the start and stop speed, resets time and the forwardlerp bool to reverse the lerp
	void swapLerpDirections()
	{
		if (forwardLerp) {
			lerpTime = 0;
			startSpeed = minimumSpeed;
			stopSpeed = degreesPerSecond;
			forwardLerp = false;
		} else {
			lerpTime = 0;
			startSpeed = degreesPerSecond;
			stopSpeed = minimumSpeed;
			forwardLerp = true;
		}
	}

	//flips the rotateleft bool to change directions in the update function
	IEnumerator rotateSwitch()
	{
		while (true) {
			rotateLeft = true;
			yield return new WaitForSeconds(rotationCycleTime);
			rotateLeft = false;
			yield return new WaitForSeconds(rotationCycleTime);
		}
	}

	//returns value of rotationOn
	public bool isRotating() { return shouldRotate; }

	//returns value of tkOn
	public bool isTkOn() { return tkOn; }

	/* 
	 * sets rotationOn, which turns on/off rotation
	 * :param option: a bool to set rotationOn
	 */
	public void setRotating(bool option)
	{
		if (option) {
			lerpTime = 0;
			swapLerpDirections();
			shouldRotate = true;
			if (rotationSwitchOption) {
				StartCoroutine(rotateSwitch());
			}
		} else {
			shouldRotate = false;
		}
	}

	/*
	 * sets tkOn, and accordingly sets gravity and kinematic for child object
	 * :param option: a boot to set tkOn and associated properties
	 */
	public void setTk(bool option)
	{
		if (option == true) {
			tkOn = true;
			childRigid.useGravity = false;
			childRigid.isKinematic = true;
		} else {
			tkOn = false;
			childRigid.useGravity = true;
			childRigid.isKinematic = false;
		}
	}

	/*
	 * turns a linear t into a "smoothestStep" line. 
	 * Source: https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
	 * :param t: the linear t you want to transform.
	 */
	float smoothestStep(float t) { return (t * t * t * (t * (6f * t - 15f) + 10f)); }


	public void setSelected (bool option) { selected = option;}

	public void setLocalPosition (Vector3 localPositionParam)
	{
		if (!selected) {
			transform.localPosition = localPositionParam;
		}
	}

	// call to select object
	public void selectObject()
	{
		setSelected(true);
		StartCoroutine(selectObjectCoroutine());
	}

	// animation and movement for when object is selected
	public float floatDistance = 1.5f;
	IEnumerator selectObjectCoroutine()
	{
		Vector3 initialPosition = transform.position;
		Vector3 finalPosition = playerCamera.transform.position + (playerCamera.transform.forward * floatDistance);
		//StartCoroutine(throwObject(playerCamera));
		float t = 0;
		float timer = 0;
		while (t < 1) {
			t = smoothestStep(timer / selectionTime);
			Vector3 tempPosition = Vector3.Lerp(initialPosition, finalPosition, t);
			transform.position = tempPosition;
			finalPosition = playerCamera.transform.position + (playerCamera.transform.forward * floatDistance);
			yield return null;
			timer += Time.deltaTime;
		}
		yield return new WaitForSeconds(1f);
		if (context.currentState == TrainerContext.TrainState.Training && context.releaseAnimationsTraining) {
			StartCoroutine(throwObject());
		} else if (context.currentState == TrainerContext.TrainState.Predicting && context.releaseAnimationsPrediction) {
			StartCoroutine(throwObject());
		}
		
		
	}

	/* 
	 * animation and movement for after object selection
	 * :param playerCameraParam: player camera reference for throw direction reference
	 */
	public IEnumerator throwObject()
	{
		setTk(false);
		setRotating(false);
		Vector3 forceUp = Vector3.Normalize(playerCamera.transform.up * .35f);
		Vector3 forceForward = playerCamera.transform.forward;
		Vector3 forceSideways = Random.Range(-.5f, .5F) * playerCamera.transform.right;
		Vector3 forceDirection = Vector3.Normalize(forceForward + forceSideways + forceUp);
		childRigid.AddForce(forceDirection * throwForce);
		yield return null;
	}

	// a function that quickly spins the object, used for focus
	public void quickSpin()
	{
		shouldRotate = false;
		StartCoroutine(quickSpinCoroutine());
	}

	//meat and bones of the quickSpin
	IEnumerator quickSpinCoroutine()
	{
		Vector3 initial = transform.eulerAngles;
		float lastA = 0;
		for (float t = 0.0f; t <= focusTime; t += Time.deltaTime / 1) {
			float angle = FocusRotation.Evaluate(t / focusTime) * 360;
			floatingItem.transform.RotateAround(transform.position, new Vector3(1, 0, 0), angle - lastA);
			lastA = angle;
			yield return null;
		}
		floatingItem.transform.RotateAround(transform.position, new Vector3(1, 0, 0), 360 - lastA);
		shouldRotate = true;
		yield return null;
	}
}
