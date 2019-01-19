using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerFocus : MonoBehaviour {

	public GameObject _IndicatorPrefab;
	private GameObject indicatorInstance;

	private FloatingItem floatingItem;
	private Material myMaterial;
	private Color origColor;
	private string PropertyString = "_Float";
	private string PropertyString1 = "_Color";

	[HideInInspector]
	public bool focusColor; //do not touch. Adjust in the context.

	private void Awake()
	{
		floatingItem = GetComponentInParent<FloatingItem>();
		if (focusColor) {
			myMaterial = GetComponent<Renderer>().material;
			myMaterial.SetFloat(PropertyString, 0f);
			origColor = myMaterial.GetColor (PropertyString1);
		}
	}


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void indicate() {
		if (_IndicatorPrefab) {
			indicatorInstance = Instantiate(_IndicatorPrefab, floatingItem.transform.position, floatingItem.transform.rotation, floatingItem.transform);
			indicatorInstance.transform.localScale = floatingItem.transform.localScale;
		}
	}

	public void focus()
	{
		floatingItem.focused = true;
		if (focusColor) {
			myMaterial.SetFloat(PropertyString, 1f);
			myMaterial.SetColor(PropertyString1, Color.blue);
		}
	}

	public void unfocus()
	{
		floatingItem.focused = false;
		if (focusColor) {
			myMaterial.SetFloat(PropertyString, 0f);
			myMaterial.SetColor(PropertyString1, origColor);
		}
		if (indicatorInstance)
			Destroy (indicatorInstance);
	}

}
