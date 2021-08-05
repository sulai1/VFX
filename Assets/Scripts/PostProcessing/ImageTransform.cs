using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition; 

[Serializable, VolumeComponentMenu("Post-processing/Custom/ImageTransform")]

public class NewBehaviourScript : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public bool IsActive() => m_Material != null && intensity.value > 0f;
    Material m_Material;

    [SerializeField]
    private Texture2D m_background;

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        m_Material.SetFloat("_Intensity", intensity.value);
        m_Material.SetTexture("_Background",m_background);
        HDUtils.DrawFullScreen(cmd, m_Material, destination);
    }
}
