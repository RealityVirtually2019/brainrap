/*
* Copyright 2017 Neurable Inc.
*/

using UnityEngine;

namespace Neurable.Interactions.Samples
{
    public class CubeRotate : MonoBehaviour
    {
        public bool AmbientRotate = false;
        public Vector3 RotateSpeed = new Vector3(5f, 0f, 0f);

        public void FixedUpdate()
        {
            if (AmbientRotate)
            {
                transform.Rotate(RotateSpeed, Space.World);
            }
        }
    }
}
