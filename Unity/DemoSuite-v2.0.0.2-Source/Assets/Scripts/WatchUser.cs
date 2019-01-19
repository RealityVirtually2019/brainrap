using UnityEngine;
using UnityEngine.Events;
using Neurable.Core;

public class WatchUser : MonoBehaviour {

	public UnityEvent OnUserReady;
	public UnityEvent OnUserTrained;
	[Tooltip("Activate DEBUG_USER to assume player is Ready and Trained")]
	public bool DEBUG_USER = false;

	NeurableUser _user;
	NeurableUser User
	{
		get
		{
			if (_user == null)
				_user = FindObjectOfType<NeurableUser>();
			return _user;
		}
	}

	bool firstConnect = false;
	public bool Connected
	{
		get
		{
			if (!firstConnect)
				firstConnect = User.Ready || DEBUG_USER;
			return firstConnect;
		}
	}

	bool firstTrained = false;
	public bool Trained
	{
		get
		{
			if (!firstTrained)
				firstTrained = User.User.HasModel() || DEBUG_USER;
			return firstTrained;
		}
	}

	private void OnEnable()
	{
		if (!Application.isEditor) DEBUG_USER = false;
		if (User == null)
			throw new MissingReferenceException("No User found in Scene");
		InvokeRepeating("CheckUser", 0, .5f);
	}

	// Update is called once per frame
	void CheckUser ()
	{
		if (!firstConnect && Connected) OnUserReady.Invoke();
		if (!firstTrained && Trained) OnUserTrained.Invoke();
		if (firstConnect && firstTrained) CancelInvoke("CheckUser");
	}
}
