using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Plot : MonoBehaviour
{
    public bool logX, logY;

    public AnimationCurve shape;

    private int resolution = 1024;

    private LineRenderer lineRenderer;

    private Vector3[] positions = new Vector3[1024];
    private int h_index;

    private int Index { get => h_index; set => h_index = resolution == 0 ? 0 : value % resolution; }

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Add(float value)
    {
        if (resolution != positions.Length)
            positions = new Vector3[resolution];
        positions[Index] = GetPosition(value, Index++);
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    public void SetAll(IEnumerable<float[]> alldata) => SetData(alldata.SelectMany(d => d));

    public void SetData(IEnumerable<float> data) => SetData(data.ToArray());
    public void SetData(float[] data)
    {
        Index = 0;
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        resolution = data.Length;
        if (resolution != positions.Length)
            positions = new Vector3[resolution];

        for (int i = 0; i < data.Length; i++)
            positions[i] = GetPosition(data[i], i);
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }


    private Vector3 GetPosition(float val, int index)
    {
        var h = logY ? Mathf.Log(val + 1) : val;
        var t = logX ? (Mathf.Log(index + 1, 2) - 1) / (Mathf.Log(positions.Length - 1, 2) - 1) : index / (float)positions.Length;
        h += shape.Evaluate(t);
        return new Vector3(t * 2f - 1f, h, 0);
    }
}
