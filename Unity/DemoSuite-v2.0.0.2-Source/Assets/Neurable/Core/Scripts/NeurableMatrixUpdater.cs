//======= Copyright (c) Neurable, Inc. ========================================
//
// Author: Arnaldo E. Pereira
//
// Script to explore projection matrices for SteamVR-compatible headsets.
//
//=============================================================================

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
#endif
using UnityEngine;

#if NEURABLE_STEAMVR || NEURABLE_STEAMVR_2
using Valve.VR;

#endif


namespace Neurable.Core
{
    [RequireComponent(typeof(NeurableCamera))]
    public sealed class NeurableMatrixUpdater : MonoBehaviour
    {
        private NeurableCamera _neurableCam;

        private NeurableCamera NeurableCam
        {
            get
            {
                if (_neurableCam == null) NeurableCam = GetComponent<NeurableCamera>();
                return _neurableCam;
            }
            set { _neurableCam = value; }
        }

        public void ResetParameters(ref Camera targetCam)
        {
#if NEURABLE_STEAMVR || NEURABLE_STEAMVR_2
            if (NeurableCam.debugMode) Debug.Log("Resetting Neurable Matrix Updater");
            if (targetCam == null)
            {
                if (NeurableCam.debugMode) Debug.LogError("No PlayerCam, please wait for new Camera to come online.");
                return;
            }

            GetNewProjectionPropertiesAndSetChangeFlag(ref targetCam);
            InitializeBinocularProjectionMatrices();
            CalculateVrMatrices();
            CalculateWorldMatrices(ref targetCam);
            SendAllMatricesToApi();
            SetOldProjectionPropertiesAndSetChangeFlag();
#endif
        }

        public Vector3 ScreenPointToWorld(Vector2 screenPoint)
        {
            Vector3 outVec;
            if (!NeurableCam.UseVrTransforms)
            {
                var localPos = new Vector3(screenPoint.x - NeurableCam.hRes / 2f, screenPoint.y - NeurableCam.vRes / 2f,
                                           100f);
                outVec = NeurableCam.PlayerCam.transform.TransformPoint(localPos);
            }
            else
            {
#if NEURABLE_STEAMVR || NEURABLE_STEAMVR_2
                var screenPointHomogeneous =
                    new Vector4(screenPoint.x, screenPoint.y, 1.0f, 1.0f); //Near clip is at z buffer = 1 in Neurable.
                var posHeadHomogeneous = _headFromBinocularProjection * screenPointHomogeneous;
                if (posHeadHomogeneous.w != 0f)
                    posHeadHomogeneous /=
                        posHeadHomogeneous.w; //Pos in m at nearClip from Neurable camera in OpenGL camera space.
                Vector3 posHead = posHeadHomogeneous;
                outVec = _worldFromHead.MultiplyPoint3x4(posHead);
#else
                var localPos =
                    new Vector3(screenPoint.x - NeurableCam.hres / 2f, screenPoint.y - NeurableCam.vres / 2f, 100f);
                outVec = NeurableCam.PlayerCam.transform.TransformPoint(localPos);
#endif
            }

            return outVec;
        }
#if NEURABLE_STEAMVR || NEURABLE_STEAMVR_2
//Projection and transform matrices that will be passed to the API
//Pass that and its inverse to the API as well.
// [SerializeField]
        private Matrix4x4 _rightEyeFromRightProjection,
                          _leftEyeFromLeftProjection,
                          _headFromRightEye,
                          _headFromLeftEye,
                          _rightProjectionFromRightEye,
                          _leftProjectionFromLeftEye,
                          _leftEyeFromHead,
                          _rightEyeFromHead,
                          _binocularProjectionFromHead,
                          _headFromBinocularProjection,
                          _headFromWorld,
                          _worldFromHead;

        private float _nearClip = 0.1f,
                      _previousNearClip,
                      _farClip = 10000f,
                      _previousFarClip,
                      _ipd,
                      _previousIpd,
                      _headToEyeDepth,
                      _previousHeadToEyeDepth;

        private bool _changeFlag;

        //To interface with SteamVR
        private CVRSystem _headsetSingleton;
        private SteamVR _steamVRSingleton;
        private EVREye _leftEye;
        private EVREye _rightEye;

        private void Awake()
        {
            //Initialize headset and camera interfaces.
            _steamVRSingleton = SteamVR.instance;
            if (_steamVRSingleton != null) _headsetSingleton = _steamVRSingleton.hmd;
            else Debug.LogError("No VR HMD Available for Neurable Transforms.");
            _leftEye = EVREye.Eye_Left;
            _rightEye = EVREye.Eye_Right;
        }

        private void FixedUpdate()
        {
            if (!NeurableUser.Instantiated) return;
            var targetCam = NeurableCam.PlayerCam;
            GetNewProjectionPropertiesAndSetChangeFlag(ref targetCam);
            if (_changeFlag)
            {
                CalculateVrMatrices();
                CalculateWorldMatrices(ref targetCam);
                SendAllMatricesToApi();
                SetOldProjectionPropertiesAndSetChangeFlag();
                return;
            }

            CalculateWorldMatrices(ref targetCam);
            SendWorldMatricesToApi();
            SetOldProjectionPropertiesAndSetChangeFlag();
        }

        /*
            * Sets the binocular projection matrix (and its inverse) matching
            * the projection used for setting tag screen positions in
            */
        private void InitializeBinocularProjectionMatrices()
        {
            if (NeurableCam == null)
            {
                Debug.LogError(name + ": Neurable Camera required to update VR Matrices");
                return;
            }

            _binocularProjectionFromHead =
                NeurableCam.viveResFromCanonicalViewport * NeurableCam.canonicalViewportFromHeadCam;
            _headFromBinocularProjection = _binocularProjectionFromHead.inverse;
        }

        //Update OpenVR projection matrices and their inverses
        private void CalculateVrMatrices()
        {
            _leftProjectionFromLeftEye = GetEyeProjection(_leftEye, _nearClip, _farClip);
            _rightProjectionFromRightEye = GetEyeProjection(_rightEye, _nearClip, _farClip);
            _leftEyeFromLeftProjection = _leftProjectionFromLeftEye.inverse;
            _rightEyeFromRightProjection = _rightProjectionFromRightEye.inverse;

            _headFromLeftEye = GetHeadFromEyeTransform(_leftEye);
            _headFromRightEye = GetHeadFromEyeTransform(_rightEye);
            _leftEyeFromHead = _headFromLeftEye.inverse;
            _rightEyeFromHead = _headFromRightEye.inverse;
        }

        //Update HeadFromWorld (Unity transform) and its inverse
        private void CalculateWorldMatrices(ref Camera targetCam)
        {
            if (!targetCam) return;
            _headFromWorld = targetCam.worldToCameraMatrix;
            _worldFromHead = _headFromWorld.inverse;
        }

        private void SendAllMatricesToApi()
        {
            SendMatrixToApi(API.Types.ProjectionMatrix.RightEyeFromRightProjection, _rightEyeFromRightProjection);
            SendMatrixToApi(API.Types.ProjectionMatrix.LeftEyeFromLeftProjection, _leftEyeFromLeftProjection);
            SendMatrixToApi(API.Types.ProjectionMatrix.HeadFromRightEye, _headFromRightEye);
            SendMatrixToApi(API.Types.ProjectionMatrix.HeadFromLeftEye, _headFromLeftEye);
            SendMatrixToApi(API.Types.ProjectionMatrix.RightProjectionFromRightEye, _rightProjectionFromRightEye);
            SendMatrixToApi(API.Types.ProjectionMatrix.LeftProjectionFromLeftEye, _leftProjectionFromLeftEye);
            SendMatrixToApi(API.Types.ProjectionMatrix.LeftEyeFromHead, _leftEyeFromHead);
            SendMatrixToApi(API.Types.ProjectionMatrix.RightEyeFromHead, _rightEyeFromHead);
            SendMatrixToApi(API.Types.ProjectionMatrix.BinocularProjectionFromHead, _binocularProjectionFromHead);
            SendMatrixToApi(API.Types.ProjectionMatrix.HeadFromBinocularProjection, _headFromBinocularProjection);
            SendMatrixToApi(API.Types.ProjectionMatrix.HeadFromWorld, _headFromWorld);
            SendMatrixToApi(API.Types.ProjectionMatrix.WorldFromHead, _worldFromHead);
        }

        private void SendWorldMatricesToApi()
        {
            SendMatrixToApi(API.Types.ProjectionMatrix.HeadFromWorld, _headFromWorld);
            SendMatrixToApi(API.Types.ProjectionMatrix.WorldFromHead, _worldFromHead);
        }

        private static void SendMatrixToApi(Neurable.API.Types.ProjectionMatrix matrixId, Matrix4x4 matrix)
        {
            if (!NeurableUser.Instantiated) return;
            var openGlMatrix = Matrix4X4ToOpenGlMatrix(matrix);
            NeurableUser.Instance.User.SetHmdMatrix(matrixId, ref openGlMatrix);
        }

        private void GetNewProjectionPropertiesAndSetChangeFlag(ref Camera targetCam)
        {
            if (targetCam)
            {
                _nearClip = targetCam.nearClipPlane;
                _farClip = targetCam.farClipPlane;
            }

            _ipd = GetIpd();
            _headToEyeDepth = GetHeadToEyeDepth();

            if (_nearClip != _previousNearClip || _farClip != _previousFarClip
                                               || _ipd != _previousIpd || _headToEyeDepth != _previousHeadToEyeDepth)
                _changeFlag = true;
        }

        private void SetOldProjectionPropertiesAndSetChangeFlag()
        {
            _previousNearClip = _nearClip;
            _previousFarClip = _farClip;
            _previousIpd = _ipd;
            _previousHeadToEyeDepth = _headToEyeDepth;

            _changeFlag = false;
        }

        //Gets the headset IPD (in meters) from SteamVR.
        private float GetIpd()
        {
            if (_headsetSingleton == null) return 0.0f;
            var val = _steamVRSingleton.GetFloatProperty(ETrackedDeviceProperty.Prop_UserIpdMeters_Float);
            return val;
        }

        //Gets the distance from the headset screen to the neurableUser.User's eyes(?) (in meters) from SteamVR.
        private float GetHeadToEyeDepth()
        {
            if (_headsetSingleton == null) return 0.0f;
            var val = _steamVRSingleton.GetFloatProperty(ETrackedDeviceProperty.Prop_UserHeadToEyeDepthMeters_Float);
            return val;
        }

        private Matrix4x4 GetEyeProjection(EVREye eye, float nearZ, float farZ)
        {
            if (_headsetSingleton == null) return new Matrix4x4();
            var svrMat = _headsetSingleton.GetProjectionMatrix(eye, nearZ, farZ);
            var unityMat = Matrix4x4.identity;

            unityMat[0, 0] = svrMat.m0;
            unityMat[0, 1] = svrMat.m1;
            unityMat[0, 2] = svrMat.m2;
            unityMat[0, 3] = svrMat.m3;

            unityMat[1, 0] = svrMat.m4;
            unityMat[1, 1] = svrMat.m5;
            unityMat[1, 2] = svrMat.m6;
            unityMat[1, 3] = svrMat.m7;

            unityMat[2, 0] = svrMat.m8;
            unityMat[2, 1] = svrMat.m9;
            unityMat[2, 2] = svrMat.m10;
            unityMat[2, 3] = svrMat.m11;

            unityMat[3, 0] = svrMat.m12;
            unityMat[3, 1] = svrMat.m13;
            unityMat[3, 2] = svrMat.m14;
            unityMat[3, 3] = svrMat.m15;

            return unityMat;
        }

        private Matrix4x4 GetHeadFromEyeTransform(EVREye eye)
        {
            var unityMat = Matrix4x4.identity;

            unityMat[0, 3] = eye == _leftEye ? -1.0f * (_ipd / 2.0f) : _ipd / 2.0f;
            unityMat[2, 3] = -_headToEyeDepth;

            return unityMat;
        }

        private static Neurable.API.Types.OpenGLMatrix Matrix4X4ToOpenGlMatrix(Matrix4x4 inM)
        {
            Neurable.API.Types.OpenGLMatrix outM;

            outM.m00 = inM[0, 0];
            outM.m01 = inM[0, 1];
            outM.m02 = inM[0, 2];
            outM.m03 = inM[0, 3];

            outM.m10 = inM[1, 0];
            outM.m11 = inM[1, 1];
            outM.m12 = inM[1, 2];
            outM.m13 = inM[1, 3];

            outM.m20 = inM[2, 0];
            outM.m21 = inM[2, 1];
            outM.m22 = inM[2, 2];
            outM.m23 = inM[2, 3];

            outM.m30 = inM[3, 0];
            outM.m31 = inM[3, 1];
            outM.m32 = inM[3, 2];
            outM.m33 = inM[3, 3];

            return outM;
        }

#endif
    }
}