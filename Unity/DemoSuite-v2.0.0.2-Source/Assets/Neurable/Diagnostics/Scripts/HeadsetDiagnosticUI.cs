using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Neurable.Diagnostics
{
    public class HeadsetDiagnosticUI : HeadsetDiagnosticBase
    {
        private Dictionary<string, Image> _electrodes;

        protected new void Awake()
        {
            base.Awake();
            _electrodes = new Dictionary<string, Image>();
        }

        protected override void PopulateChannels()
        {
            if (_electrodes.Count != electrodeColors.Count)
            {
                Image[] children = gameObject.GetComponentsInChildren<Image>();
                foreach (string channel in electrodeColors.Keys)
                {
                    foreach (Image child in children)
                    {
                        if (child.name.ToUpper() == channel.ToUpper())
                        {
                            _electrodes[channel] = child;
                            break;
                        }
                    }
                }

                if (_electrodes.Count != electrodeColors.Count)
                {
                    Debug.LogError("Electrodes not Populated. Make sure there is a MeshRenderer attached to each properly named Electrode.");
                }
            }
        }

        protected override void SetColor(string channelName, string channelColor)
        {
            Image target;
            if (_electrodes.TryGetValue(channelName, out target))
            {
                target.color = nodeColors[ChannelColorToNumber(channelColor)];
            }
            else
            {
                Debug.Log("Channel Name is invalid or Electrodes not populated");
            }
        }
    }
}
