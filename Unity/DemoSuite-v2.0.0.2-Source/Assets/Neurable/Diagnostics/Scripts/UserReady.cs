/*
 * Copyright 2017 Neurable Inc.
 */
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Neurable.Core;

namespace Neurable.Diagnostics
{
    /*
     * Script that creates a User Connection notification
     */
    public class UserReady : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAsAttribute("OnUserReady"), Tooltip("Events to call when the User becomes ready.")]
        public UnityEvent onUserReady;

        [Header("Visual Notification Options (Optional)")]
        [Tooltip("Panel for visual notifications. Optional")]
        public UserStatePanel userStatePanelPrefab;
        [Tooltip("Camera pivot for visual notifications.")]
        public Camera cameraLock;
        [Tooltip("Distance from Camera for visual notifications.")]
        public float distance = 1f;
        [Tooltip("Switch for disabling the Notification")]
        public bool disableNotification = false;

        private UserStatePanel _userStatePanel;
        private Coroutine _checkStateRoutine;
        private bool _userBeenReady;

        public virtual void Start()
        {
            if (userStatePanelPrefab)
            {
                _userStatePanel = Instantiate(userStatePanelPrefab);
                _userStatePanel.transform.SetParent(NeurableUser.Instance.transform, false);
            }

            _checkStateRoutine = StartCoroutine(CheckUserStateRoutine());
        }

        Camera PlayerCam
        {
            get
            {
                if (cameraLock == null)
                {
                    if (NeurableUser.Instantiated && NeurableUser.Instance.NeurableCam)
                        cameraLock = NeurableUser.Instance.NeurableCam.PlayerCam;
                }

                return cameraLock;
            }
        }

        public virtual void Update()
        {
            if (_userStatePanel)
            {
                Transform pivot = transform;
                if (PlayerCam) pivot = PlayerCam.transform;
                _userStatePanel.transform.position = pivot.position + pivot.forward * distance;
                _userStatePanel.transform.rotation = pivot.rotation;
            }
        }

        private IEnumerator CheckUserStateRoutine()
        {
            var waitForSeconds = new WaitForSecondsRealtime(.5f);
            while (true)
            {
                if (disableNotification)
                {
                    if (_userStatePanel) _userStatePanel.DisableNotifications();
                    yield return waitForSeconds;
                    continue;
                }

                var userReady = false;
                var brainReady = false;
                var eyeReady = false;

                if (NeurableUser.Instantiated)
                {
                    brainReady = NeurableUser.Instance.User.IsReadyEeg();
                    if (NeurableUser.Instance.UseHybridEyeSystem)
                    {
                        eyeReady = NeurableUser.Instance.User.IsReadyEye();
                    }
                    else
                    {
                        eyeReady = true;
                    }

                    userReady = NeurableUser.Instance.Ready;
                }

                if (_userStatePanel) _userStatePanel.SetState(eyeReady, brainReady, userReady);

                if (!_userBeenReady && userReady)
                {
                    _userBeenReady = true;
#if CVR_NEURABLE
                    Analytics.Portal.NeurableCognitiveInterface.UserReadyEvent();
#endif
                    onUserReady.Invoke();
                    waitForSeconds = new WaitForSecondsRealtime(3f);
                }

                yield return waitForSeconds;
            }
        }

        protected virtual void OnDisable()
        {
            if (_checkStateRoutine != null) StopCoroutine(_checkStateRoutine);
            if (_userStatePanel) Destroy(_userStatePanel);
        }
    }
}
