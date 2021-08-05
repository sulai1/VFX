using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Chip : IConstructable
{
    public int size = 256;
    public int centerSize = 16;
    public int laneCount = 20;
    [Range(0, 1)]
    public float laneWidth = .1f;
    public int iterations = 10;
    public Texture2D tex;

    [Range(0, 1)]
    public float curveProp = .1f;
    int[] map;
    List<(int, int)>[] lanes;

    List<(int, int)> offsets = new List<(int, int)>()
    {
        (1,0),(0,1),(-1,0),(0,-1),
        (-1,-1),(1,-1),(-1,1),(1,1),
    };

    private void Start()
    {
        Create();
    }
    override public void Create()
    {
        CreateMap();
        tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        tex.SetPixelData(map, 0);
        tex.Apply();

        Mesh mesh = new Mesh();
        List<int> indices = new List<int>();
        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();

        int laneIndex = 0;
        foreach (var lane in lanes)
        {
            if (lane.Count > 1)
            {
                int count;
                var (x0, y0) = lane[0];
                Color laneColor = new Color(laneIndex / (float)laneCount, 0, 0);

                AddDot(vertices, indices, colors, laneColor, new Vector2(x0, y0), 2 * laneWidth);

                var (x1, y1) = lane[1];

                var va = new Vector2(x0, y0);
                var vb = new Vector2(x1, y1);

                var d = (va - vb).normalized * laneWidth / 2f;
                var n = new Vector2(d.y, -d.x);


                vertices.Add(va + n);
                vertices.Add(va - n);

                colors.Add(laneColor);
                colors.Add(laneColor);

                for (int i = 0; i < lane.Count - 2; i++)
                {

                    (x1, y1) = lane[i];
                    var (x2, y2) = lane[i + 1];
                    var (x3, y3) = lane[i + 2];

                    var (dx, dy) = (x1 - x2, y1 - y2);
                    if ((x2 - dx, y2 - dy) == (x3, y3))
                        AddSimpleLine(indices, vertices, colors, lane, laneColor, x1, y1, x2, y2);
                    else
                    {
                        va = new Vector2(x1, y1);
                        vb = new Vector2(x2, y2);
                        var vc = new Vector2(x3, y3);

                        d = (va - vb).normalized * laneWidth / 2f;
                        n = new Vector2(d.y, -d.x);

                        d = (vb - vc).normalized * laneWidth / 2f;
                        var nb = new Vector2(d.y, -d.x);

                        vertices.Add(IntersectLine2D(va + n, vb + n, vb + nb, vc + nb));
                        vertices.Add(IntersectLine2D(va - n, vb - n, vb - nb, vc - nb));

                        colors.Add(laneColor);
                        colors.Add(laneColor);


                        count = vertices.Count;
                        //Debug.Log(vertices[count - 2] + "-" + vertices[count - 1]);
                        //Debug.Log("" + va.ToString("F2") + "," + vb.ToString("F2") + ":" + n.ToString("F2") + " - " + vb.ToString("F2") + "," + vc.ToString("F2") + ":" + nb.ToString("F2"));

                        indices.Add(count - 3);
                        indices.Add(count - 4);
                        indices.Add(count - 2);
                        indices.Add(count - 1);
                    }
                }

                (x0, y0) = lane[lane.Count - 2];
                (x1, y1) = lane[lane.Count - 1];
                // add last quad and dot
                AddSimpleLine(indices, vertices, colors, lane, laneColor, x0, y0, x1, y1);

                AddDot(vertices, indices, colors, laneColor, new Vector2(x1, y1), 2 * laneWidth);

                laneIndex++;
            }
        }

        for (int i = 0; i < vertices.Count; i++)
            vertices[i] = vertices[i] / size * 2 - Vector3.one;

        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetIndices(indices, MeshTopology.Quads, 0);

        GetComponent<MeshFilter>().sharedMesh = mesh;
        for (int i = 0; i < 10; i++)
        {
            Debug.Log(mesh.colors[mesh.colors.Length - i - 1]);
        }
    }

    private void AddSimpleLine(List<int> indices, List<Vector3> vertices, List<Color> colors, List<(int, int)> lane, Color laneColor, int x0, int y0, int x1, int y1)
    {
        var va = new Vector2(x0, y0);
        var vb = new Vector2(x1, y1);

        var d = (va - vb).normalized * laneWidth / 2f;
        var n = new Vector2(d.y, -d.x);

        vertices.Add(vb + n);
        vertices.Add(vb - n);

        colors.Add(laneColor);
        colors.Add(laneColor);

        var count = vertices.Count;
        indices.Add(count - 3);
        indices.Add(count - 4);
        indices.Add(count - 2);
        indices.Add(count - 1);
    }

    private Vector2 IntersectLine2D(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float d = (p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x);

        float x = (p1.x * p2.y - p1.y * p2.x) * (p3.x - p4.x) - (p1.x - p2.x) * (p3.x * p4.y - p3.y * p4.x);
        x /= d;

        float y = (p1.x * p2.y - p1.y * p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x * p4.y - p3.y * p4.x);
        y /= d;
        return new Vector2(x, y);
    }


    private static void AddDot(List<Vector3> vertices, List<int> indices, List<Color> colors, Color laneColor, Vector2 v, float width)
    {
        vertices.Add(v + new Vector2(+width, -width));
        vertices.Add(v + new Vector2(+width, +width));
        vertices.Add(v + new Vector2(-width, +width));
        vertices.Add(v + new Vector2(-width, -width));

        colors.Add(laneColor);
        colors.Add(laneColor);
        colors.Add(laneColor);
        colors.Add(laneColor);

        var count = vertices.Count;

        indices.Add(count - 1);
        indices.Add(count - 2);
        indices.Add(count - 3);
        indices.Add(count - 4);
    }

    private void CreateMap()
    {
        map = new int[size * size];
        lanes = new List<(int, int)>[laneCount];
        for (int i = 0; i < laneCount; i++)
        {
            var x = UnityEngine.Random.Range(0, size);
            var y = UnityEngine.Random.Range(0, size);

            lanes[i] = new List<(int, int)>() { (x, y) };
            map[x + y * size] = int.MaxValue;
        }
        for (int j = 0; j < iterations; j++)
        {
            int l = UnityEngine.Random.Range(1, laneCount - 1);
            for (int i = 0; i < l; i++)
            {
                var c = lanes[i].Count - 1;
                var (x, y) = lanes[i][c];

                float xf = size / 2f - x;
                float yf = size / 2f - y;

                int ox, oy;
                if (UnityEngine.Random.value > curveProp)
                    if (xf > yf)
                        yf = 0;
                    else
                        xf = 0;
                ox = xf > 0 ? 1 : xf < 0 ? -1 : 0;
                oy = yf > 0 ? 1 : yf < 0 ? -1 : 0;


                if (Check(x, y, ox, oy, map))
                {
                    map[x + ox + (y + oy) * size] = int.MaxValue;
                    lanes[i].Add((x + ox, y + oy));
                }
                else
                {
                    if (xf > yf)
                        oy = 0;
                    else
                        ox = 0;
                    if (Check(x, y, ox, oy, map))
                    {
                        map[x + ox + (y + oy) * size] = int.MaxValue;
                        lanes[i].Add((x + ox, y + oy));
                    }
                    else
                    {
                        foreach (var o in offsets.OrderBy(_ => UnityEngine.Random.Range(0, size)))
                        {
                            int index = x + o.Item1 + (y + o.Item2) * size;
                            if (index >= 0 && index < map.Length && Check(x, y, o.Item1, o.Item2, map))
                            {

                                map[index] = int.MaxValue;
                                lanes[i].Add((x + o.Item1, y + o.Item2));
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private bool Check(int x, int y, int ox, int oy, int[] map)
    {
        int cx = x + ox;
        int cy = y + oy;

        if (InsideCenterBounds(cx, cy))
            return false;
        if (IndexOutOfBounds(cx, cy))
            return false;

        for (int nx = cx - 1; nx <= cx + 1; nx++)
            for (int ny = cy - 1; ny <= cy + 1; ny++)
            {
                if (nx != x || ny != y)
                    if (!IndexOutOfBounds(nx, ny) && map[nx + ny * size] > 0)
                        return false;
            }
        return true;
    }

    private bool InsideCenterBounds(int x, int y)
    {
        int s2 = size / 2;
        int c2 = centerSize / 2;
        return x > s2 - c2 && x < s2 + c2 && y > s2 - c2 && y < s2 + c2;
    }

    private bool IndexOutOfBounds(int x, int y)
    {
        return x < 0 || x >= size || y < 0 || y >= size;
    }
}
