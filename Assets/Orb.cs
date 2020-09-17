using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Orb : MonoBehaviour
{
    public AudioProcessor processor;

    float threshold = .5f;
    float thresholdT = .5f;
    float min = float.MaxValue;
    float minT = float.MaxValue;
    float bpm = 120;

    [Range(0, .5f)]
    public float alpha = .2f;
    [Range(0, .5f)]
    public float alphaBPM = .1f;

    [Range(.1f, 1)]
    public float spacing;
    [Range(1f, 10)]
    public float spacing2;
    private float t = 0;
    private float t2 = 0;
    private VisualEffect vfx;

    float last = 0;

    // Start is called before the first frame update
    void Start()
    {
        vfx = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        if (processor != null)
        {
            t += Time.deltaTime;
            t2 += Time.deltaTime;
            vfx.SetFloat("Radius", .5f + processor.clipLoudness * 1000);

            float loudness = processor.clipLoudness * 100000;

            float energy = Energy(processor.Data);

            float value =  energy;


            vfx.SetFloat("Frequency", .5f + value);
            float thresh = threshold;// avg + (threashold - avg)*.9f;

            if (value > thresh && t > spacing)
            {
                Debug.Log(loudness + "min " + min + " Low  " + value + " Threashold " + threshold);
                vfx.SendEvent("Burst");
                threshold = value;
                float b = 60f / t;
                if (Mathf.Abs(t - last) < spacing)
                {
                    bpm = Mathf.Lerp(bpm, b, alphaBPM);
                }
                Debug.Log(t+" last: "+last +" b"+b+ " bpm " + bpm);
                last = t;
                t = 0;
            }
            else
            {
                threshold *= 1 - alpha* alpha;
            }

            minT = Mathf.Min(min, value);
            thresholdT = Mathf.Max(thresholdT, value);
            if (t2 > spacing2)
            {
                min = Mathf.Lerp(min, minT, alpha);
                threshold = Mathf.Lerp(threshold, thresholdT, alpha);

                minT = float.MaxValue;
                thresholdT = 0;
                t2 = 0;
            }

        }
    }

    private float Energy(float[] data)
    {
        float e = 0;
        for (int i = 0; i < data.Length; i++)
            e += data[i] * data[i];
        return e;
    }
}
