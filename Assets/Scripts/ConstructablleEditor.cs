using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IConstructable), true)]
public class ConstructablleEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var t = target as IConstructable;
        if (GUILayout.Button("Create"))
            t.Create();
        switch (t)
        {
            case Chip c:
                EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(false,500), c.tex);
                break;
            default:
                break;
        }
    }
}
