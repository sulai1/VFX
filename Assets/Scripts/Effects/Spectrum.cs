using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Spectrum : MonoBehaviour
{
    public bool logY = false;

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
        int len = data.Length;
        if (len != positions.Length)
            positions = new Vector3[len];
        for (int i = 0; i < len; i++)
            positions[i] = GetPosition(data, i);
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    private Vector3 GetPosition(float[] data, int index)
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
        var t = logY ? (Mathf.Log(index+1,2)-1) / (Mathf.Log(data.Length - 1,2)-1): index / (float)data.Length;

        return new Vector3( (t * 2f - 1f), h, 0);
    }
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(0, 0.3f, 0), new Vector3( 2, 1));
    }
}
