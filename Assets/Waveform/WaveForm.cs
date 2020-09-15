﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WaveForm : MonoBehaviour
{
    public bool spectrum;
    public float height = 200;
    public float width = 10;
    public float yOffset = 0;

    public AnimationCurve curve;

    [SerializeField]
    public int length = 1024;

    private Vector3[] positions = new Vector3[1024];
    LineRenderer lineRenderer;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }


    public void SetData(AudioProcessor processor)
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (spectrum)
            Debug.Log("spectrum " + processor.LogSpectrum.Length);
        float[] data = spectrum ? processor.LogSpectrum : processor.Data;
        positions = new Vector3[Mathf.Min(length, data.Length)];
        int l = 1;
        if (data.Length > 0)
            l = data.Length / positions.Length;

        for (int i = 0; i < positions.Length; i++)
            positions[i] = GetPosition(data, i, l);
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    private Vector3 GetPosition(float[] data, int index, int l)
    {
        float sum = 0;
        for (int i = 0; i < l; i++)
        {
            sum += data[index * l + i];
        }
        sum /= l;
        var h = spectrum ? (sum) * height + yOffset : sum * height + yOffset;
        return new Vector3(width * ((index / ((float)positions.Length)) * 2f - 1f), h, 0);
    }

}