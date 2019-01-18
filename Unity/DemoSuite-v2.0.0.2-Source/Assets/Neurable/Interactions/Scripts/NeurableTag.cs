/*
 * Copyright 2017 Neurable Inc.
 */

using UnityEngine;
using UnityEngine.Events;
using System;
using Neurable.Core;

namespace Neurable.Interactions
{
    /*
     * Class handling Communication between Neurable API and in game objects illiciting visual stimulus
     */
    public class NeurableTag : MonoBehaviour, INeurableSelectable
    {
        [Header("Tag Properties")]
        [SerializeField]
        protected string Description = "";
        public float flashDuration = 0.1f; // duration of flash event

        protected bool dying = false;

        #region Event Properties

        [Header("Events")]
        public UnityEvent OnTrigger; // public, inspector driven Event called by Update when _ActivateSelection is true
        public UnityEvent
            OnAnimate; // public, inspector driven Event called by Update when _ActivateAnimation is true, or by animation controllers in scene
        public API.Types.TagCallback
            NeurableAnimationCallback, NeurableSelectionCallback; // Delegate Functions called directly by the API
        private bool _ActivateSelection = false;                  // Switch queried in Update loop for action triggers
        private bool
            _ActivateAnimation = false; // Switch queried in Update loop for animation triggers

        #endregion

        #region Projection Properties

        public enum VertexSource
        {
            BoxCollider,
            CapsuleCollider,
            MeshRenderer
        };

        [Header("Projection Properties")]
        public VertexSource BoundSource = VertexSource.BoxCollider;
        [SerializeField]
        protected Vector2 projectedPosition;
        [SerializeField]
        public float ScreenWidth, ScreenHeight;

        public Vector2 ProjectedPosition
        {
            get
            {
                UpdatePosition(); // do not access position without update
                return projectedPosition;
            }
        }

        #endregion

        #region MonoBehavior Startup / Teardown

        public virtual void Awake()
        {
            NeurableDescriptor = Description;
        }

        public virtual void OnDisable()
        {
            NeurableVisible = false;
            _ActivateSelection = false;
            _ActivateAnimation = false;
        }

        public virtual void OnBecameInvisible()
        {
            NeurableVisible = false;
        }

        public virtual void LateUpdate()
        {
            updatedPositionThisFrame = false;
        }

        void CleanMemory()
        {
            dying = true;
            if (neurableTag != null)
            {
                API.Library.DeleteTag(neurableTag.GetPointer());
            }

            neurableTag = null;
        }

        public virtual void OnDestroy()
        {
            CleanMemory();
        }

        #endregion

        #region Neurable Initialization

        public string NeurableDescriptor
        {
            get { return Description; }
            set
            {
                Description = (value == "") ? name : value;
                NeuroTag.SetDescription(Description);
            }
        }

        private API.Tag neurableTag;

        public API.Tag NeuroTag
        {
            get
            {
                // Return Neurable Tag. If unitialized, create it.
                if (neurableTag == null && !dying && Application.isPlaying)
                {
                    neurableTag = new API.Tag();
                    NeurableAnimationCallback = NeurableAnimationFunction;
                    neurableTag.SetAnimation(NeurableAnimationCallback);
                    NeurableSelectionCallback = NeurableSelectionFunction;
                    neurableTag.SetAction(NeurableSelectionCallback);
                    NeurableEnabled = false;
                    if (!NeurableUser.Instantiated) Debug.LogError(name + ": Could not find NeurableUser");
                    if (NeurableUser.Instance.NeurableCam == null)
                        Debug.LogError(name + ": Camera required to Update Positions");
                }

                return neurableTag;
            }
        }

        public int NeurableID
        {
            get { return NeuroTag.GetPointer(); }
        }

        #endregion

        #region Animation + Selection Hooks

        public virtual void Update()
        {
            // Perform actions/animations if triggered
            if (_ActivateSelection) Trigger();
            if (_ActivateAnimation) Animate();
            _ActivateSelection = false;
            _ActivateAnimation = false;

            // Update Positions if Active
            if (NeurableEnabled) UpdatePosition();
        }

#if CVR_NEURABLE
        private CognitiveVR.DynamicObject _cognitiveHandle;
        public CognitiveVR.DynamicObject CognitiveHandle
        {
            get
            {
                if (!_cognitiveHandle) _cognitiveHandle = GetComponent<CognitiveVR.DynamicObject>();
                return _cognitiveHandle;
            }
        }
#endif

        public void Trigger()
        {
            if (OnTrigger != null)
                OnTrigger.Invoke();
            else
                Debug.LogWarning("No Triggered Action for " + name);
            if (NeurableUser.Instantiated) NeurableUser.Instance.User.AcknowledgeSelection(NeuroTag);
#if CVR_NEURABLE
            if (!CognitiveVR.Core.Initialized) return;
            var tagSelect = new CognitiveVR.CustomEvent("Neurable Selection");
            if (CognitiveHandle) tagSelect.SetDynamicObject(CognitiveHandle.Id);
            tagSelect.Send(transform.position);
#endif
        }

        public void Animate()
        {
            if (OnAnimate != null) OnAnimate.Invoke();
        }

        // Callbacks to send to the API
        public virtual void NeurableSelectionFunction(int returnedTagID, IntPtr description, IntPtr unused)
        {
            _ActivateSelection = true;
        }

        public virtual void NeurableAnimationFunction(int returnedTagID, IntPtr description, IntPtr unused)
        {
            _ActivateAnimation = true;
        }

        // Artificially return Callbacks from the API for Testing
        public void simulateTagSelect()
        {
            NeuroTag.SimulateAction();
        }

        public void simulateTagAnim()
        {
            NeuroTag.SimulateAnimation();
        }

        #endregion

        #region Projection

        protected Vector3 WorldCenter;
        protected Vector3[] WorldVertices;
        protected Vector2[] ProjectedVertices;

        private bool updatedPositionThisFrame = false;

        /*
         * Updates the 
         */
        public virtual void UpdatePosition(bool forceRegistration = false)
        {
            if (!updatedPositionThisFrame) // only calculate once per frame
            {
                if (!isActiveAndEnabled) return;
                if ((!NeurableUser.Instantiated) || NeurableUser.Instance.NeurableCam == null)
                {
                    return;
                }

                updatedPositionThisFrame = true;
                switch (BoundSource)
                {
                    case VertexSource.MeshRenderer:
                        WorldCenter = GetCenterMeshRenderer();
                        WorldVertices = GetVerticesMeshRenderer();
                        break;
                    case VertexSource.CapsuleCollider:
                        WorldCenter = GetCenterCapCollider();
                        WorldVertices = GetVerticesCapCollider();
                        break;
                    case VertexSource.BoxCollider:
                    default:
                        WorldCenter = GetCenterBoxCollider();
                        WorldVertices = GetVerticesBoxCollider();
                        break;
                }

                projectedPosition = NeurableUser.Instance.NeurableCam.WorldToEyePoint(WorldCenter);
                ProjectedVertices = new Vector2[WorldVertices.Length];
                bool onscreen =
                    NeurableUser.Instance.NeurableCam
                                .IsOnScreen(projectedPosition); // if center is offscreen, treat as invisible
                if (!onscreen && !forceRegistration)
                {
                    NeurableVisible = false;
                    return;
                }

                float min_x = 10e9f, min_y = 10e9f, max_x = -10e9f, max_y = -10e9f;
                for (int i = 0; i < WorldVertices.Length; i++)
                {
                    ProjectedVertices[i] = NeurableUser.Instance.NeurableCam.WorldToEyePoint(WorldVertices[i]);
                    NeurableUser.Instance.NeurableCam.SnapEdgesToScreen(ref ProjectedVertices[i]);
                    if (ProjectedVertices[i].x != NeurableCamera.posErr.x)
                    {
                        max_x = Math.Max(max_x, ProjectedVertices[i].x);
                        min_x = Math.Min(min_x, ProjectedVertices[i].x);
                    }

                    if (ProjectedVertices[i].y != NeurableCamera.posErr.y)
                    {
                        min_y = Math.Min(min_y, ProjectedVertices[i].y);
                        max_y = Math.Max(max_y, ProjectedVertices[i].y);
                    }
                }

                ScreenWidth = max_x - min_x;
                ScreenHeight = max_y - min_y;
                if (NeurableEnabled) RegisterPosition();
                NeurableVisible = onscreen;
            }
        }

        private void RegisterPosition()
        {
            if (NeuroTag != null)
                NeuroTag.SetCameraPerspective(projectedPosition.x, projectedPosition.y, ScreenWidth, ScreenHeight);
        }

        // Use a Box Collider to determine the Bounding area of the Neurable Enabled Surface.

        #region Box Collider Vertices

        protected BoxCollider source_BoxCollider;

        protected virtual BoxCollider SourceBoxCollider
        {
            get
            {
                if (source_BoxCollider == null) source_BoxCollider = GetComponent<BoxCollider>();
                if (source_BoxCollider == null) source_BoxCollider = GetComponentInChildren<BoxCollider>(true);
                if (source_BoxCollider == null)
                {
                    Debug.LogError("No Box Collider on Obejct");
                    return new BoxCollider();
                }

                return source_BoxCollider;
            }
        }

        protected virtual Vector3 GetCenterBoxCollider()
        {
            if (SourceBoxCollider == null) return new Vector3(-10e13f, -10e13f, -10e13f);
            return transform.TransformPoint(SourceBoxCollider.center);
        }

        protected virtual Vector3[] GetVerticesBoxCollider()
        {
            if (SourceBoxCollider == null) return new Vector3[0];
            Vector3 center = SourceBoxCollider.center;
            Vector3 ext = SourceBoxCollider.size / 2;
            return new Vector3[8]
            {
                transform.TransformPoint(center + new Vector3(-ext.x, -ext.y, -ext.z)),
                transform.TransformPoint(center + new Vector3(+ext.x, -ext.y, -ext.z)),
                transform.TransformPoint(center + new Vector3(-ext.x, -ext.y, +ext.z)),
                transform.TransformPoint(center + new Vector3(+ext.x, -ext.y, +ext.z)),
                transform.TransformPoint(center + new Vector3(-ext.x, +ext.y, -ext.z)),
                transform.TransformPoint(center + new Vector3(+ext.x, +ext.y, -ext.z)),
                transform.TransformPoint(center + new Vector3(-ext.x, +ext.y, +ext.z)),
                transform.TransformPoint(center + new Vector3(+ext.x, +ext.y, +ext.z))
            };
        }

        #endregion

        // Use a Mesh Renderer's Bounds to determine the Bounding area of the Neurable Enabled Surface.

        #region Mesh Renderer Vertices

        protected Renderer source_MeshRenderer;
        bool warnFirstTime = false;

        protected virtual Renderer SourceMeshRenderer
        {
            get
            {
                if (source_MeshRenderer == null) source_MeshRenderer = GetComponent<Renderer>();
                if (source_MeshRenderer == null) source_MeshRenderer = GetComponentInChildren<Renderer>();

                if (source_MeshRenderer == null)
                {
                    Debug.LogError("No MeshRenderer on Obejct");
                    return new Renderer();
                }

                if (!warnFirstTime)
                {
                    SkinnedMeshRenderer test = source_MeshRenderer as SkinnedMeshRenderer;
                    if (test != null && !test.updateWhenOffscreen)
                        Debug.LogWarning("For Skinned Mesh Renderers, we suggest marking the Update When Offscreen feature");
                    warnFirstTime = true;
                }

                return source_MeshRenderer;
            }
        }

        protected virtual Vector3 GetCenterMeshRenderer()
        {
            if (SourceMeshRenderer == null) return new Vector3(-10e13f, -10e13f, -10e13f);
            return SourceMeshRenderer.bounds.center;
        }

        protected virtual Vector3[] GetVerticesMeshRenderer()
        {
            if (SourceMeshRenderer == null) return null;
            Vector3 center = SourceMeshRenderer.bounds.center;
            Vector3 ext = SourceMeshRenderer.bounds.extents;
            return new Vector3[8]
            {
                (center + new Vector3(-ext.x, -ext.y, -ext.z)),
                (center + new Vector3(+ext.x, -ext.y, -ext.z)),
                (center + new Vector3(-ext.x, -ext.y, +ext.z)),
                (center + new Vector3(+ext.x, -ext.y, +ext.z)),
                (center + new Vector3(-ext.x, +ext.y, -ext.z)),
                (center + new Vector3(+ext.x, +ext.y, -ext.z)),
                (center + new Vector3(-ext.x, +ext.y, +ext.z)),
                (center + new Vector3(+ext.x, +ext.y, +ext.z))
            };
        }

        #endregion

        // Use a Capsule Collider to determine the Bounding area of the Neurable Enabled Surface.

        #region Capsule Collider Vertices

        protected CapsuleCollider source_CapCollider;

        protected virtual CapsuleCollider SourceCapCollider
        {
            get
            {
                if (source_CapCollider == null) source_CapCollider = GetComponent<CapsuleCollider>();
                if (source_CapCollider == null) source_CapCollider = GetComponentInChildren<CapsuleCollider>(true);
                if (source_CapCollider == null)
                {
                    Debug.LogError("No CapsuleCollider on Obejct");
                    return new CapsuleCollider();
                }

                return source_CapCollider;
            }
        }

        protected virtual Vector3 GetCenterCapCollider()
        {
            if (SourceCapCollider == null) return new Vector3(-10e13f, -10e13f, -10e13f);
            return transform.TransformPoint(SourceCapCollider.center);
        }

        protected virtual Vector3[] GetVerticesCapCollider()
        {
            Vector3[] vertices;
            Vector3 center = SourceCapCollider.center;
            Vector3 ext;
            float capH = SourceCapCollider.height, capR = SourceCapCollider.radius;

            #region Determine Capsule Vertices

            switch (SourceCapCollider.direction)
            {
                case 0: // X Axis
                    ext = new Vector3(capH / 2 - capR, capR, capR);
                    vertices = new Vector3[10]
                    {
                        transform.TransformPoint(center + new Vector3(-ext.x, 0, -ext.z)),
                        transform.TransformPoint(center + new Vector3(+ext.x, 0, -ext.z)),
                        transform.TransformPoint(center + new Vector3(-ext.x, -ext.y, 0)),
                        transform.TransformPoint(center + new Vector3(+ext.x, -ext.y, 0)),
                        transform.TransformPoint(center + new Vector3(-ext.x, +ext.y, 0)),
                        transform.TransformPoint(center + new Vector3(+ext.x, +ext.y, 0)),
                        transform.TransformPoint(center + new Vector3(-ext.x, 0, +ext.z)),
                        transform.TransformPoint(center + new Vector3(+ext.x, 0, +ext.z)),
                        transform.TransformPoint(center + new Vector3(capH / 2, 0, 0)),
                        transform.TransformPoint(center - new Vector3(capH / 2, 0, 0))
                    };
                    break;
                case 1: // Y Axis
                    ext = new Vector3(capR, capH / 2 - capR, capR);
                    vertices = new Vector3[10]
                    {
                        transform.TransformPoint(center + new Vector3(0, -ext.y, -ext.z)),
                        transform.TransformPoint(center + new Vector3(+ext.x, -ext.y, 0)),
                        transform.TransformPoint(center + new Vector3(-ext.x, -ext.y, 0)),
                        transform.TransformPoint(center + new Vector3(0, -ext.y, +ext.z)),
                        transform.TransformPoint(center + new Vector3(0, +ext.y, -ext.z)),
                        transform.TransformPoint(center + new Vector3(+ext.x, +ext.y, 0)),
                        transform.TransformPoint(center + new Vector3(-ext.x, +ext.y, 0)),
                        transform.TransformPoint(center + new Vector3(0, +ext.y, +ext.z)),
                        transform.TransformPoint(center + new Vector3(0, capH / 2, 0)),
                        transform.TransformPoint(center - new Vector3(0, capH / 2, 0))
                    };
                    break;
                case 2: // Z Axis
                    ext = new Vector3(capR, capR, capH / 2 - capR);
                    vertices = new Vector3[10]
                    {
                        transform.TransformPoint(center + new Vector3(-ext.x, 0, -ext.z)),
                        transform.TransformPoint(center + new Vector3(0, -ext.y, -ext.z)),
                        transform.TransformPoint(center + new Vector3(-ext.x, 0, +ext.z)),
                        transform.TransformPoint(center + new Vector3(0, -ext.y, +ext.z)),
                        transform.TransformPoint(center + new Vector3(0, +ext.y, -ext.z)),
                        transform.TransformPoint(center + new Vector3(+ext.x, 0, -ext.z)),
                        transform.TransformPoint(center + new Vector3(0, +ext.y, +ext.z)),
                        transform.TransformPoint(center + new Vector3(+ext.x, 0, +ext.z)),
                        transform.TransformPoint(center + new Vector3(0, 0, capH / 2)),
                        transform.TransformPoint(center - new Vector3(0, 0, capH / 2))
                    };
                    break;
                default:
                    vertices = new Vector3[0];
                    break;
            }

            #endregion

            return vertices;
        }

        #endregion

        #region Debug Mesh

        // NeurableTag's `OnDrawGizmosSelected` will tell show you the box used to calculate screen positions
        private void OnDrawGizmosSelected()
        {
            if (!NeurableEnabled) return;
            if (WorldVertices == null) return;
            if (WorldVertices.Length < 8) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(WorldCenter, .05f);
            Gizmos.DrawLine(WorldVertices[1], WorldVertices[0]);
            Gizmos.DrawLine(WorldVertices[3], WorldVertices[1]);
            Gizmos.DrawLine(WorldVertices[3], WorldVertices[2]);
            Gizmos.DrawLine(WorldVertices[0], WorldVertices[2]);

            Gizmos.DrawLine(WorldVertices[5], WorldVertices[4]);
            Gizmos.DrawLine(WorldVertices[7], WorldVertices[5]);
            Gizmos.DrawLine(WorldVertices[7], WorldVertices[6]);
            Gizmos.DrawLine(WorldVertices[4], WorldVertices[6]);

            Gizmos.DrawLine(WorldVertices[5], WorldVertices[1]);
            Gizmos.DrawLine(WorldVertices[7], WorldVertices[3]);
            Gizmos.DrawLine(WorldVertices[2], WorldVertices[6]);
            Gizmos.DrawLine(WorldVertices[4], WorldVertices[0]);

            if (WorldVertices.Length < 10) return;
            Gizmos.DrawWireSphere(WorldVertices[8], .1f);
            Gizmos.DrawWireSphere(WorldVertices[9], .1f);
        }

        #endregion

        #endregion

        #region Active and Enabled

        /*
         * NeurableEnabled Indicates whether the Tag is relevent in the scene.
         * For example, if an ability is on cooldown, the indicator may be visible, but inactive.
         * When usable, the ability indicator has NeurableEnabled=true. On Cooldown, NeurableEnabled=false
         */
        [SerializeField]
        protected bool neurableEnabled = true;

        public virtual bool NeurableEnabled
        {
            get { return neurableEnabled; }
            set
            {
                if (NeuroTag == null) return;
                neurableEnabled = value;
                _ActivateSelection = false;
                _ActivateAnimation = false;
                NeurableActive = NeurableEnabled && NeurableVisible;
            }
        }

        /*
         * NeurableVisible indicates whether the Tag is actually visible in the scene
         * For example, a Target in World Space may always be selectable, but is not visible when the player isn't looking.
         * When the target comes back into view, it will be selectable again when NeurableEnabled==true && NeurableVisible==true
         */
        [SerializeField]
        protected bool neurableVisible = true;

        public virtual bool NeurableVisible
        {
            get
            {
                UpdatePosition(); // Update for Relevant Visibility
                return neurableVisible;
            }
            set
            {
                neurableVisible = value;
                NeurableActive = NeurableEnabled && neurableVisible;
            }
        }

        /*
         * NeurableActive is true when both NeurableEnabled && NeurableVisible are true.
         */
        bool lastActive = false;
        bool firstRun = false;

        protected virtual bool NeurableActive
        {
            set
            {
                // Update API on new Value. If newly active, register position before Active
                if (!firstRun || lastActive != value)
                {
                    if (value) RegisterPosition();
                    if (NeuroTag != null) NeuroTag.SetActive(value);
                    lastActive = value;
                    firstRun = true;
                    _ActivateSelection = false;
                    _ActivateAnimation = false;
                }
            }
        }

        #endregion
    }
}
