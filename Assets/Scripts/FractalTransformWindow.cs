using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class FractalTransformWindow : EditorWindow
{

    Texture2D input;
    Texture2D render;
    private Texture2D transform;
    private Gradient gradient;
    private float swirl;

    [MenuItem("Window/My Window")]
    // Start is called before the first frame update
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(FractalTransformWindow));
    }
    private void OnGUI()
    {
        input = (Texture2D)EditorGUILayout.ObjectField("input", input, typeof(Texture2D), false);


        EditorGUILayout.BeginHorizontal();
        if (input)
        {
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Init"))
            {
                InitTransform();
            }

            EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(false, input.height), input);
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical();

            gradient = EditorGUILayout.GradientField(gradient);

            swirl = EditorGUILayout.Slider(swirl, 0, 1);
            if (GUILayout.Button("Swirl"))
            {
                Swirl();
            }
            if (GUILayout.Button("HorseShoe"))
            {
                HorseShoe();
            }
            if (transform != null)
                EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(false, transform.height), transform);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            if (transform != null && GUILayout.Button("Render"))
            {
                Render();
            }
            if (render != null) EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(false, render.height), render);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void InitTransform()
    {
        transform = new Texture2D(input.width, input.height, TextureFormat.RGB24, false);
        transform.filterMode = FilterMode.Point;
        byte[] data = new byte[transform.width * transform.height * 3];
        int index = 0;
        for (int y = 0; y < transform.height; y++)
            for (int x = 0; x < transform.width; x++)
            {
                data[index++] = (byte)x;
                data[index++] = (byte)y;
                data[index++] = 0;
            }
        transform.SetPixelData(data, 0);
        transform.Apply();
    }
    private void Swirl()
    {
        byte[] data = transform.GetRawTextureData();
        byte[] newData = new byte[data.Length];

        for (int y = 0; y < transform.height; y++)
            for (int x = 0; x < transform.width; x++)
            {
                int index = 0;
                float fx, fy, tx, ty;
                tx = x / (float)transform.width * 2 - 1;
                ty = y / (float)transform.height * 2 - 1;

                tx *= swirl * 100;
                ty *= swirl * 100;

                float r = Mathf.Sqrt(tx * tx + ty * ty);
                r *= r;
                fx = tx * Mathf.Sin(r) - ty * Mathf.Cos(r);
                fy = ty * Mathf.Cos(r) + ty * Mathf.Sin(r);

                fx = fx * .5f + .5f;
                fy = fy * .5f + .5f;

                fx -= (int)fx;
                fy -= (int)fy;

                int i = x + y * transform.width;
                i *= 3;

                if (fx >= 0 && fx <= 1 && fy >= 0 && fy <= 1)
                {
                    int ix = (int)(fx * transform.width);
                    int iy = (int)(fy * transform.height);

                    index = ix + iy * transform.width;
                    index *= 3;


                    newData[i] = data[index];
                    newData[i + 1] = data[index + 1];
                    newData[i + 2] = (byte)((byte)(fx) * (byte)(fy));
                }
            }
        transform.SetPixelData(newData, 0);
        transform.Apply();
    }
    private void HorseShoe()
    {
        byte[] data = transform.GetRawTextureData();
        byte[] newData = new byte[data.Length];

        for (int y = 0; y < transform.height; y++)
            for (int x = 0; x < transform.width; x++)
            {
                int index = 0;
                float fx, fy, tx, ty;
                tx = x / (float)transform.width * 3 - 1;
                ty = y / (float)transform.height * 3 - 1;

                tx /= Mathf.Max(float.Epsilon, swirl / 200);
                ty /= Mathf.Max(float.Epsilon, swirl / 200);

                float r = Mathf.Sqrt(tx * tx + ty * ty);
                r *= r;
                fx = (tx - ty) * (tx + ty) / r;
                fy = 2 * x * y / r;

                fx = fx * .5f + .5f;
                fy = fy * .5f + .5f;

                fx -= (int)fx;
                fy -= (int)fy;

                int i = x + y * transform.width;
                i *= 3;

                if (fx >= 0 && fx <= 1 && fy >= 0 && fy <= 1)
                {
                    int ix = (int)(fx * transform.width);
                    int iy = (int)(fy * transform.height);

                    index = ix + iy * transform.width;
                    index *= 3;


                    newData[i] = data[index];
                    newData[i + 1] = data[index + 1];
                    newData[i + 2] = (byte)((byte)(fx) * (byte)(fy));
                }
            }
        transform.SetPixelData(newData, 0);
        transform.Apply();
    }
    private void Render()
    {
        if (!render)
            render = new Texture2D(input.width, input.height, TextureFormat.RGB24, false);
        byte[] texData = input.GetRawTextureData();
        byte[] transformData = transform.GetRawTextureData();
        byte[] newData = new byte[input.width * input.height * 3];

        for (int y = 0; y < transform.height; y++)
            for (int x = 0; x < transform.width; x++)
            {

                int i = x + y * transform.width;

                byte tx = transformData[i * 3];
                byte ty = transformData[i * 3 + 1];
                float d = transformData[i * 3 + 2] / 256f;

                float v;
                switch (input.format)
                {
                    case TextureFormat.RGB24:
                    case TextureFormat.Alpha8:
                        v = texData[tx + ty * transform.width] / 256f;
                        break;
                    default:
                        Color c = input.GetPixel(tx, ty);
                        v = c.grayscale;
                        break;
                }
                var col = gradient.Evaluate(v);
                newData[i * 3] = (byte)(col.r * 256);
                newData[i * 3 + 1] = (byte)(col.g * 256);
                newData[i * 3 + 2] = (byte)(col.b * 256);

            }
        render.SetPixelData(newData, 0);
        render.Apply();
    }
}
