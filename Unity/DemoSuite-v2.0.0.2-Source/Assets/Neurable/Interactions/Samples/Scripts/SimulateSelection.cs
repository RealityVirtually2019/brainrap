/*
* Copyright 2017 Neurable Inc.
*/

using UnityEngine;

namespace Neurable.Interactions.Samples
{
    // Used to test whether the internal Neruable API can trigger Action Callbacks
    [RequireComponent(typeof(NeurableTag))]
    public class SimulateSelection : MonoBehaviour
    {
        public KeyCode TriggerButton = KeyCode.Space;
        public KeyCode FlickerButton = KeyCode.F;
        public bool debug = false;

        private NeurableTag mytag;

        void Awake()
        {
            mytag = GetComponent<NeurableTag>();
        }

        void Update()
        {
            if (Input.GetKeyDown(TriggerButton))
            {
                if (mytag)
                {
                    mytag.simulateTagSelect();
                    if (debug) Debug.Log("Tag Selection Simlulated: " + name);
                }
            }

            if (Input.GetKeyDown(FlickerButton))
            {
                if (mytag)
                {
                    mytag.simulateTagAnim();
                    if (debug) Debug.Log("Tag Animation Simlulated: " + name);
                }
            }
        }
    }
}
