using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VFX;
using UnityEngineInternal;

[RequireComponent(typeof(AudioSource))]
public class AudioProcessor : MonoBehaviour
{
    private const int SampleRate = 44100;
    public int Length = 43;
    public AudioSource audioSource;


    public AudioBuffer Buffer { get; private set; }
    public float[] Data => data;

    public LineRenderer lineRenderer;

    public float clipLoudness;

    const int sampleDataLength = 1024;
    const int spectrumDataLength = 8192;

    private string device;
    private int logSpectrumLength;

    private float[] data;
    private float[] spectrum;
    private float[] logSpectrum;
    private float[] akku = new float[1024];

    [Range(0, 1)]
    public float gamma = 0.5f;

    public SpectrumEvent spectrumEvent;
    public SpectrumEvent2 spectrumEvent2;
    public WaveformEvent waveformEvent;

    [Serializable]
    public class SpectrumEvent : UnityEvent<int, float> { }

    [Serializable]
    public class SpectrumEvent2 : UnityEvent<AudioProcessor> { }

    [Serializable]
    public class WaveformEvent : UnityEvent<AudioProcessor> { }

    public float[] LogSpectrum => logSpectrum;
    public float[] Spectrum => spectrum;
    AssemblyLoadEventHandler load;

    public void Awake()
    {
        load = new AssemblyLoadEventHandler(Init);
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.AssemblyLoad += load;
        Init();
    }

    public void OnDestroy()
    {
        AppDomain.CurrentDomain.AssemblyLoad -= load;
    }

    private void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (spectrum == null || spectrum.Length != spectrumDataLength)
            spectrum = new float[spectrumDataLength];
        if (Buffer == null)
            Buffer = new AudioBuffer(Length, SampleRate, data.Length);
        if (audioSource != null)
        {
            //Debug.Log(lineRenderer.positionCount + " " + audioSource.clip.length);

            Play();
            audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Hanning);
            // Get Wave
            int offsetSamples = (audioSource.timeSamples > data.Length) ? audioSource.timeSamples - data.Length : 0;
            audioSource.clip.GetData(data, offsetSamples);
            //audioSource.Play();
            //audioSource.GetOutputData(left, 0);
            Buffer.Insert(data);

            waveformEvent.Invoke(this);

            clipLoudness = 0f;
            for (int i = 0; i < spectrum.Length / 2; i++)
            {
                clipLoudness += spectrum[i];
            }


            logSpectrum = CalcultateLogSpectrum(spectrum, ref akku);
            clipLoudness /= sampleDataLength;


        }
        else
        {
            Debug.LogWarning("No Audio Available!");
        }
    }
    public void SetMicrophone(Dropdown dropdown)
    {
        SetMicrophone(Microphone.devices[dropdown.value]);
    }
    public void SetMicrophone(string device)
    {
        this.device = device;
        audioSource.clip = Microphone.Start(device, true, 100, SampleRate);
        if (audioSource.clip == null)
            Debug.Log("Failed Loading Mic");
        Debug.Log("Start recording");
        int min;
        int max;
        Microphone.GetDeviceCaps(device, out min, out max);
        audioSource.Play();
    }

    public void Play()
    {
        audioSource.Play();
        audioSource.timeSamples = Microphone.GetPosition(device);//When set up here, it will be almost real-time synchronization.
        data = new float[sampleDataLength];
        //aud.timeSamples = 0;
        //Debug.Log("Start playing" + min + " " + max);
    }

    public float[] CalcultateLogSpectrum(float[] spectrum, ref float[] akku)
    {

        logSpectrumLength = Mathf.FloorToInt(Mathf.Sqrt(sampleDataLength + 1) - .5f);
        var logSpectrum = new float[logSpectrumLength];
        if (akku.Length != logSpectrumLength)
            akku = new float[logSpectrumLength];

        int index = 0;
        for (int i = 0; i < logSpectrumLength; i++)
        {
            float sum = 0;
            int v = i + 1;
            for (int j = 0; j < v && spectrum.Length > index; j++)
                sum += spectrum[index++];
            logSpectrum[i] = -1 / Mathf.Log(sum);
        }

        for (int i = 0; i < logSpectrum.Length; i++)
        {
            akku[i] = Mathf.Lerp(akku[i], logSpectrum[i], gamma);
        }
        spectrumEvent2.Invoke(this);

        return logSpectrum;
    }
    void Init(object sender, AssemblyLoadEventArgs args)
    {
        Init();
        Debug.Log("Assembly Loading");
    }
    private void Init()
    {
        if (Microphone.devices.Length > 0)
            device = Microphone.devices[0];
        audioSource = GetComponent<AudioSource>();
        this.logSpectrum = new float[logSpectrumLength];
        SetMicrophone(device);
        Play();
        Buffer = new AudioBuffer(Length, SampleRate, data.Length);
    }

}
