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

    public VisualEffect equalizer;
    public AudioSource audioSource;
    public AudioLowPassFilter lowpass;


    //public AudioBuffer Buffer { get; private set; }
    public float[] Data => data;    

    public LineRenderer lineRenderer;

    public Text debug;
    public float clipLoudness;
    public float updateStep = 0.1f;
    public int sampleDataLength = 8192;

    private VisualEffect vfx;
    private int logSpectrumLength;
    private float currentUpdateTime = 0f;
    private float[] data;

    private float[] spectrum;
    private float[] logSpectrum;
    private float[] akku=new float[1024];

    public Texture2D tex;
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

    [Range(0, 1)]
    public float threashold = .5f;
    private string device;
    public float[] LogSpectrum => logSpectrum;
    public float[] Spectrum => spectrum;


    void Awake()
    {
        Init();
    }

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        Init();
    }


    // Update is called once per frame
    void Update()
    {
        spectrum = new float[sampleDataLength];
        currentUpdateTime += Time.deltaTime;
        if (audioSource != null && currentUpdateTime >= updateStep)
        {
            //Debug.Log(lineRenderer.positionCount + " " + audioSource.clip.length);

            Play();
            audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Hanning);
            // Get Wave
            int offsetSamples =  (audioSource.timeSamples > data.Length) ? audioSource.timeSamples - data.Length : 0;
            audioSource.clip.GetData(data, offsetSamples);
            //Buffer.Insert(data, audioSource.timeSamples);

            waveformEvent.Invoke(this);
            currentUpdateTime = 0f;

            clipLoudness = 0f;
            for (int i = 0; i < spectrum.Length / 2; i++)
            {
                clipLoudness += Mathf.Abs(spectrum[i]);
            }
            float low = 0;
            for (int i = 0; i < 5; i++)
            {
                low += Mathf.Abs(spectrum[i]);
            }
            low /= 5f;
            
            debug.text = audioSource.pitch + " - " + audioSource.time + " - " + clipLoudness;
            
            logSpectrum =  CalcultateLogSpectrum(spectrum, ref akku);
            tex.SetPixelData(logSpectrum, 0, 0);
            clipLoudness /= sampleDataLength;
            if (vfx != null)
            {
                vfx.SetFloat("Radius", .5f + clipLoudness * 200);
                vfx.SetFloat("Frequency", 1 + low * 2);
                if (low > 0.06)
                    vfx.SendEvent("Burst");
                equalizer.SetTexture("Spectrum", tex);
                equalizer.SendEvent("Emit");
            }
            for (int i = 0; i < logSpectrum.Length; i++)
            {
                float a = logSpectrum[i];
                if (a > threashold)
                    spectrumEvent.Invoke(i, a);
            }
            tex.Apply();

        }
    }
    public void SetMicrophone(Dropdown dropdown)
    {
        SetMicrophone(Microphone.devices[dropdown.value]);
    }
    public void SetMicrophone(string device)
    {
        this.device = device;
        audioSource.clip = Microphone.Start(device, true, 100, 44100);
        //Buffer = new AudioBuffer(1, 44100,data.Length);
        if (audioSource.clip == null)
            Debug.Log("Failed Loading Mic");
        Debug.Log("Start recording");
        audioSource.Play();
    }

    public void Play()
    {
        audioSource.Play();
        audioSource.timeSamples = Microphone.GetPosition(device);//When set up here, it will be almost real-time synchronization.
        data = new float[40240];
        int min;
        int max;
        Microphone.GetDeviceCaps(device, out min, out max);
        //aud.timeSamples = 0;
        //Debug.Log("Start playing" + min + " " + max);
    }

    public void Show(int i, float a)
    {
        Debug.Log($"{i}-{a}");
    }

    public float[] CalcultateLogSpectrum(float[] spectrum, ref float[] akku)
    {

        logSpectrumLength = Mathf.FloorToInt(Mathf.Sqrt(sampleDataLength+1)-.5f);
        var logSpectrum = new float[logSpectrumLength];
        if (akku.Length != logSpectrumLength)
            akku = new float[logSpectrumLength];

        int index = 0;
        for (int i = 0; i < logSpectrumLength; i++)
        {
            float sum = 0;
            int v = i+1;
            for (int j = 0; j < v&&spectrum.Length>index; j++) 
                sum += Mathf.Abs(spectrum[index++]);
            logSpectrum[i] = sum   ;
        }

        for (int i = 0; i < logSpectrum.Length; i++)
        {
            akku[i] = Mathf.Lerp(akku[i], logSpectrum[i], gamma);
        }
        spectrumEvent2.Invoke(this);

        return logSpectrum;
    }

    private void Init()
    {
        if (Microphone.devices.Length > 0)
            device = Microphone.devices[0];
        audioSource = GetComponent<AudioSource>();
        vfx = GetComponent<VisualEffect>();
        tex = new Texture2D(10, 1, TextureFormat.ARGB32, false);
        this.logSpectrum = new float[logSpectrumLength];
        SetMicrophone(device);
        Play();
    }

}
