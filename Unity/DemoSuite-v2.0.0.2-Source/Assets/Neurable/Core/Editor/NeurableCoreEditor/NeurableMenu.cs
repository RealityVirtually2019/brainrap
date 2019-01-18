/*
* Copyright 2017 Neurable Inc.
*/

using UnityEngine;
using UnityEditor;

namespace Neurable.Core
{
    public static class NeurableMenu
    {
        private const string MANAGER_NAME = "Neurable Manager";
        private const string PATH_BASE = "Assets/Neurable/";

        [MenuItem("Neurable/Add Neurable User", priority = -1000)]
        private static void InstantiateUser()
        {
            GameObject user = null;
            var found = Object.FindObjectOfType<NeurableUser>();
            if (found != null)
            {
                user = found.gameObject;
            }
            else
            {
                var prefab = AssetDatabase.LoadAssetAtPath( PATH_BASE + "Core/Prefabs/NeurableUser.prefab", typeof(GameObject));
                if (prefab)
                    user = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            }
            
            if (!user) throw new MissingComponentException("NeurableUser Prefab not Found");
#if NEURABLE_DIAGNOSTICS
            var userReady = InstantiateManager<Diagnostics.UserReady>(user.name);
            var userStatePanelPath = PATH_BASE + "Diagnostics/Prefabs/UserStatePanel.prefab";
            userReady.userStatePanelPrefab = AssetDatabase.LoadAssetAtPath<Diagnostics.UserStatePanel>(userStatePanelPath);
            InstantiateManager<Diagnostics.NeurableCursor>(user.name);
#endif
            Selection.activeGameObject = user;
        }

#if NEURABLE_ANALYTICS
        [MenuItem("Neurable/Add Components.../Add Neurable Fixation Engine", priority = -15)]
        private static void InstantiateFixationEngine()
        {
            InstantiateManager<Analytics.NeurableMentalStateEngine>();
            var ob = InstantiateManager<Analytics.FixationEngine>();
            Selection.activeGameObject = ob.gameObject;
        }

        [MenuItem("Neurable/Add Components.../Add Neurable Affective State Engine", priority = -14)]
        private static void InstantiateAffectiveStateEngine()
        {
            var ob = InstantiateManager<Analytics.NeurableMentalStateEngine>();
            Selection.activeGameObject = ob.gameObject;
        }

        [MenuItem("Neurable/Add Components.../Add Neurable EEG Data Engine", priority = -13)]
        private static void InstantiateEegDataEngine()
        {
            var ob = InstantiateManager<Analytics.NeurableEegDataEngine>();
            Selection.activeGameObject = ob.gameObject;
        }
#endif
        
        private static GameObject SpawnManagerBase(string name = MANAGER_NAME)
        {
            var found = GameObject.Find(name);
            return (found != null) ? found : new GameObject(name);
        }

        public static T InstantiateManager<T>(string managerName = MANAGER_NAME) where T : UnityEngine.Component
        {
            var found = Object.FindObjectOfType<T>();
            if (found != null)
                return found;
            var parentObject = SpawnManagerBase(managerName);
            return parentObject.AddComponent<T>();
        }

        [MenuItem("Neurable/View Documentation", priority = 10)]
        private static void OpenNeurableDocs()
        {
            Application.OpenURL("https://wiki.neurable.com/-LMnMGPGyap6Om7i9Oje/");
        }
    }
}
