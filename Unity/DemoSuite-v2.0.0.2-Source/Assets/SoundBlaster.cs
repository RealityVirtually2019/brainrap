/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(LineRenderer))]
public class SoundBlaster : MonoBehaviour
{

    [Header("Passes Attention State Values into the Eye")]
    [SerializeField]
    Particle BoomBoom;
    Microphone BrainSound;
    public float sensitivity = 100;
    public float loudness = 0;

    public void Start()
    {
        var audio = GetComponent<AudioSource>();
        audio.clip = Microphone.Start("Built-in Microphone", true, 10, 44100);
        audio.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
        audio.Play();
    }
    
    public void Update()
    {
        loudness = GetAveragedVolume() * sensitivity;
        //BoomBoom.energy = audio.
    }

    float GetAveragedVolume()
    {
        float[] data = new float[256];
        float a = 0;
        //audio.GetOutputData(data, 0);
        foreach (float s in data)
        {
            a += Mathf.Abs(s);
        }
        return a / 256;
    }

    // NeurableAffectiveStateEngine returns a tuple of <time, value>.
    // This function takes the second value and passes it to the Slider
    public void UpdateBoomBoomWithAudio(float timestamp, float value)
    {
        BoomBoom.energy = value;
    }
}*/