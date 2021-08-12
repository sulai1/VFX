using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Audio
{
    public class AudioSpectrumEnergy : AudioProcessor<float>
    {

        public float Peak { get; protected set; } = 0;
        public float BpM { get; protected set; } = 0;
        public float MeadianBPM { get; protected set; } = 0;

        [Range(64, 256)]
        public float BPMMin = 120, BPMMax = 180;

        [Range(0, 1)]
        public float threshold = 0.5f;
        [Range(0, 1)]
        public float release = .5f;
        [Range(0, 1)]
        public float attack = .5f;
        [Range(0, 1)]
        public float bpmKeep = .5f;
        [Range(0, 1)]
        public float lowpass = .5f;

        public UnityEvent peakEvent;
        public UnityEvent<float[]> onProcessed1;
        public UnityEvent<float[]> onThresholdChanged;

        protected readonly Queue<float> buffer2 = new Queue<float>();

        private float akku;
        private bool down;
        private float lastPeakTime;
        private float delta;
        public int l;

        protected override float Process1(AudioBuffer input)
        {

            if (BPMMin > BPMMax)
                BPMMin = BPMMax;

            while (buffer2.Count >= bufferSize)
                buffer2.Dequeue();

            var data = input.Current.spectrum;
            float energy = 0;
            l = (int)Mathf.Max(2,data.Length * lowpass);
            
            for (int i = 0; i < l; i++)
            {
                energy += data[i] * data[i];
            }
            energy /= l;
            Peak = Mathf.Max(Peak, energy);
            energy /= Peak;
            DetectPeaks(energy);
            return energy;
        }

        private void DetectPeaks(float energy)
        {
            delta = Time.realtimeSinceStartup - lastPeakTime;
            Peak *= 0.999f;
            var newbpm = 60f / delta;


            if (akku >= threshold && !down)
            {
                newbpm = Mathf.Clamp(newbpm, BPMMin, BPMMax);
                down = true;
                peakEvent.Invoke();
                if (Mathf.Abs(BpM - newbpm) < 4)
                    MeadianBPM = Mathf.Lerp(newbpm, MeadianBPM, bpmKeep);
                BpM = Mathf.Lerp(newbpm, BpM, bpmKeep);
                lastPeakTime = Time.realtimeSinceStartup;

            }

            if (akku < threshold && down)
                down = false;

            // Adjust akku
            if (akku >= energy)
                akku = Mathf.Lerp(energy, akku, release);
            else
                akku = Mathf.Lerp(energy, akku, attack);
            buffer2.Enqueue(Mathf.Max(akku, akku));
            //buffer2.Enqueue(down ? 1 : 0);
            akku = Mathf.Clamp01(akku);
            onProcessed1.Invoke(buffer2.ToArray());
            onThresholdChanged.Invoke(new float[] { threshold, threshold });
        }
    }
}