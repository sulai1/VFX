using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Numerics;
using UnityEngine.Events;


namespace Assets.Scripts.Audio
{
    public class AudioBuffer
    {
        public readonly int length;
        public readonly int sampleRate;
        public readonly int windowSize;

        public class WindowData
        {
            public readonly float[] data;
            public readonly float[] spectrum;
            public readonly float sampleRate;
            public int Length => data.Length;
            public WindowData(float sampleRate, float[] data, float[] spectrum)
            {
                this.data = data;
                this.spectrum = spectrum;
                //Debug.Log("Energy " + energy + " Data : " + string.Join("'", data));
            }
        }

        private readonly Queue<WindowData> buffer = new Queue<WindowData>();
        //private FFT fft;

        public AudioBuffer(float seconds, int sampleRate, int windowSize) : this(Mathf.FloorToInt(seconds * sampleRate), sampleRate, windowSize) { }


        public float Duration => length / sampleRate;
        public int Length => length;
        public int DataLength => length * windowSize;
        public int WindowSize => windowSize;

        public WindowData Current => buffer.Last();
        public bool IsEmpty => buffer.Count <= 0;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="length">Number of windows to store</param>
        /// <param name="sampleRate"></param>
        /// <param name="windowSize"></param>
        public AudioBuffer(int length, int sampleRate, int windowSize)
        {
            this.length = length;
            this.sampleRate = sampleRate;
            this.windowSize = windowSize;
            buffer = new Queue<WindowData>(length);

        }


        public IEnumerable<float> Data()
        {
            foreach (var data in buffer)
                for (int j = 0; j < data.Length; j++)
                    yield return data.data[j];
        }

        public void Insert(float samplerate, float[] data, float[] spectrum)
        {
            if (buffer.Count >= length)
            {
                buffer.Dequeue();
            }

            WindowData newData = new WindowData(samplerate, data, spectrum);
            buffer.Enqueue(newData);

        }

    } 
}