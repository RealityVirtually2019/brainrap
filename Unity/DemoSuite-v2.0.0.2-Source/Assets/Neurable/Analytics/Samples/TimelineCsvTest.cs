using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace Neurable.Analytics.Samples
{
    public class TimelineCsvTest : MonoBehaviour
    {
        private NeurableMentalStateEngine stateEngine;
        private FixationEngine fixationEngine;
		public float subTimelineLength = 10f;

		public KeyCode keyToStartRecording = KeyCode.C;
		public KeyCode keyToStopRecording = KeyCode.V;
		public KeyCode keyToPrintFixationCsv = KeyCode.Space;

		public string dataFolder = "AffectiveStateData";
		public string reportCardFile = "ReportCard.csv";

		private AffectiveStateTimeline _masterTimeline;
        private float _startTime;
        private float _endTime;

        private void Start()
        {
			stateEngine = NeurableMentalStateEngine.instance;
			fixationEngine = FixationEngine.instance;
			_masterTimeline = stateEngine.stateTimeline;
        }

        private void Update()
        {
            if (Input.GetKeyDown(keyToPrintFixationCsv))
			{
				PrintFixationCsv();
			}
			if (Input.GetKeyDown(keyToStartRecording))
			{
				StartRecording();
			}
			if (Input.GetKeyDown(keyToStopRecording))
			{
				EndRecordingAndExport();
			}
        }

		private void StartRecording()
        {
            _startTime = _masterTimeline.GetLatestTimestamp();
        }

        private void EndRecordingAndExport()
        {
            _endTime = _masterTimeline.GetLatestTimestamp();
            
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            var mainWindow = _masterTimeline.GetSubTimeline(_startTime, _endTime);
            
            mainWindow.PrintDataPointsToCsv(dataFolder + "/MainDataPoints.csv");
            mainWindow.PrintOverviewCsv(dataFolder + "/MainOverview.csv");

            var allTimelineOverview = AffectiveStateTimeline.GetMultitimelineOverviewCsvHeader() + "\r\n";
            
            var i = 0;
            for (var t = _startTime; t < _endTime; t += subTimelineLength)
            {
                var timeline = mainWindow.GetSubTimeline(t, subTimelineLength);
                timeline.PrintDataPointsToCsv(dataFolder + "/SubtimelineDataPoints" + i + ".csv");
                timeline.PrintOverviewCsv(dataFolder + "/SubtimelineOverview" + i + ".csv");
                allTimelineOverview += timeline.GetMultitimelineOverviewCsvData() + "\r\n";
                i++;
            }
            
            File.WriteAllText(dataFolder + "/AllTimelineOverview.csv", allTimelineOverview);
		}

		#region Fixation CSV Output

		public string GetFixationCsvHeader()
		{
			List<string> line = new List<string>();
			line.Add("Item Name");
			line.Add("Total Gaze Time");
			line.Add("Longest Fixation");
			line.Add("First Fixation");
			line.Add("Number of Fixations");
			line.Add("Average Fixation Time");

			line.Add("Initial Stress");
			line.Add("Final Stress");
			line.Add("Delta Stress");
			line.Add("Min Stress Timestamp");
			line.Add("Min Stress");
			line.Add("Max Stress Timestamp");
			line.Add("Max Stress");

			line.Add("Initial Attention");
			line.Add("Final Attention");
			line.Add("Delta Attention");
			line.Add("Min Attention Timestamp");
			line.Add("Min Attention");
			line.Add("Max Attention Timestamp");
			line.Add("Max Attention");

			line.Add("Initial Calm");
			line.Add("Final Calm");
			line.Add("Delta Calm");
			line.Add("Min Calm Timestamp");
			line.Add("Min Calm");
			line.Add("Max Calm Timestamp");
			line.Add("Max Calm");

			line.Add("Initial Fatigue");
			line.Add("Final Fatigue");
			line.Add("Delta Fatigue");
			line.Add("Min Fatigue Timestamp");
			line.Add("Min Fatigue");
			line.Add("Max Fatigue Timestamp");
			line.Add("Max Fatigue");

			line.Add("Initial GrandMean");
			line.Add("Final GrandMean");
			line.Add("Delta GrandMean");
			line.Add("Min GrandMean Timestamp");
			line.Add("Min GrandMean");
			line.Add("Max GrandMean Timestamp");
			line.Add("Max GrandMean");
			return string.Join(",", line.ToArray());
		}

		public void PrintFixationCsv()
		{
			if (fixationEngine == null) return;
			if (!Directory.Exists("AffectiveStateData"))
			{
				Directory.CreateDirectory("AffectiveStateData");
			}

			List<string> lines = new List<string>();
			lines.Add(GetFixationCsvHeader());

			var points = fixationEngine.GetPointsOfInterest_IDs();
			foreach(var point in points)
			{
				var line = StringifyFixationPoint(point);
				if (line.Length > 0)
					lines.Add(line);
			}
			File.WriteAllText(dataFolder + "/" + reportCardFile, string.Join("\r\n", lines.ToArray()));
		}

		// Collate all data for a single fixation point into a csv line as follows
		// Item, Total Gaze Time, Longest Fixation, First Fixation, Number of Fixations, Average Time, Initial Stress, Final Stress*, Delta Stress*, Min Stress* Time, Min Stress*, Max Stress* Time, Max Stress*, ...
		public string StringifyFixationPoint(FixationID point)
		{
			FixationData data;
			if (!fixationEngine.GetDataForPoint(point, out data)) return "";

			List<string> line = new List<string>();
			line.Add(point.Name);
			float firstFixation = data.GetFirstFixationTimeStamp();
			float lastFixation = data.GetLastFixationTimeStamp();

			var timelines = fixationEngine.GetAllTimelinesForFixation(data);
			var firstState = stateEngine.stateTimeline.GetValueAtTime(firstFixation);
			var lastState = stateEngine.stateTimeline.GetValueAtTime(lastFixation);

			line.Add(data.GetTotalFixationDuration().ToString());
			line.Add(data.GetLongestFixationLength().ToString());
			line.Add(firstFixation.ToString());
			line.Add(data.GetNumberFixations().ToString());
			line.Add(data.GetAverageGazeTime().ToString());

			line.Add(StringifyStateHelper(AffectiveStateType.Stress, timelines, firstState, lastState));
			line.Add(StringifyStateHelper(AffectiveStateType.Attention, timelines, firstState, lastState));
			line.Add(StringifyStateHelper(AffectiveStateType.Calm, timelines, firstState, lastState));
			line.Add(StringifyStateHelper(AffectiveStateType.Fatigue, timelines, firstState, lastState));
			line.Add(StringifyStateHelper(AffectiveStateType.GrandMean, timelines, firstState, lastState));

			return string.Join(",", line.ToArray());
		}

		// Collate Individual Affective State data from pre-initialized collections
		private string StringifyStateHelper(AffectiveStateType type, List<AffectiveStateTimeline> timelines, AffectiveStatesCollection firstState, AffectiveStatesCollection lastState)
		{
			List<string> line = new List<string>();
			float firstValue, lastValue, deltaValue;
			TimestampedValue min, max;
			min = fixationEngine.GetMinimumStateOverEvents(type, timelines);
			max = fixationEngine.GetMaximumStateOverEvents(type, timelines);

			firstState.GetValue(type, AveragingType.EMA, out firstValue);
			lastState.GetValue(type, AveragingType.EMA, out lastValue);
			deltaValue = lastValue - firstValue;
			min = fixationEngine.GetMinimumStateOverEvents(type, timelines);
			max = fixationEngine.GetMaximumStateOverEvents(type, timelines);
			line.Add(firstValue.ToString());
			line.Add(lastValue.ToString());
			line.Add(deltaValue.ToString());
			line.Add(min.timestamp.ToString());
			line.Add(min.value.ToString());
			line.Add(max.timestamp.ToString());
			line.Add(max.value.ToString());

			return string.Join(",", line.ToArray());
		}
		#endregion
	}
}
