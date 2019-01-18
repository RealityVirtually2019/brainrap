using UnityEngine;
using Neurable.Interactions;

[RequireComponent(typeof(NeurableTag))]
public class ToggleTagOnDisable : MonoBehaviour {

	private ElicitorManager context;
	private NeurableTag thisTag;

	public virtual void Awake ()
	{
		context = GetComponentInParent<ElicitorManager>();
		thisTag = GetComponent<NeurableTag>();
	}

	public void OnEnable()
	{
		if (context == null) context = GetComponentInParent<ElicitorManager>();
		if (thisTag == null) thisTag = GetComponent<NeurableTag>();
		if (thisTag && context && context.isActiveAndEnabled)
		{
			context.addTag(thisTag);
		}
	}

	public void OnDisable()
	{
		if (context == null) context = GetComponentInParent<ElicitorManager>();
		if (thisTag == null) thisTag = GetComponent<NeurableTag>();
		if (thisTag && context && context.isActiveAndEnabled)
		{
			NeurableTag[] ts = { thisTag };
			context.removeTags(ts);
		}
	}
}
