using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    /*---------------------------------------*/
    #region SerializedParameter
    public int bufferSize;
    public int windowSize = 1024;
    public float ClipLoudness { get; set; }
    public float peak;
    [Range(0, 1)]
    public float gamma = 0.5f;
    #endregion SerializedParameter

    /*---------------------------------------*/
    #region UnityEvents
    public FloatSeriesEvent energiesEvent;
    public FloatSeriesEvent spectrumEvent;
    public FloatSeriesEvent logSpectrumEvent;
    public FloatSeriesEvent waveformEvent;

    public FloatValueEvent peakEvent;
    public FloatValueEvent beatEvent;

    [Serializable]
    public class FloatSeriesEvent : UnityEvent<int, IEnumerable<float>> { }
    [Serializable]
    public class FloatValueEvent : UnityEvent<float> { }
    #endregion UnityEvents

    /*---------------------------------------*/
    private const int spectrumDataLength = 8192;

    private AudioSource audioSource;

    private float threshold;


    private string device;
    private int logSpectrumLength;

    private float[] data;
    private float[] spectrum;
    private float[] logSpectrum;
    private float[] akku = new float[1024];


    /*---------------------------------------*/
    public float[] Data => data;
    public float[] LogSpectrum => logSpectrum;
    public float[] Spectrum => spectrum;
    public float[] Akku => akku;
    public AudioBuffer Buffer { get; private set; }

    /*---------------------------------------*/
    public void Awake()
    {
        Init();
    }

    public void OnDestroy()
    {
        //  AppDomain.CurrentDomain.AssemblyLoad -= load;
    }

    private void Start()
    {
        Init();
    }
    void Init(object sender, AssemblyLoadEventArgs args)
    {
        Init();
        Debug.Log("Assembly Loading");
    }
    private void Init()
    {
        Debug.Log("Mics Available : " + Microphone.devices.Length + " " + string.Join(",", Microphone.devices));
        if (Microphone.devices.Length > 0)
            device = Microphone.devices[0];
        audioSource = GetComponent<AudioSource>();
        this.logSpectrum = new float[logSpectrumLength];
        SetMicrophone(device);
        Play();
        CreateBuffer();
    }

    public void SetMicrophone(Dropdown dropdown)
    {
        Debug.Log("Mics : " + string.Join(",", Microphone.devices[dropdown.value]));
        SetMicrophone(Microphone.devices[dropdown.value]);
    }
    public void SetMicrophone(string device)
    {
        this.device = device;
        audioSource.clip = Microphone.Start(device, true, 100, SampleRate);
        if (audioSource.clip == null)
            Debug.Log($"Failed Loading Mic({device})");
        int min;
        int max;
        Microphone.GetDeviceCaps(device, out min, out max);
        Debug.Log("Start recording" + min + " " + max);
    }
    public void CreateBuffer()
    {
        Buffer = new AudioBuffer(bufferSize, SampleRate, data.Length);
    }
    public void Play()
    {
        audioSource.Play();
        audioSource.timeSamples = Microphone.GetPosition(device);//When set up here, it will be almost real-time synchronization.
        data = new float[windowSize];
    }
    // Update is called once per frame
    void Update()
    {
        if (spectrum == null || spectrum.Length != spectrumDataLength)
            spectrum = new float[spectrumDataLength];
        if (Buffer == null || Buffer.Length != bufferSize)
            CreateBuffer();
        if (audioSource != null)
        {

            Play();
            audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Hanning);
            // Get Wave
            int offsetSamples = (audioSource.timeSamples > data.Length) ? audioSource.timeSamples - data.Length : 0;
            audioSource.clip.GetData(data, offsetSamples);
            Buffer.Insert(data);
            AnalyzeLoudness();
            DetectBeats();

            logSpectrum = CalcultateLogSpectrum(spectrum, windowSize, ref akku, gamma);

            float[] energies = Buffer.Energies;
            energiesEvent.Invoke(energies.Length, energies);
            logSpectrumEvent.Invoke(logSpectrum.Length, logSpectrum);
            waveformEvent.Invoke(Buffer.DataLength, Buffer.Data());
            spectrumEvent.Invoke(spectrum.Length, spectrum);
        }
        else
        {
            Debug.LogWarning("No Audio Available!");
        }
    }

    private void DetectBeats()
    {
        var energies = Buffer.Energies;
        var current = Buffer.Energies.Last();
        int count = 0;
        for(int i=0;i<energies.Length-1;i++)
        {
            if(current > energies[i])
            {
                count++;
            }
        }
        if (count > 0.8 * energies.Length)
            beatEvent.Invoke(current);
    }

    private void AnalyzeLoudness()
    {
        ClipLoudness = 0f;
        for (int i = 0; i < spectrum.Length / 2; i++)
        {
            ClipLoudness += spectrum[i];
        }
        ClipLoudness /= windowSize;
        threshold *= 1 - Mathf.Pow(gamma, 2);
        if (ClipLoudness > threshold)
        {
            threshold = ClipLoudness;
            peak = ClipLoudness;
            peakEvent.Invoke(peak);
        }
    }

    public static float[] CalcultateLogSpectrum(float[] spectrum, int windowSize, ref float[] akku, float gamma)
    {

        var logSpectrumLength = Mathf.FloorToInt(Mathf.Sqrt(windowSize + 1) - .5f);
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
            akku[i] = Mathf.Lerp(akku[i], logSpectrum[i], Mathf.Pow(gamma, 2));
        }

        return logSpectrum;
    }

}
