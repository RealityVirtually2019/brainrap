using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArousalNeuron : MonoBehaviour {
    LineRenderer neuron;
	// Use this for initialization
	void Start () {
        neuron = GetComponent<LineRenderer>();
        neuron.startWidth = 0.02f;
        neuron.endWidth = 0.02f;
    }
	public void UpdateNeuronWithArousal(float timestamp, float value)
    {
        Debug.Log(value);
        
        neuron.SetVertexCount(Mathf.Abs((int)(value*1000))/2);
        float angle = Random.Range(-180.0f, 180.0f) * 2 * Mathf.PI / 360;
        for (int i = 1; i <= neuron.positionCount; i++)
        {
            neuron.SetPosition(i, new Vector3(Mathf.Pow(Mathf.Abs(value * 10),i)* Mathf.Sin(angle), value*i, Mathf.Pow(Mathf.Abs(value * 10) , i) * Mathf.Cos(angle)));
        }
    }
}
