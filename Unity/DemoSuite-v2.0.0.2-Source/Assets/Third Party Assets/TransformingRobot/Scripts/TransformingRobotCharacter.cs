using UnityEngine;
using System.Collections;

public class TransformingRobotCharacter : MonoBehaviour {

	public enum RobotMode {TANK, PLANE, ROBOT };
	private int numModes = 2;
	private RobotMode robotMode = RobotMode.TANK;
	public Animator robotAnimator;
	Rigidbody robotRigidBody;



	// Use this for initialization
	void Start () {
		robotAnimator = GetComponent<Animator> ();
		robotAnimator.speed = 1;
		robotRigidBody = GetComponent<Rigidbody> ();
	}

	void Update(){

	}

	void RobotModeChange(RobotMode aRobotMode){
		robotMode = aRobotMode;
		switch(robotMode)
		{
			case RobotMode.ROBOT:
				Robot();
				break;
			case RobotMode.TANK:
				Tank();
				break;
			case RobotMode.PLANE:
				Plane();
				break;
		}

	}

	public void TransformationParams()
	{
		robotAnimator.applyRootMotion = true;
		robotRigidBody.useGravity = false;
		robotRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
	}

	protected void RobotParams()
	{
		//transform.rotation = Quaternion.identity;
		robotAnimator.applyRootMotion = true;
		robotRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		robotRigidBody.useGravity = true;
	}
	public void Robot(){
		TransformationParams();
		robotAnimator.SetTrigger ("Robot");
		Invoke("RobotParams", 1f);
	}

	public void TankParams()
	{
		//transform.rotation = Quaternion.identity;
		robotAnimator.applyRootMotion = false;
		robotRigidBody.constraints = RigidbodyConstraints.FreezeRotationX;
		robotRigidBody.constraints = RigidbodyConstraints.FreezeRotationZ;
		robotRigidBody.useGravity = true;
	}
	public void Tank()
	{
		TransformationParams();
		robotAnimator.SetTrigger ("Tank");
		Invoke("TankParams", 1f);
	}

	protected void PlaneParams()
	{
		//transform.rotation = Quaternion.identity;
		robotAnimator.applyRootMotion = true;
		robotRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		robotRigidBody.useGravity = true;
	}
	public void Plane()
	{
		TransformationParams();
		robotAnimator.SetTrigger ("Plane");
		Invoke("PlaneParams", 1f);
	}

	public void CycleMode()
	{
		RobotMode newMode = (RobotMode)(((int)robotMode + 1) % numModes);
		RobotModeChange(newMode);
	}
}
