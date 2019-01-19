using UnityEngine;
using Neurable.Core;

namespace Neurable.Diagnostics
{
    [RequireComponent(typeof(NeurableCamera))]
    public class NeurableCursor : MonoBehaviour
    {
        [Header("Activation")]
        [SerializeField, Tooltip("Whether or not the Cursor is active")]
        protected bool _cursorActive = false; // Inspector Value. Is the Cursor Active

        [Header("Point Options")]
        [SerializeField, Tooltip("If true, will use a Line to represent the user's gaze")]
        protected bool useRay;
        [SerializeField, Tooltip("Distance from the User's camera to place the cursor")]
        protected float pointDistance = 1f;
        [SerializeField, Tooltip("Size of the cursor's point")]
        protected float pointSize = 0.01f;

        [Header("Appearance")]
        [SerializeField, Tooltip("Layer to place the cursor")]
        protected string layerName = "Default";
        [SerializeField, Tooltip("Material to build into cursor. Default: Standard Shader")]
        protected Material cursorMat;
        [SerializeField, Tooltip("LineRenderer Color Options")]
        protected Color cursorColor = Color.cyan;

        protected LineRenderer CursorRendererInstance; // LineRenderer Component of CursorInstance
        protected NeurableCamera CameraReference;      // Reference to NeurableCamera

        #region Cursor Activation

        protected bool cursorActived = false; // Processed Value of _cursorActive, updated after cursor initialized

        public bool CursorActive
        {
            get
            {
                if (cursorActived != _cursorActive) CursorActive = _cursorActive;
                return cursorActived;
            }
            set
            {
                if (value && !cursorActived)
                {
                    initializeCursor();
                }

                if (!value && cursorActived)
                {
                    destroyCursor();
                }

                cursorActived = value;
                _cursorActive = cursorActived;
            }
        }

        public void ToggleCursor()
        {
            CursorActive = !CursorActive;
        }

        #endregion

        #region Renderer Initialization

        protected Material CursorMaterial
        {
            get
            {
                if (!cursorMat)
                {
                    cursorMat = new Material(Shader.Find("Unlit/Color"));
                    cursorMat.color = cursorColor;
                }

                return cursorMat;
            }
        }

        LineRenderer initializeRenderer()
        {
            GameObject newInstance = new GameObject();
            newInstance.layer = LayerMask.NameToLayer(layerName);
            newInstance.transform.parent = transform;
            LineRenderer rend = newInstance.AddComponent<LineRenderer>();
            rend.numCapVertices = 5;
            rend.useWorldSpace = true;
            rend.name = "Neurable Eyetracker Debug Point";
            rend.material = CursorMaterial;
            rend.startColor = cursorColor;
            rend.endColor = cursorColor;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
            return rend;
        }

        void initializeCursor()
        {
            destroyCursor();
            CursorRendererInstance = initializeRenderer();
        }

        void destroyCursor()
        {
            if (CursorRendererInstance) Destroy(CursorRendererInstance.gameObject);
            CursorRendererInstance = null;
        }

        #endregion

        #region Draw Calls

        public static Vector3 ExtendPointAlongVector(Vector3 origin, Vector3 point, float distance, bool ray = false)
        {
            Vector3 directionVector = Vector3.Normalize(point - origin);
            float d = ray ? distance * 5 : distance;
            return (origin + (directionVector * d));
        }

        void DrawPointInCameraspace(LineRenderer rend, Vector3 point)
        {
            if (!rend) return;
            Vector3[] points;
            if (!CameraReference.PlayerCam)
            {
                Debug.LogError("No PlayerCamera Found");
                points = new Vector3[0];
            }
            else
            {
                Vector3 origin = CameraReference.PlayerCam.transform.position;
                Vector3 extended = ExtendPointAlongVector(origin, point, pointDistance, useRay);
                if (useRay)
                {
                    points = new Vector3[] {origin, extended};
                    rend.startWidth = pointSize;
                }
                else
                {
                    points = new Vector3[] {extended, extended};
                    rend.startWidth = pointSize;
                }
            }

            rend.SetPositions(points);
            rend.endWidth = pointSize;
        }

        #endregion

        void Start()
        {
            CameraReference = GetComponent<NeurableCamera>();
            if (!CameraReference) throw new MissingComponentException("No Neurable Camera Found");
        }

        void Update()
        {
            if (CursorActive) DrawPointInCameraspace(CursorRendererInstance, CameraReference.GazeToWorld());
        }
    }
}
