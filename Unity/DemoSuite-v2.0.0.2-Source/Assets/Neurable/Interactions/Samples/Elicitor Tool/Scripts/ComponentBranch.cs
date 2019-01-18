/*
* Copyright 2017 Neurable Inc.
*/

using UnityEngine;
using System.Collections.Generic;

namespace Neurable.Interactions.Samples
{
    // Component Branch manages multiple INeurableUtilityControllable scripts on an object
    public class ComponentBranch : MonoBehaviour, INeurableUtilityBranchable
    {
        #region ControllableComponents Property

        private List<INeurableUtilityControllable> _controlledComponents;

        public List<INeurableUtilityControllable> ControllableComponents
        {
            get
            {
                if (_controlledComponents == null)
                {
                    _controlledComponents = new List<INeurableUtilityControllable>();
                    foreach (INeurableUtilityControllable t in GetComponentsInChildren<INeurableUtilityControllable>())
                    {
                        if (!t.Equals(this))
                        {
                            Behaviour b = t as Behaviour;
                            if (b != null) b.enabled = false;
                            _controlledComponents.Add(t);
                        }
                    }
                }

                return _controlledComponents;
            }
            set { _controlledComponents = value; }
        }

        public List<string> ControllableComponentStrings
        {
            get
            {
                List<string> strings = new List<string>();
                foreach (INeurableUtilityControllable comp in ControllableComponents)
                {
                    strings.Add(comp.ToString());
                }

                return strings;
            }
        }

        #endregion

        #region ActiveComponent Property

        private INeurableUtilityControllable _activeComponent;

        public INeurableUtilityControllable ActiveComponent
        {
            get { return _activeComponent; }
            set
            {
                foreach (Behaviour comp in ControllableComponents)
                {
                    if (comp != null) comp.enabled = false;
                }

                _activeComponent = value;
                if (_activeComponent != null)
                {
                    Behaviour newcomp = _activeComponent as Behaviour;
                    if (newcomp != null) newcomp.enabled = true;
                }
            }
        }

        public void ChangeComponent(int ComponentIndex)
        {
            if (ComponentIndex >= ControllableComponents.Count)
            {
                Debug.LogError("Component does not exist in Control Branch");
                return;
            }

            ActiveComponent = ControllableComponents[ComponentIndex];
        }

        #endregion

        #region Pass INeurableUtilityControllable to ActiveComponent

        public void RunElicitor()
        {
            if (ActiveComponent == null)
            {
                Debug.LogError("No Active Component Selected");
                return;
            }

            ActiveComponent.RunElicitor();
        }

        public float ElicitorIntensity
        {
            get
            {
                if (ActiveComponent == null)
                {
                    Debug.LogError("No Active Component Selected");
                    return -1f;
                }

                return ActiveComponent.ElicitorIntensity;
            }
            set
            {
                if (ActiveComponent == null)
                {
                    Debug.LogError("No Active Component Selected");
                    return;
                }

                ActiveComponent.ElicitorIntensity = value;
            }
        }

        public float ElicitorHue
        {
            get
            {
                if (ActiveComponent == null)
                {
                    Debug.LogError("No Active Component Selected");
                    return -1f;
                }

                return ActiveComponent.ElicitorHue;
            }
            set
            {
                if (ActiveComponent == null)
                {
                    Debug.LogError("No Active Component Selected");
                    return;
                }

                ActiveComponent.ElicitorHue = value;
            }
        }

        #endregion

        public void OnEnable()
        {
            if (_activeComponent == null) ChangeComponent(0);
        }
    }
}
