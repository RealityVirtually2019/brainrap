using UnityEngine.Events;
using UnityEngine;
using Neurable.Interactions;

public class TrainerTag : NeurableTag {
	[Header("Training Focus")]
	public UnityEvent OnFocus;
	public UnityEvent OnUnfocus;
	public void Focus()
	{
		if (OnFocus != null && OnFocus.GetPersistentEventCount() > 0)
			OnFocus.Invoke();
		else
			Debug.LogWarning("No Focus Action for " + name);
	}
	public void Unfocus()
	{
		if (OnUnfocus != null && OnUnfocus.GetPersistentEventCount() > 0)
			OnUnfocus.Invoke();
		else
			Debug.LogWarning("No Focus Action for " + name);
	}

	public override bool NeurableVisible
	{
		get
		{
			if (isActiveAndEnabled)
				UpdatePosition(true); // Update for Relevant Visibility
			return isActiveAndEnabled;
		}

		set
		{
			base.NeurableVisible = isActiveAndEnabled;
		}
	}
}
