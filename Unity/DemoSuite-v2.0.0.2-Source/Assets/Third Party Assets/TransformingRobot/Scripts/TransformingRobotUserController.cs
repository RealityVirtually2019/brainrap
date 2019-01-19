using UnityEngine;
using System.Collections;
using Neurable.Interactions;

public class TransformingRobotUserController : NeurableTag {

	private TransformingRobotCharacter transformingRobotCharacter;
	private int counter = 0;
	private ElicitorManager context;

	public override void Awake ()
	{
		base.Awake();
		context = GameObject.FindObjectOfType<ElicitorManager>();
		transformingRobotCharacter = GetComponent<TransformingRobotCharacter> ();	
	}

	protected virtual void OnEnable()
	{
		NeurableEnabled = true;
	}

	protected virtual void OnBecameVisible()
	{
		NeurableEnabled = true;
	}

	public void changeAnimation(){
		context.StopAnim();
		counter++;
		int animType = (counter % 3);
		if (animType == 0){
			transformingRobotCharacter.Tank();
		}
		else if (animType == 1){
			transformingRobotCharacter.Robot();
		}
		else{
			transformingRobotCharacter.Plane();
		}
		context.StartAnim();
	}
}
