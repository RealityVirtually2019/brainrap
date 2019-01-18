/*
* Copyright 2017 Neurable Inc.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Neurable.Core;

namespace Neurable.Analytics
{
    [Serializable]
    public class EegDataEvent : UnityEvent<float, string[], float[]>
    {
    } // Arguments are Time, titles, values

    public class NeurableEegDataEngine : MonoBehaviour
    {
        [SerializeField, Tooltip("Number of times to submit EEG data a second. Sample Rate is limited to frequency of Unity Update(). If 0, will default to every Unity Frame.")]
        public float sampleRateHz = 10f;
        private float _timeOfLastSubmission;

        [Header("Parameters are (float)Timestamp, (string[])Titles, (float[])Values")] [SerializeField]
        public EegDataEvent onEegDataChange;

        private readonly List<string> _blacklist = new List<string> { "Time", "Eye", "TRG" };
        private Neurable.API.Types.DataCallback _onData;

        private readonly List<DataPacket> _newDataPackets = new List<DataPacket>();

        private class DataPacket
        {
            public readonly float time;
            public readonly string[] titles;
            public readonly float[] values;

            public DataPacket(double time, string[] titles, double[] values)
            {
                this.time = (float) time;
                this.titles = titles;
                this.values = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    this.values[i] = (float) values[i];
                }
            }
            public DataPacket(float time, string[] titles, float[] values)
            {
                this.time = time;
                this.titles = titles;
                this.values = values;
            }
        }

        public void DataCallback(double timestamp, String[] titles, double[] values)
        {
            _newDataPackets.Add(new DataPacket(timestamp, titles, values));
        }

        // Use this for initialization
        private void OnEnable()
        {
            _onData = DataCallback;
            if (NeurableUser.Instantiated)
            {
                NeurableUser.Instance.User.SetDataCallback(_onData, Neurable.API.Types.CallbackType.RAW_DATA);
            }
        }

        private void OnDisable()
        {
            if (NeurableUser.Instantiated)
            {
                NeurableUser.Instance.User.SetDataCallback(null, Neurable.API.Types.CallbackType.RAW_DATA);
            }
        }

        private void Start()
        {
            _timeOfLastSubmission = Time.time;
            _onData = DataCallback;
            if (NeurableUser.Instantiated)
            {
                NeurableUser.Instance.User.SetDataCallback(_onData, Neurable.API.Types.CallbackType.RAW_DATA);
            }
        }

        private bool TimeToUpdate {
            get
            {
                if (_newDataPackets.Count == 0) return false;
                if (sampleRateHz == 0f) return true;
                return (Time.time - _timeOfLastSubmission >= 1 / sampleRateHz);
            }
        }

        private void Update()
        {
            if (!TimeToUpdate) return;

            DataPacket data;
            if (AverageBufferedData(out data))
                SubmitSensorData(data);
        }

        private bool AverageBufferedData(out DataPacket packet)
        {
            if (_newDataPackets == null || _newDataPackets.Count == 0)
            {
                packet = new DataPacket(0f, null, null);
                return false;
            }
            var sums = new Dictionary<string, float>();
            var counts = new Dictionary<string, int>();
            var timeStamp = 0f;
            for (var i = 0; i < _newDataPackets.Count; i++)
            {
                var dataPack = _newDataPackets[i];
                if (dataPack.titles == null) continue;
                for (var j = 0; j < dataPack.titles.Length; j++)
                {
                    var title = dataPack.titles[j];
                    var value = dataPack.values[j];
                    if (IsBlacklisted(title)) continue;
                    if (!sums.ContainsKey(title))
                    {
                        sums.Add(title, (float)value);
                        counts.Add(title, 1);
                    }
                    else
                    {
                        sums[title] += (float) value;
                        counts[title]++;
                    }
                }

                timeStamp = dataPack.time;
            }
            _newDataPackets.Clear();

            if (sums.Count <= 0 || sums.Count != counts.Count)
            {
                packet = new DataPacket(0f, null, null);
                return false;
            }

            var titles = new string[sums.Count];
            var values = new float[sums.Count];
            var index = 0;

            foreach (var kv in sums)
            {
                titles[index] = kv.Key;
                
                if (counts[kv.Key] == 0)
                {
                    values[index] = 0;
                }
                else
                {
                    values[index] = kv.Value / counts[kv.Key];
                }
                index++;
            }
            packet = new DataPacket(timeStamp, titles, values);
            return true;
        }

        private void SubmitSensorData(DataPacket packet)
        {
            onEegDataChange.Invoke(packet.time, packet.titles, packet.values);
#if CVR_NEURABLE
            for (var i = 0; i < packet.titles.Length; i++)
            {
                var title = packet.titles[i].ToUpper();
                CognitiveVR.SensorRecorder.RecordDataPoint("Neurable.EEG." + title, packet.values[i]);
            }
#endif
            _timeOfLastSubmission = Time.time;
        }

        private bool IsBlacklisted(string title)
        {
            var titleUpper = title.ToUpper();
            for (var i = 0; i < _blacklist.Count; i++)
            {
                if (titleUpper.Contains(_blacklist[i].ToUpper()))
                    return true;
            }
            return false;
        }
    }
}