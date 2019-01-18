/*
* Copyright 2017 Neurable Inc.
*/

using UnityEngine;
using UnityEngine.Events;
using Neurable.Core;

namespace Neurable.Internal
{
    public class ImportEvent : MonoBehaviour
    {
        public KeyCode KeyToImport = KeyCode.None;

        public UnityEvent OnImportFail;
        public UnityEvent OnImportSucceed;

        private void Update()
        {
            if (Input.GetKeyDown(KeyToImport)) ImportModel();
        }

        public void ImportModel()
        {
            if (NeurableUser.Instance.ImportModel())
            {
                OnImportSucceed.Invoke();
            }
            else
            {
                OnImportFail.Invoke();
            }
        }
    }
}
