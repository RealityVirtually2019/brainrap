/*
* Copyright 2017 Neurable Inc.
*/

using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;

#endif
#if NEURABLE_STEAMVR_2
using Valve.VR;
#endif

namespace Neurable.Core
{
    // Converts a 3D Transform to an XY Screen Transform
    [RequireComponent(typeof(NeurableMatrixUpdater))]
    public class NeurableCamera : MonoBehaviour
    {
        public static Vector2 posErr = new Vector2(-1000000, -1000000);
        private NeurableMatrixUpdater _matrixUpdater;

        [FormerlySerializedAs("selectedCam")] [SerializeField] [Tooltip("Explicit Reference to the Selected Camera")]
        private Camera _playerCam;
        private Camera _mainCam;

        [Tooltip(
            "When True, PlayerCam will always default to the active SteamVR_Camera object OR Camera.Main. When False, PlayerCam must be set Explicitly via PlayerCam Property or Inspector")]
        public bool useMainCameraAlways = true; // Always target Camera.Main

        private RaycastHit[] _rayResults;
        private bool _runThisFrame, _usingEyes, _usingVr, _vrChecked, _initializedCam, _dataReady;

        [Header("Debug Attributes")]
        [Tooltip("Show Debug Logs as Camera Properties change, update FocalPoint Attribute on Update")]
        public bool debugMode;

        [SerializeField] [Tooltip("Last Known Eye Position, updated on demand (at most once a frame)")]
        private Vector2 _focalPoint;
        private Vector2 _midScreen;

        [HideInInspector] public float hFov, vFov, vRes, hRes;

        private NeurableMatrixUpdater MatrixUpdater
        {
            get
            {
                if (_matrixUpdater == null) _matrixUpdater = GetComponent<NeurableMatrixUpdater>();
                return _matrixUpdater;
            }
        }

        public bool UseVrTransforms
        {
            get
            {
                if (!_vrChecked)
                {
                    _usingEyes = true;
#if UNITY_2017_2_OR_NEWER
					usingVR = (XRDevice.isPresent && XRSettings.loadedDeviceName != "none" && XRSettings.enabled);
#else
                    _usingVr = VRDevice.isPresent && VRSettings.loadedDeviceName != "none" && VRSettings.enabled;
#endif
                    _vrChecked = true;
                }

                return _usingEyes && _usingVr;
            }
        }

        public Camera PlayerCam
        {
            get
            {
#if NEURABLE_STEAMVR || NEURABLE_STEAMVR_2
                if (!_runThisFrame && useMainCameraAlways && (_playerCam == null || !_playerCam.isActiveAndEnabled))
                {
                    _runThisFrame = true;
                    var steamCam = FindObjectOfType<SteamVR_Camera>();
                    if (steamCam)
                    {
                        _mainCam = steamCam.camera;
                    }
                    else
                    {
                        _mainCam = Camera.main;
                        if (_mainCam && _mainCam.cullingMask == 0)
                        {
                            var cams = _mainCam.gameObject.GetComponentsInChildren<Camera>();
                            var i = 0;
                            for (; i < cams.Length; ++i)
                            {
                                var cam = cams[i];
                                if (cam == _mainCam) continue;
                                _mainCam = cam;
                                break;
                            }
                        }
                    }

                    if (_mainCam != null && _mainCam != _playerCam)
                        PlayerCam = _mainCam;
                }
#else
                if (!_runThisFrame && useMainCameraAlways &&
                    (_playerCam == null || !_playerCam.isActiveAndEnabled || !_playerCam.CompareTag("MainCamera")))
                {
                    _runThisFrame = true;
                    _mainCam = Camera.main;
                    if (_mainCam && _mainCam.cullingMask == 0)
                    {
                        var cams = _mainCam.gameObject.GetComponentsInChildren<Camera>();
                        for (var i = 0; i < cams.Length; ++i)
                        {
                            var cam = cams[i];
                            if (cam != _mainCam)
                            {
                                _mainCam = cam;
                                break;
                            }
                        }
                    }

                    if (_mainCam != null && _mainCam != _playerCam)
                        PlayerCam = _mainCam;
                }
#endif
                if (debugMode && _playerCam == null)
                    Debug.LogError("No Camera Object Found. No Camera.main object or no Targeted Camera");
                if (!_initializedCam) SetResolution(_playerCam);
                return _playerCam;
            }
            set
            {
                if (!NeurableUser.Instantiated || value == null || _initializedCam && value == _playerCam) return;
                if (debugMode) Debug.Log("New Camera Found: " + value.name);
                _playerCam = value;
                SetResolution(_playerCam);
            }
        }

        public Vector2 FocalPoint
        {
            get
            {
                if (!_dataReady) _dataReady = NeurableUser.Instance.Ready; // Reduce API Calls.
                var timestamp = 0.0;
                if (UseVrTransforms && NeurableUser.Instance.User != null && _dataReady &&
                    NeurableUser.Instance.GetEyeData(out timestamp, out _focalPoint))
                    return _focalPoint;
                _focalPoint = _midScreen;
                return _focalPoint;
            }
        }

        public Vector2 NormalizedFocalPoint
        {
            get
            {
                var normalizedFocal = FocalPoint;
                normalizedFocal.x /= hRes;
                normalizedFocal.y /= vRes;
                if (normalizedFocal.x > 1f) normalizedFocal.x = 1f;
                if (normalizedFocal.y > 1f) normalizedFocal.y = 1f;
                if (normalizedFocal.x < 0f) normalizedFocal.x = 0f;
                if (normalizedFocal.y < 0f) normalizedFocal.y = 0f;
                return normalizedFocal;
            }
        }

        private void Update()
        {
            if (debugMode && Application.isEditor) _focalPoint = FocalPoint;
            if (PlayerCam != null) headCamFromWorld = PlayerCam.worldToCameraMatrix;
        }

        private void LateUpdate()
        {
            _runThisFrame = false;
        }

        private void SetResolution(Camera targetCam)
        {
            if (!NeurableUser.Instantiated || !targetCam)
            {
                _initializedCam = false;
                return;
            }

            if (debugMode) Debug.Log("Resetting Neurable Camera");
            if (UseVrTransforms)
            {
                SetResolution_VR(targetCam);
            }
            else
            {
                hRes = targetCam.pixelWidth;
                vRes = targetCam.pixelHeight;
                hFov = vFov = targetCam.fieldOfView;
                NeurableUser.Instance.User.SetCameraResolution(hRes, vRes);
                NeurableUser.Instance.User.SetCameraFov(hFov, vFov, targetCam.nearClipPlane, targetCam.farClipPlane);
            }

            _midScreen = new Vector2((hRes - 1.0f) / 2.0f, (vRes - 1.0f) / 2.0f);
            MatrixUpdater.ResetParameters(ref targetCam);
            _initializedCam = true;
        }

        public Vector2 WorldToEyePoint(Vector3 pos)
        {
            if (UseVrTransforms) return WorldToEyePoint_VR(pos);
            if (PlayerCam == null) return Vector2.zero;
            return PlayerCam.WorldToScreenPoint(pos);
        }

        public float FocalDistance(Vector2 position)
        {
            return Vector2.Distance(position, FocalPoint);
        }

        public bool IsOnScreen(Vector2 position)
        {
            return position.x <= hRes && position.x >= 0 && position.y <= vRes && position.y >= 0;
        }

        public void SnapEdgesToScreen(ref Vector2 position)
        {
            if (position.x > hRes && position.x < 2 * hRes) position.x = hRes;
            if (position.y > vRes && position.y < 2 * vRes) position.y = vRes;
            if (position.x < 0f && position.x > -hRes) position.x = 0f;
            if (position.y < 0f && position.y > -vRes) position.y = 0f;
        }

        public Vector3 GazeToWorld()
        {
            if (MatrixUpdater)
                return MatrixUpdater.ScreenPointToWorld(FocalPoint);

            Debug.LogError("Gaze Functions Require a functional NeurableMatrixUpdater script and SteamVR");
            return posErr;
        }

        public Vector3 ScreenPointToWorld(Vector2 screenPoint)
        {
            if (MatrixUpdater)
                return MatrixUpdater.ScreenPointToWorld(screenPoint);

            Debug.LogError("World Camera Functions Require a functional NeurableMatrixUpdater script and SteamVR");
            return posErr;
        }

        public Ray GazeRay()
        {
            if (PlayerCam == null) return new Ray();
            return new Ray(PlayerCam.transform.position,
                           Vector3.Normalize(GazeToWorld() - PlayerCam.transform.position));
        }

        public bool GazeRaycast(out RaycastHit hitInfo, float maxDistance = Mathf.Infinity,
                                int layerMask = Physics.DefaultRaycastLayers,
                                QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            if (PlayerCam == null)
            {
                hitInfo = new RaycastHit();
                return false;
            }

            return Physics.Raycast(GazeRay(), out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        }

        public bool GazeSpherecast(float radius, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity,
                                   int layerMask = Physics.DefaultRaycastLayers,
                                   QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            if (PlayerCam == null)
            {
                hitInfo = new RaycastHit();
                return false;
            }

            return Physics.SphereCast(GazeRay(), radius, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        }

        public RaycastHit[] GazeRaycastAll(float maxDistance = Mathf.Infinity,
                                           int layerMask = Physics.DefaultRaycastLayers,
                                           QueryTriggerInteraction queryTriggerInteraction =
                                               QueryTriggerInteraction.UseGlobal)
        {
            if (PlayerCam == null) return null;
            if (_rayResults == null) _rayResults = new RaycastHit[20];
            if (Physics.RaycastNonAlloc(GazeRay(), _rayResults, maxDistance, layerMask, queryTriggerInteraction) <= 0)
                _rayResults = null;
            return _rayResults;
        }

        public RaycastHit[] GazeSpherecastAll(float radius, float maxDistance = Mathf.Infinity,
                                              int layerMask = Physics.DefaultRaycastLayers,
                                              QueryTriggerInteraction queryTriggerInteraction =
                                                  QueryTriggerInteraction.UseGlobal)
        {
            if (PlayerCam == null) return null;
            if (_rayResults == null) _rayResults = new RaycastHit[20];
            if (Physics.SphereCastNonAlloc(GazeRay(), radius, _rayResults, maxDistance, layerMask,
                                           queryTriggerInteraction) <= 0)
                _rayResults = null;
            return _rayResults;
        }


        #region VR Specific Functions

        private readonly float _hFovVr = 1.9530234f; //Horizontal FOV SMI gives for Vive, in radians

        private readonly float _vFovVr = 1.8430677f; //Vertical FOV SMI gives for Vive, in radians

        private readonly float  _vResVr = 1200.0f; //Vertical resolution of the Vive in pixels, as a float

        private float _nearClip = -0.1f, _farClip = -10000f;

        protected float headToEyeDepth = 0.015f; //Distance from eyes (screens) to head (point between eyes)

        [HideInInspector] public Matrix4x4 headCamFromWorld, canonicalViewportFromHeadCam, viveResFromCanonicalViewport;

        // Initialize vector transforms
        private void SetResolution_VR(Camera targetCam)
        {
            if (targetCam != null)
            {
                var ncp = targetCam.nearClipPlane;
                var fcp = targetCam.farClipPlane;
                if (ncp == 0f) ncp = 0.1f;
                _nearClip = -1.0f * ncp;
                _farClip = -1.0f * fcp;
            }

            vRes = _vResVr;
            hFov = _hFovVr;
            vFov = _vFovVr;
            hRes = _vResVr * Mathf.Tan(hFov / 2.0f) / Mathf.Tan(vFov / 2.0f);

            /*
             * Perspective projection to camera for both eyes matching given headset
             * specs. Follows OpenGL convention of z axis being negative in the direction
             * in which the camera is looking.
             */
            canonicalViewportFromHeadCam = new Matrix4x4();
            canonicalViewportFromHeadCam[0, 0] = -1.0f / Mathf.Tan(hFov / 2.0f);
            canonicalViewportFromHeadCam[0, 1] = 0.0f;
            canonicalViewportFromHeadCam[0, 2] = 0.0f;
            canonicalViewportFromHeadCam[0, 3] = 0.0f;

            canonicalViewportFromHeadCam[1, 0] = 0.0f;
            canonicalViewportFromHeadCam[1, 1] = -1.0f / Mathf.Tan(vFov / 2.0f);
            canonicalViewportFromHeadCam[1, 2] = 0.0f;
            canonicalViewportFromHeadCam[1, 3] = 0.0f;

            canonicalViewportFromHeadCam[2, 0] = 0.0f;
            canonicalViewportFromHeadCam[2, 1] = 0.0f;
            canonicalViewportFromHeadCam[2, 2] = (_nearClip + _farClip) / (_nearClip - _farClip);
            canonicalViewportFromHeadCam[2, 3] = -2.0f * _farClip * _nearClip / (_nearClip - _farClip);

            canonicalViewportFromHeadCam[3, 0] = 0.0f;
            canonicalViewportFromHeadCam[3, 1] = 0.0f;
            canonicalViewportFromHeadCam[3, 2] = 1.0f;
            canonicalViewportFromHeadCam[3, 3] = 0.0f;

            /*
            * Transform to screen space matching the binocular resolution for the given
            * headset specs (1:1 pixel aspect ratio). Follows OpenGL convention of
            * (0,0) being in the lower left corner.
            */
            viveResFromCanonicalViewport = new Matrix4x4();
            viveResFromCanonicalViewport[0, 0] = hRes / 2.0f;
            viveResFromCanonicalViewport[0, 1] = 0.0f;
            viveResFromCanonicalViewport[0, 2] = 0.0f;
            viveResFromCanonicalViewport[0, 3] = (hRes - 1.0f) / 2.0f;

            viveResFromCanonicalViewport[1, 0] = 0.0f;
            viveResFromCanonicalViewport[1, 1] = vRes / 2.0f;
            viveResFromCanonicalViewport[1, 2] = 0.0f;
            viveResFromCanonicalViewport[1, 3] = (vRes - 1.0f) / 2.0f;

            viveResFromCanonicalViewport[2, 0] = 0.0f;
            viveResFromCanonicalViewport[2, 1] = 0.0f;
            viveResFromCanonicalViewport[2, 2] = 1.0f;
            viveResFromCanonicalViewport[2, 3] = 0.0f;

            viveResFromCanonicalViewport[3, 0] = 0.0f;
            viveResFromCanonicalViewport[3, 1] = 0.0f;
            viveResFromCanonicalViewport[3, 2] = 0.0f;
            viveResFromCanonicalViewport[3, 3] = 1.0f;

            NeurableUser.Instance.User.SetCameraResolution(hRes, vRes);
            NeurableUser.Instance.User.SetCameraFov(hFov, vFov, _nearClip, _farClip);
        }

        // Takes a 3D position and projects it into screen space
        private Vector2 WorldToEyePoint_VR(Vector3 pos)
        {
            if (PlayerCam == null) return Vector2.zero;

            var posC3 = headCamFromWorld.MultiplyPoint3x4(pos);

            if (posC3.z >= 0) return posErr;

            var posC4 = new Vector4(posC3.x, posC3.y, posC3.z, 1.0f);

            var posP4 = canonicalViewportFromHeadCam * posC4;
            posP4 /= posP4.w;

            Vector3 posP3 = posP4;
            var posS3 = viveResFromCanonicalViewport.MultiplyPoint3x4(posP3);

            if (posS3.z < -1 || posS3.z > 1) return posErr;

            var posS2 = new Vector2(posS3.x, posS3.y);

            return posS2;
        }

        #endregion
    }
}