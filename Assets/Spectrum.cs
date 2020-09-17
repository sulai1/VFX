using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Spectrum : MonoBehaviour
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

        float[] data = processor.Spectrum;
        int len = Mathf.Min(length, data.Length);
        if (len != positions.Length)
            positions = new Vector3[len];
        int l = 1;
        if (data.Length > 0)
            l = data.Length / 8 / positions.Length;

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
            int j = index * l + i;
            sum += (float)data[j];
        }
        sum /= l;
        sum *= Mathf.Log(index+1);
        var h = spectrum ? (sum) * height + yOffset : sum * height + yOffset;
        var t = (index + 1 * index + 1) / ((float)((positions.Length + 1) * (positions.Length + 1)));
        t *= positions.Length/2;
        h += curve.Evaluate(t);
        return new Vector3(width * (t * 2f - 1f), h, 0);
    }

}
