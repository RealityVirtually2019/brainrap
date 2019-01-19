/*
* Copyright 2017 Neurable Inc.
*/

using UnityEngine;

namespace Neurable.Diagnostics
{
    [RequireComponent(typeof(Animator))]
    public class LogoClear : MonoBehaviour
    {
        public void ClearLogo()
        {
            GetComponent<Animator>().SetBool("UserReady", true);
            Destroy(gameObject, 5);
        }
    }
}
