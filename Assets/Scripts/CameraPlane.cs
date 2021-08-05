using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlane : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var cam = Camera.main;
        CreateMesh(cam);

    }

    private void CreateMesh(Camera cam)
    {
        Vector3[] frustumCorners = new Vector3[4];
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        for (int i = 0; i < 4; i++)
        {
            var worldSpaceCorner = cam.transform.TransformVector(frustumCorners[i]);
            Debug.DrawRay(cam.transform.position, worldSpaceCorner, Color.blue);
        }

        var mesh = new Mesh();
        mesh.vertices = frustumCorners;
        mesh.SetIndices(new int[] { 0, 1, 2, 3 }, MeshTopology.Quads, 0);

        var meshFilter = GetComponent<MeshFilter>();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
