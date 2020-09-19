using System.Collections;
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

        IEnumerable<float> data = processor.Buffer.Data();
        int datalen = processor.Buffer.DataLength;
        int len = Mathf.Min(length, datalen);
        if (len != positions.Length)
            positions = new Vector3[len];
        int l = Mathf.Max(datalen/len, 1);

        var enumerator = data.GetEnumerator();
        for (int i = 0; i < positions.Length; i++)
            positions[i] = GetPosition(enumerator, i, l);
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    private Vector3 GetPosition(IEnumerator<float> data, int index, int l)
    {
        float sum = 0;
        for (int i = 0; i < l; i++)
        {
            data.MoveNext();
            sum += data.Current;
        }
        sum /= l;
        var h = spectrum ? (sum) * height + yOffset : sum * height + yOffset;
        var t = index / ((float)positions.Length);
        h += curve.Evaluate(t);
        return new Vector3(width * (t * 2f - 1f), h, 0);
    }

}
