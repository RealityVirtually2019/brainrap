using System.Collections.Generic;
using UnityEngine;

namespace Neurable.Diagnostics
{
    public class HeadsetDiagnosticMesh : HeadsetDiagnosticBase
    {
        [Header("Material for electrodes")]
        public Material nodeMaterial;
        private Dictionary<string, MeshRenderer> _electrodes;

        protected new void Awake()
        {
            base.Awake();
            _electrodes = new Dictionary<string, MeshRenderer>();
        }

        protected override void PopulateChannels()
        {
            if (_electrodes.Count != electrodeColors.Count)
            {
                MeshRenderer[] children = gameObject.GetComponentsInChildren<MeshRenderer>();
                foreach (string channel in electrodeColors.Keys)
                {
                    foreach (MeshRenderer child in children)
                    {
                        if (child.name.ToUpper() == channel.ToUpper())
                        {
                            _electrodes[channel] = child;
                            child.material = Instantiate(nodeMaterial);
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
            MeshRenderer target;
            if (_electrodes.TryGetValue(channelName, out target))
            {
                target.material.SetColor("_EmissionColor", nodeColors[ChannelColorToNumber(channelColor)]);
            }
            else
            {
                Debug.Log("Channel Name is invalid or Electrodes not populated");
            }
        }
    }
}
