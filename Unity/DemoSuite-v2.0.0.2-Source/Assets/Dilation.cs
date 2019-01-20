using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameObject))]
public class Dilation : MonoBehaviour
{

    [Header("Passes Attention State Values into the Eye")]
    [SerializeField]
    GameObject Pupil;

    // NeurableAffectiveStateEngine returns a tuple of <time, value>.
    // This function takes the second value and passes it to the Slider
    public void UpdateEyeWithAttentionState(float timestamp, float value)
    {
        if (Pupil != null)
            Pupil.transform.localScale = new Vector3((value*10+1), 0.03f, (value * 10 + 1));
    }
}