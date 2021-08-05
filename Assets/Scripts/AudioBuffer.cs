using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using mdsplib.FT;
using System.Numerics;

public class AudioBuffer
{
    private readonly int length;
    private readonly int sampleRate;
    private readonly int windowSize;

    float energy = 0;
    float bpm = 0;
    int counter = 0;

    struct WindowData
    {
        public float[] data;
        public Complex[] fft;
        public float energy;
        public int Length => data.Length;
    }

    private Queue<WindowData> buffer;
    //private FFT fft;

    public AudioBuffer(float seconds, int sampleRate, int windowSize) : this(Mathf.FloorToInt(seconds * sampleRate), sampleRate, windowSize) { }

    public Complex[] spectrum;

    public float Duration => length / sampleRate;
    public int Length => length;
    public int DataLength => length * windowSize;
    public int WindowSize => windowSize;
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

        //fft = new FFT();
        //fft.Initialize((uint)windowSize);
    }


    public IEnumerable<float> Data()
    {
        foreach (var data in buffer)
            for (int j = 0; j < data.Length; j++)
                yield return data.data[j];
    }

    public float[] Energies
    {
        get
        {
            float[] energies = new float[Length];
            int i = 0;
            foreach (var data in buffer)
                energies[i++] = data.energy;
            return energies;
        }
    }

    public void Insert(float[] data)
    {
        counter++;
        if (buffer.Count >= length)
        {
            buffer.Dequeue();
        }
        //spectrum = fft.Direct(data);
        energy = rangeQuadSum(data);
        buffer.Enqueue(new WindowData { data = data, energy = energy, fft = spectrum });

        //compute average energy
        float e = 0f;
        foreach (var d in buffer)
        {
            e += d.energy;
        }
        e /= (float)Length;

        //compute variance
        float v = 0f;
        foreach (var d in buffer)
        {
            v += (d.energy - e) * (d.energy - e);
        }
        v /= (float)Length;

        // compute threashold
        float c = (-.0025714f * v) + 1.5142857f;
        if (energy >= c * e)
        {
            Update();
        }
        //Debug.Log("energy " + energy + " thresh " + c * e + "avg " + e + " var " + v);
    }

    float last = 0;
    private void Update()
    {
        float d = (Time.time - last);
        bpm = 60f / d;
        //Debug.Log("d " + d + " bpm " + bpm + " len " + buffer.Count);
        last = Time.time;
    }

    private static float rangeQuadSum(float[] samples)
    {
        float tmp = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            tmp += samples[i] * samples[i];
        }

        return tmp;
    }

    private static float rangeSum(float[] data, int start, int stop)
    {
        float tmp = 0;
        for (int i = start; i <= stop; i++)
        {
            tmp += data[i];
        }

        return tmp;
    }

    public float CalculateBPM()
    {
        var trackLength = (float)buffer.Count * windowSize / sampleRate;

        // 0.1s window ... 0.1*44100 = 4410 samples, lets adjust this to 3600 

        // calculate energy over windows of size sampleSetep

        int beats = 0;
        float average = 0;
        float sumOfSquaresOfDifferences = 0;
        float variance = 0;
        float newC = 0;
        List<float> variances = new List<float>();

        // how many energies before and after index for local energy average
        int offset = 10;

        for (int i = offset; i <= Energies.Length - offset - 1; i++)
        {
            // calculate local energy average
            float currentEnergy = Energies[i];
            float qwe = rangeSum(Energies.ToArray(), i - offset, i - 1) + currentEnergy + rangeSum(Energies.ToArray(), i + 1, i + offset);
            qwe /= offset * 2 + 1;

            // calculate energy variance of nearby energies
            List<float> nearbyEnergies = Energies.Skip(i - 5).Take(5).Concat(Energies.Skip(i + 1).Take(5)).ToList<float>();
            average = nearbyEnergies.Average();
            sumOfSquaresOfDifferences = nearbyEnergies.Select(val => (val - average) * (val - average)).Sum();
            variance = (sumOfSquaresOfDifferences / nearbyEnergies.Count) / Mathf.Pow(10, 22);

            // experimental linear regression - constant calculated according to local energy variance
            newC = variance * 0.009f + 1.385f;
            if (currentEnergy > newC * qwe)
                beats++;
        }

        return beats / (trackLength / 60f);
    }
}