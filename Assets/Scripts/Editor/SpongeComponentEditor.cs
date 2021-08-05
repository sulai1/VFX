using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;


[VolumeComponentEditor(typeof(Sponge))]
public class SpongeComponentEditor : VolumeComponentEditor
{
    private Sponge sponge;
    private SerializedDataParameter Intensity;
    private bool opacityFoldout;
    List<SerializedDataParameter> opacityParameters = new List<SerializedDataParameter>();
    private bool motionFoldout;
    List<SerializedDataParameter> motionParamters = new List<SerializedDataParameter>();
    private bool sensorFoldout;
    List<SerializedDataParameter> sensorParameters = new List<SerializedDataParameter>();

    public override void OnEnable()
    {
        base.OnEnable();
        var o = new PropertyFetcher<Sponge>(serializedObject);
        sponge =  target as Sponge;
        Intensity = Unpack(o.Find(x => x.intensity));
        opacityParameters = new List<SerializedDataParameter>() 
        {
            Unpack(o.Find(x => x.opacity)),
            Unpack(o.Find(x => x.evaporateSpeed)),
            Unpack(o.Find(x => x.diffuseSpeed)),
            Unpack(o.Find(x => x.agentStrength)),
        };

        motionParamters = new List<SerializedDataParameter>()
        {
            Unpack(o.Find(x => x.moveSpeed)),
            Unpack(o.Find(x => x.turnSpeed)),
            Unpack(o.Find(x => x.randomStrength)),
            Unpack(o.Find(x => x.force)),
            Unpack(o.Find(x => x.repell)),
        };


        sensorParameters = new List<SerializedDataParameter>()
        {
            Unpack(o.Find(x => x.sensorOffsetDst)),
            Unpack(o.Find(x => x.sensorAngleSpacing)),
            Unpack(o.Find(x => x.sensorSize)),
        };
        var opacity = Unpack(o.Find(x => x.opacity));
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PropertyField(Intensity);
        if (GUILayout.Button("Reset"))
        {
            sponge.Setup();
        }
        opacityFoldout = EditorGUILayout.Foldout(opacityFoldout, "Opacity");
        if (opacityFoldout)
        {
            foreach (var ctrl in opacityParameters)
            {
                PropertyField(ctrl);
            }
        }
        motionFoldout = EditorGUILayout.Foldout(motionFoldout, "Motion");
        if (motionFoldout)
        {
            foreach (var ctrl in motionParamters)
            {
                PropertyField(ctrl);
            }
        }
        sensorFoldout = EditorGUILayout.Foldout(motionFoldout, "Sensor");
        if (sensorFoldout)
        {
            foreach (var ctrl in sensorParameters)
            {
                PropertyField(ctrl);
            }
        }
    }
}
