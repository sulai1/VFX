using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioBuffer
{
    private readonly int length;
    private readonly int sampleRate;
    private readonly int windowSize;
    private readonly int capacity;

    private float bpm;
    private int start;
    private int offset;
    private float[][] buffer;


    public AudioBuffer(float seconds, int sampleRate, int windowSize) : this(Mathf.FloorToInt(seconds * sampleRate), sampleRate, windowSize) { }
    public AudioBuffer(int length, int sampleRate, int windowSize)
    {
        start = 0;
        this.length = length;
        this.sampleRate = sampleRate;
        this.capacity = 2 * length;
        buffer = new float[capacity][];
    }

    public float Duration => length / sampleRate;
    public int Length => length;
    public float BPM => bpm;

    public IEnumerable<float> Data()
    {
        for (int i=start;i<buffer.Length;i++)
            for(int j=offset;j<buffer[i].Length;i++)
                yield return buffer[i][j];
    }


    public void Insert(float[] data, int offset)
    {
        this.offset = offset;
        int samples = Mathf.Min(data.Length, Mathf.FloorToInt(Time.deltaTime * sampleRate));
        Debug.Log("Buffer Insert");

        try
        {
            if (start >= capacity)
            {
                start = 0;
                Update();
            }
            buffer[start] = data;
            start++;
        }
        catch
        {
            Debug.Log(start + " " + buffer.Length + ", " + data.Length );
        }
    }

    private void Update()
    {
        //bpm = CalculateBPM(Data().ToArray(), sampleRate);
        Debug.Log("Buffer Update (BPM:" + bpm + ")");
    }

    private static float rangeQuadSum(float[] samples, int start, int stop)
    {
        float tmp = 0;
        for (int i = start; i <= stop; i++)
        {
            tmp += Mathf.Pow(samples[i], 2);
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

    public static float CalculateBPM(float[] data, int sampleRate)
    {
        var trackLength = (float)data.Length / sampleRate;

        // 0.1s window ... 0.1*44100 = 4410 samples, lets adjust this to 3600 
        int sampleStep = 3600;

        // calculate energy over windows of size sampleSetep
        List<float> energies = new List<float>();
        for (int i = 0; i < data.Length - sampleStep - 1; i += sampleStep)
        {
            energies.Add(rangeQuadSum(data, i, i + sampleStep));
        }

        int beats = 0;
        float average = 0;
        float sumOfSquaresOfDifferences = 0;
        float variance = 0;
        float newC = 0;
        List<float> variances = new List<float>();

        // how many energies before and after index for local energy average
        int offset = 10;

        for (int i = offset; i <= energies.Count - offset - 1; i++)
        {
            // calculate local energy average
            float currentEnergy = energies[i];
            float qwe = rangeSum(energies.ToArray(), i - offset, i - 1) + currentEnergy + rangeSum(energies.ToArray(), i + 1, i + offset);
            qwe /= offset * 2 + 1;

            // calculate energy variance of nearby energies
            List<float> nearbyEnergies = energies.Skip(i - 5).Take(5).Concat(energies.Skip(i + 1).Take(5)).ToList<float>();
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