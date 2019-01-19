/*
* Copyright 2017 Neurable Inc.
*/

using System.Collections.Generic;
using UnityEngine;

namespace Neurable.Interactions.Samples
{
    public class GridSpawner : MonoBehaviour
    {
        public GameObject objToSpawn;

        public int rows = 4, cols = 5; // Overridden by Context
        public Vector2 spaceBetweenObjs = Vector2.one;

        public Vector3 setRotation = new Vector3(20.077f, 9.425f, 31.785f);
        private ElicitorManager context;

        void Start()
        {
            SpawnObjects();
        }

        private List<List<NeurableTag>> objList;

        public List<List<NeurableTag>> SpawnedObjects
        {
            get
            {
                if (objList == null) SpawnObjects();
                return objList;
            }
        }

        public void SpawnObjects()
        {
            context = GetComponent<ElicitorManager>();
            if (context && context.numRows > 0 && context.numCols > 0)
            {
                rows = context.numRows;
                cols = context.numCols;
            }

            objList = new List<List<NeurableTag>>(rows);
            for (int row = 0; row < rows; row++)
            {
                objList.Add(new List<NeurableTag>(cols));
            }

            Vector3 offset = Vector3.zero;
            if (objToSpawn != null) offset = objToSpawn.transform.position;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Vector3 p = transform.TransformPoint(new Vector3(j * spaceBetweenObjs.x + offset.x,
                                                                     i * spaceBetweenObjs.y + offset.y, offset.z));
                    GameObject temp = Instantiate(objToSpawn, p, objToSpawn.transform.rotation, transform);
                    temp.transform.localEulerAngles = setRotation;
                    objList[i].Add(temp.GetComponent<NeurableTag>());
                }

                context.addTags(objList[i].ToArray());
            }

            context.StartAnim();
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying) return;
            Vector3 offset = Vector3.zero;
            if (objToSpawn != null) offset = objToSpawn.transform.position;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Vector3 p = transform.TransformPoint(new Vector3(j * spaceBetweenObjs.x + offset.x,
                                                                     i * spaceBetweenObjs.y + offset.y, offset.z));
                    Gizmos.DrawWireCube(p, objToSpawn.transform.lossyScale);
                }
            }
        }
    }
}
