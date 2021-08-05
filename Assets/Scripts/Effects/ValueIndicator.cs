using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueIndicator : Plot
{
    public float release, threshold;
    private Queue<float> data = new Queue<float>();
    Vector3[] positions = new Vector3[1024];

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Insert(float value)
    {
        if (data.Count > resolution)
            data.Dequeue();
        data.Enqueue(value);


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
