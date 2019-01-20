using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class StressBall : MonoBehaviour {

        [Header("Passes Stress State Values into the Associated Slider Object")]
        [SerializeField]
        SphereCollider stressBall;

        SphereCollider BallObject
        {
            get
            {
                if (stressBall == null) stressBall = GetComponent<SphereCollider>();
                if (stressBall == null) stressBall = GetComponentInChildren<SphereCollider>();
                return stressBall;
            }
        }

        // NeurableAffectiveStateEngine returns a tuple of <time, value>.
        // This function takes the second value and passes it to the Slider
        public void UpdateBallWithStressState(float timestamp, float value)
        {
            if (BallObject != null)
                BallObject.transform.position = new Vector3(-5.5f, value * 10+2, 14);
        }
    }