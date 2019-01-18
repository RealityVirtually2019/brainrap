using System.Collections.Generic;
using UnityEngine;

namespace Neurable.Diagnostics
{
	public class HeadsetDiagnosticReporter : HeadsetDiagnosticBase
	{
#if CVR_NEURABLE
		private readonly Dictionary<string, float> _colorConverter= new Dictionary<string, float> {{"RED", 0f}, {"YELLOW", 0.5f}, {"GREEN", 1f}};
#endif

		protected override void Awake()
		{
			base.Awake();
			periodicReset = false;
			triggerButton = KeyCode.None;
			if (sampleRateHz > 9f) sampleRateHz = 5f;
		}
		protected override void PopulateChannels() { }
		protected override void SetColor(string channelName, string channelColor)
		{
#if CVR_NEURABLE
			if (_colorConverter.ContainsKey(channelColor))
			{
				CognitiveVR.SensorRecorder.RecordDataPoint("Neurable.EEG." + channelName.ToUpper() + ".Quality", _colorConverter[channelColor]);
			}
#endif
		}
	
	}
}
