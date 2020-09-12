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
    public float clipLoudness;
    public float updateStep = 0.1f;
    public int sampleDataLength = 1024;

    private VisualEffect vfx;
    private int logSpectrumLength = 10;
    private float currentUpdateTime = 0f;
    private float[] spectrum;
    private float[] logSpectrum;

    public Texture2D tex;
    [Range(0, 1)]
    public float gamma = 0.5f;

    public SpectrumEvent spectrumEvent;

    [Serializable]
    public class SpectrumEvent : UnityEvent<int, float> { }

    [Range(0, 1)]
    public float threashold = .5f;
    private string device;

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
        Debug.Log(" " + audioSource.clip );
        if (audioSource != null && currentUpdateTime >= updateStep)
        {
            Play();
            audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Hanning);
            currentUpdateTime = 0f;

            tex.Apply();
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
            Debug.Log(clipLoudness*100000);
            LogSpectrum(spectrum, logSpectrum);
            tex.SetPixelData(logSpectrum, 0, 0);
            clipLoudness /= sampleDataLength;
            //if (vfx!=null)
            //{
            //    vfx.SetFloat("Radius", .5f + clipLoudness * 200);
            //    vfx.SetFloat("Frequency", 1 + low * 2);
            //    if (low > 0.06)
            //        vfx.SendEvent("Burst");
            //    equalizer.SetTexture("Spectrum", tex);
            //    equalizer.SendEvent("Emit"); 
            //}
            for (int i = 0; i < logSpectrum.Length; i++)
            {
                float a = logSpectrum[i];
                if (a > threashold)
                    spectrumEvent.Invoke(i, a);
            }

        }
    }
    
    public void SetMicrophone(Dropdown dropdown)
    {
        SetMicrophone(Microphone.devices[dropdown.value]);
    }
    public void SetMicrophone(string device)
    {
        this.device = device;
        audioSource.clip = Microphone.Start(device, true, 10, 44100);
        if (audioSource.clip == null)
            Debug.Log("Failed Loading Mic");
        Debug.Log("Start recording");
        audioSource.Play();
    }

    public void Play()
    {
        audioSource.Play();
        audioSource.timeSamples = Microphone.GetPosition(device);//When set up here, it will be almost real-time synchronization.

        int min;
        int max;
        Microphone.GetDeviceCaps(device, out min, out max);
        //aud.timeSamples = 0;
        Debug.Log("Start playing" + min + " " + max);
    }

    public void Show(int i, float a)
    {
        Debug.Log($"{i}-{a}");
    }

    public float[] LogSpectrum(float[] spectrum, float[] akku)
    {
        float[] logSpectrum = new float[logSpectrumLength];

        int index = 0;
        for (int i = 0; i < logSpectrumLength; i++)
        {
            float sum = 0;
            int v = 1 << i;
            for (int j = 0; j < v; j++)
                sum += Mathf.Abs(spectrum[index++]);
            sum /= v;
            logSpectrum[i] = sum * (i + 1) * (i + 1);
        }

        for (int i = 0; i < logSpectrum.Length; i++)
        {
            akku[i] = Mathf.Lerp(this.logSpectrum[i], logSpectrum[i], gamma);
        }

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
