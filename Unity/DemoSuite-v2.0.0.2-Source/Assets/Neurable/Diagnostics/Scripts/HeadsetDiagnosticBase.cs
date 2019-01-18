/*
* Copyright 2017 Neurable Inc.
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Neurable.Core;

namespace Neurable.Diagnostics
{
    public abstract class HeadsetDiagnosticBase : MonoBehaviour
    {
        [Header("Sampling Options")]
        [SerializeField, Tooltip("Number of times per second to poll EEG Quality. Sample Rate is limited to frequency of Unity Update(). If 0, will default to every Unity Frame.")]
        public float sampleRateHz = 5f;
        [Tooltip("When enabled, Utility will trigger a baseline reset every 20 seconds while actively polling data.")]
        public bool periodicReset = false;

        [Header("Countdown Text Indicator")]
		public string refreshButtonString = "(R)efresh";
		public Text countDownText;

		[Header("Baseline Reset Button")]
		public Button resetButton;
		public KeyCode triggerButton = KeyCode.R;

		[Header("Color Options")]
        public Color[] nodeColors = { Color.green, Color.yellow, Color.red, Color.grey }; //Setup in Editor // needs to have 3 elements
        
        public GameObject disableDuringCountDown;

        protected Dictionary<string, string> electrodeColors;
        private float _countdown = 0f;
        private bool _countingDown = false;
        private bool _allowReset = true;
        private int _channelCount = 0;
        private float _timeOfLastRequest = 0.0f;

        protected virtual void Awake()
        {
            electrodeColors = new Dictionary<string, string>();
            _timeOfLastRequest = Time.time;
        }

        private bool TimeToUpdate {
            get
            {
                if (sampleRateHz == 0f)
                    return true;
                return (Time.time - _timeOfLastRequest >= 1 / sampleRateHz);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(triggerButton) && _allowReset)
            {
                ResetBaseline();
            }

            if (TimeToUpdate)
            {
                RequestDiagnostics();
                PopulateChannels();
                UpdateColors();
                HandleCountdown();
                _timeOfLastRequest = Time.time;
            }
        }

        protected abstract void PopulateChannels();

        public void ResetBaseline()
        {
            if ((!NeurableUser.Instantiated) || !isActiveAndEnabled ||
                !gameObject.activeInHierarchy || !_allowReset)
            {
                return;
            }

            NeurableUser.Instance.User.ResetDiagnosticBaseline();
#if CVR_NEURABLE
            Analytics.Portal.NeurableCognitiveInterface.BaselineResetEvent();
#endif
        }

        public void AllowBaselineReset(bool val)
        {
            _allowReset = val;
        }
        
        public void RequestDiagnostics()
        {
            if (!NeurableUser.Instantiated)
            {
                return;
            }
            
            _channelCount = NeurableUser.Instance.User.GetDiagnosticChannelCount();
            var channelNames = NeurableUser.Instance.User.GetDiagnosticChannelNames();
            var channelColors = NeurableUser.Instance.User.GetDiagnosticChannelColors(periodicReset);
            for (var i = 0; i < _channelCount; ++i)
            {
                electrodeColors[channelNames[i]] = channelColors[i];
            }

            _countdown = (float) NeurableUser.Instance.User.GetDiagnosticCountdown();
        }
        
        public void UpdateColors()
        {
            foreach (var channel in electrodeColors.Keys)
            {
                SetColor(channel, electrodeColors[channel]);
            }
        }

        protected abstract void SetColor(string channelName, string channelColor);
        
        protected int ChannelColorToNumber(string s)
        {
            var upper = s.ToUpper();
            switch (upper)
            {
                case "GREEN":
                    return 0;
                case "YELLOW":
                    return 1;
                case "RED":
                    return 2;
                case "GRAY":
                    return 3;
                default:
                    return -1;
            }
        }
        
        protected void HandleCountdown()
        {
            if (_countdown > 0)
            {
                if (!_countingDown)
                {
                    StartCountdown();
                }

                CountdownTick();
            }
            else
            {
                if (_countingDown)
                {
                    EndCountdown();
                }
            }
        }

        private void StartCountdown()
        {
            if (resetButton)
            {
                resetButton.interactable = false;
            }
            if (disableDuringCountDown)
            {
                disableDuringCountDown.SetActive(false);
            }
            if (countDownText && refreshButtonString == "")
            {
				refreshButtonString = countDownText.text;
            }

            _countingDown = true;
        }

        private void CountdownTick()
        {
            var clock = Mathf.CeilToInt(_countdown);
            var newText = "Wait: " + clock;
            if (countDownText.text != newText)
            {
                countDownText.text = newText;
            }
        }

        private void EndCountdown()
        {
            if (_countingDown && countDownText)
            {
                countDownText.text = refreshButtonString;
            }
            if (resetButton)
            {
                resetButton.interactable = true;
            }
            if (disableDuringCountDown)
            {
                disableDuringCountDown.SetActive(true);
            }
            _countingDown = false;
        }
        
        private void OnDisable()
        {
            EndCountdown();
        }
    }
}