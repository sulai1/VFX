using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Audio
{
    [RequireComponent(typeof(UnityEngine.AudioSource))]
    public class AudioPreprocessor : MonoBehaviour
    {

        /*---------------------------------------*/
        #region SerializedParameter
        public int bufferSize;
        public int windowSize = 1024;

        #endregion SerializedParameter

        /*---------------------------------------*/
        #region UnityEvents

        public UnityEvent<AudioBuffer> bufferEvent;
        public FloatSeriesEvent spectrumEvent;
        public FloatSeriesEvent waveformEvent;

        [Serializable]
        public class FloatSeriesEvent : UnityEvent<float[]> { }
        [Serializable]
        public class FloatValueEvent : UnityEvent<float> { }
        #endregion UnityEvents

        /*---------------------------------------*/
        [SerializeField]
        private int SampleRate = 44100;

        private UnityEngine.AudioSource audioSource;

        public float Peak { get; private set; }

        public float FPeak { get; private set; }


        private string device;

        private float[] data;
        private float[] spectrum;

        /*---------------------------------------*/
        public float[] Data => data;
        public float[] Spectrum => spectrum;
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
            audioSource = GetComponent<UnityEngine.AudioSource>();
            SetMicrophone(device);
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
            audioSource.loop = true;
            audioSource.clip = Microphone.Start(device, audioSource.loop, 1, SampleRate);
            if (audioSource.clip == null)
                Debug.Log($"Failed Loading Mic({device})");
            int min;
            int max;
            Microphone.GetDeviceCaps(device, out min, out max);
            Debug.Log("Start recording" + min + " " + max);
            while (!(Microphone.GetPosition(null) > 0))
                audioSource.Play();
        }
        public void CreateBuffer()
        {
            data = new float[windowSize];
            Buffer = new AudioBuffer(bufferSize, SampleRate, data.Length);
        }

        // Update is called once per frame
        void Update()
        {
            if (spectrum == null || data.Length != windowSize)
            {
                data = new float[windowSize];
                spectrum = new float[windowSize];
            }
            if (Buffer == null || Buffer.Length != bufferSize)
                CreateBuffer();
            if (audioSource != null)
            {
                //audioSource.GetOutputData(data, 0);
                //// Get Wave
                int offsetSamples = (audioSource.timeSamples > data.Length) ? audioSource.timeSamples - data.Length : 0;
                audioSource.clip.GetData(data, offsetSamples);

                //float[] real = new float[windowSize * 2];
                //float[] imaginary = new float[windowSize * 2];
                //for (int i = 0; i < windowSize; i++)
                //    real[i] = data[i];
                //for (int i = windowSize; i < windowSize * 2; i++)
                //    if (!Buffer.IsEmpty)
                //        real[i] = data[i-windowSize];
                //    else
                //        real[i] = 0;
                //for (int i = 0; i < real.Length; i++)
                //    real[i] *= 0.5f * (1f - Mathf.Cos(i / (float)windowSize / 2f));
                //MathNet.Numerics.IntegralTransforms.Fourier.Forward(real,imaginary);
                //for (int i = 0; i < windowSize * .012f; i++)
                //{
                //    spectrum[i] = (float)real[i];
                //}
                audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

                Peak = Mathf.Max(Peak, data.Max(x => Mathf.Abs(x)));
                FPeak = Mathf.Max(FPeak, spectrum.Max());
                Buffer.Insert(SampleRate, Normalize(data, Peak), Normalize(spectrum, FPeak));

                bufferEvent.Invoke(Buffer);
                waveformEvent.Invoke(Buffer.Data().ToArray());
                spectrumEvent.Invoke(Buffer.Current.spectrum);

            }
            else
            {
                Debug.LogWarning("No Audio Available!");
            }
        }


        private float[] Normalize(float[] data, float max)
        {
            // Normalize
            float[] normalized = new float[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                normalized[i] = data[i] / max;
            }
            return normalized;
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
}