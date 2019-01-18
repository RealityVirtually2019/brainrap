using UnityEngine;

namespace Neurable.Diagnostics
{
    public class UserStatePanel : MonoBehaviour
    {
        public GameObject GeneralNotification;
        public GameObject EyeNotReady;
        public GameObject BrainNotReady;

        private bool _eyeReady;
        private bool _brainReady;
        private bool _userReady;

        //Note: the UserReady icon will not reappear, but the eye and brain ones can
        public void SetState(bool eyeReady, bool brainReady, bool userReady)
        {
            if (userReady && !_userReady)
            {
                HideLogo();
                _userReady = userReady;
            }

            if (eyeReady != _eyeReady)
            {
                EyeNotReady.SetActive(!eyeReady);
                _eyeReady = eyeReady;
            }

            if (brainReady != _brainReady)
            {
                BrainNotReady.SetActive(!brainReady);
                _brainReady = brainReady;
            }
        }

        public void DisableNotifications()
        {
            SetState(eyeReady: true, brainReady: true, userReady: true);
        }

        private void HideLogo()
        {
            if (GeneralNotification == null) return;

            var logoClear = GeneralNotification.GetComponent<LogoClear>();
            if (logoClear)
            {
                logoClear.ClearLogo();
            }
        }
    }
}
