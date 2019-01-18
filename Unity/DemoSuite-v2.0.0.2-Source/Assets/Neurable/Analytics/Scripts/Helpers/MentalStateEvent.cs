using System;
using UnityEngine.Events;

namespace Neurable.Analytics
{
    [Serializable]
    public class MentalStateEvent : UnityEvent<float, float>
    {
        // Arguments are Time, Value
    }
}
