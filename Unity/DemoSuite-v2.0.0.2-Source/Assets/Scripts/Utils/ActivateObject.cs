using System.Collections.Generic;
using UnityEngine;

namespace Neurable.Internal
{
    public class ActivateObject : MonoBehaviour
    {
        [Header("Toggles the Listed Targets in the Scene")]
        public List<GameObject> Targets;
        public KeyCode Trigger = KeyCode.UpArrow;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(Trigger))
            {
                ToggleTargets();
            }
        }

        public void ToggleTargets()
        {
            foreach (var Target in Targets) ToggleObject(Target);
        }

        public void ToggleObject(GameObject obj)
        {
            obj.SetActive(!obj.activeInHierarchy);
        }
    }
}
