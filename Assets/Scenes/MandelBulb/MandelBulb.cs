using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class MandelBulb : MonoBehaviour
{
    [SerializeField]
    private int width = 256, height = 256;
    public int Width => width;
    public int Height => height;

    [SerializeField]
    private RenderTexture tex;
    private ComputeShader cs;




    [Range(1, 20)]
    public float fractalPower = 10;
    public float darkness = 70;

    [Header("Colour mixing")]
    [Range(0, 1)] public float blackAndWhite;

    [SerializeField]
    private Gradient fgGradient = new Gradient();

    [SerializeField]
    private Gradient bgGradient = new Gradient();
    private ComputeBuffer gradientData;
    private GradientData[] array;

    public struct GradientData
    {
        internal const int Stride = 8 * 16 + 8 * 4 ;
        public Vector4 color;
        public float key;
    }

    private void OnEnable()
    {
        Create();
    }

    private void Create()
    {
        Debug.Log("enable");
        cs = Resources.Load<ComputeShader>("Compute Shader/MandelBulbCompute");
        if (cs == null)
            Debug.Log("cs not found");
        else
            Debug.Log("cs found");

        tex = new RenderTexture(Width, Height, 4); 
        tex.enableRandomWrite = true;
        tex.Create();

        gradientData = new ComputeBuffer(8, GradientData.Stride);
        array = new GradientData[16];
        for (int i = 0; i < fgGradient.colorKeys.Length; i++)
        {
            var k = fgGradient.colorKeys[i];
            array[i] = new GradientData()
            {
                color = new Vector4(k.color.r, k.color.g, k.color.b, k.color.a),
                key = k.time,
            };
        }
        for (int i = 0; i < bgGradient.colorKeys.Length; i++)
        {
            var k = bgGradient.colorKeys[i];
            array[i+8] = new GradientData()
            {
                color = new Vector4(k.color.r, k.color.g, k.color.b, k.color.a),
                key = k.time,
            };
        }

        gradientData.SetData( array );
    }

    // Start is called before the first frame update
    void Start()
    {
        Create();
    }
    // Update is called once per frame 
    void Update()
    {

        if (cs != null && tex != null)
        {
            Debug.Log("run "+string.Join(",",array.Select(e=>e.key)));
            Compute(Camera.main);
        }
        else
        {
            OnEnable();
        }
    }

    public void Compute(Camera camera)
    {
        if (cs != null && tex != null)
        {
            int kernelIndex = cs.FindKernel("CSMain");

            cs.SetTexture(0, "Destination", tex);
            cs.SetFloat("power", Mathf.Max(fractalPower, 1.01f));
            cs.SetFloat("darkness", darkness);
            cs.SetFloat("blackAndWhite", blackAndWhite);

            cs.SetMatrix("_CameraToWorld", Camera.main.cameraToWorldMatrix);
            cs.SetMatrix("_CameraInverseProjection", Camera.main.projectionMatrix.inverse);
            cs.SetVector("_LightDirection", Camera.main.transform.forward);

            cs.SetTexture(kernelIndex, "Result", tex);
            cs.SetInt("width", tex.width);
            cs.SetInt("height", tex.height);

            cs.SetBuffer(kernelIndex, "gradients", gradientData);
            cs.Dispatch(kernelIndex, Mathf.CeilToInt(tex.width / 8f), Mathf.CeilToInt(tex.height / 8f), 1);
            GetComponent<MeshRenderer>().sharedMaterial.mainTexture = tex;
        }
    }

    private void SetRay(ComputeShader cs, string name, Ray ray)
    {
        //cs.SetVector(name, ray.origin);
    }

    private void OnDrawGizmos()
    {
        var tmp_mat = Gizmos.matrix;
        var verts = GetComponent<MeshFilter>().sharedMesh.vertices;
        foreach (var v in verts)
            DrawCornerRay(transform.TransformPoint(v));
        Gizmos.matrix = tmp_mat;
    }

    private void DrawCornerRay(Vector3 pos)
    {
        var cam = Camera.main.transform.position;
        var dir = pos - cam;    
        Gizmos.color = Color.red;
        Gizmos.DrawRay(pos, dir);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(cam, dir);
        Handles.Label(pos, "p");
    }
}
