using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Plot : MonoBehaviour
{
    public bool logX, logY, showPeaks;

    public AnimationCurve curve;

    public int resolution = 1024;
    
    protected LineRenderer lineRenderer;

    private Vector3[] positions = new Vector3[1024];
    
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }


    public void SetData(AudioProcessor processor)
    {
        SetData(processor.Buffer.DataLength, processor.Buffer.Data());
    }

    public void SetData(int length, IEnumerable<float> data)
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        int datalen = length;
        int len = Mathf.Min(length, datalen);
        if (len != positions.Length)
            positions = new Vector3[len];
        int l = Mathf.Max(datalen / len, 1);

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
        var h = logY ? Mathf.Log(sum + 1) : sum;
        var t = logX ? (Mathf.Log(index + 1, 2) - 1) / (Mathf.Log(positions.Length - 1, 2) - 1) : index / (float)positions.Length;
        h += curve.Evaluate(t);
        return new Vector3(t * 2f - 1f, h, 0);
    }

    private Vector3 GetPosition(float[] data, int index, int l)
    {
        //float sum = 0;
        //for (int i = 0; i < l; i++)
        //{
        //    int j = index * l + i;
        //    sum += (float)data[j];
        //}
        //sum /= l;
        //sum *= Mathf.Log(index+1,10);
        var h = data[index];
        var t = logY ? (Mathf.Log(index + 1, 2) - 1) / (Mathf.Log(data.Length - 1, 2) - 1) : index / (float)data.Length;

        return new Vector3((t * 2f - 1f), h, 0);
    }
}
