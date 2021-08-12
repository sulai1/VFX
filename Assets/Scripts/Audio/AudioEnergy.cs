using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Audio
{
    public class AudioEnergy : AudioProcessor<float[]>
    {
        public float peak = 0;
        [Range(1, 1024)]
        public int binSize = 10;
        protected override float[] Process1(AudioBuffer input)
        {
            var data = input.Current.data.ToArray();
            int length = Mathf.FloorToInt(data.Length / (float)binSize);
            float[] energy = new float[length];
            for (int j = 0; j < length; j++)
            {
                for (int i = j * binSize; i < data.Length && i < (j + 1) * binSize; i++)
                {
                    energy[j] += data[i] * data[i];
                }
                energy[j] /= (float)binSize;
                peak = Mathf.Max(peak, energy[j]);
            }
            for (int j = 0; j < length; j++)
            {
                energy[j] /= peak;
            }
                return energy;
        }
    }
}