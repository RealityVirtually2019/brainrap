using UnityEngine;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif
using UnityEngine.Events;

namespace Neurable.Internal
{
    public class NoVRControl : MonoBehaviour
    {
        public float NoVRHeight = 1.6f;
        public UnityEvent NoVRConversion;

        void Start()
        {
            if (!isUsingVR())
            {
                if (NoVRConversion != null) NoVRConversion.Invoke();
                transform.position += transform.up * NoVRHeight;
            }
        }

        public static bool isUsingVR()
        {
#if UNITY_2017_2_OR_NEWER
        return (XRDevice.isPresent && XRSettings.loadedDeviceName != "none");
#else
            return (VRDevice.isPresent && VRSettings.loadedDeviceName != "none");
#endif
        }
    }
}
