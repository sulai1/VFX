using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class LineWave : AudioVisualEffect
{

    public float bpm = 120;
    public float angleOffset;

    float lastTime = 0;

    protected VisualEffect VFX => GetComponent<VisualEffect>();
    public override bool Enabled { get => VFX.enabled; set => VFX.enabled = value; }

    // Update is called once per frame
    [ExecuteInEditMode]
    void Update()
    {
        if (Time.time - lastTime > bpm / 60  )
        {
            VFX.SetFloat("Frequency", Random.Range(0.1f, 20f));
            VFX.SetFloat("AngleOffset", angleOffset);
            Debug.Log("Burst");
            lastTime = Time.time;
        }
    }
}
