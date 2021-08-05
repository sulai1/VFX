using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/Refract")]
public sealed class Refract : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public ClampedIntParameter octaves = new ClampedIntParameter(3,1,8);
    public Vector2Parameter offset = new Vector2Parameter(new Vector2(123456, 123456));
    public ClampedFloatParameter lacunarity = new ClampedFloatParameter(0.5f, 0f, 1f);
    public ClampedFloatParameter gain = new ClampedFloatParameter(0.5f, 0f, 1f);
    public ClampedFloatParameter amplitude = new ClampedFloatParameter(0.5f, 0f, 1f);
    public ClampedFloatParameter frequency = new ClampedFloatParameter(1f, 0f, 10f);
    public ClampedFloatParameter power = new ClampedFloatParameter(2f, 0f, 10f);
    public ClampedFloatParameter scale = new ClampedFloatParameter(1f, 0f, 10f);

    Material m_Material;

    public bool IsActive() => m_Material != null && intensity.value > 0f;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    const string kShaderName = "Hidden/Shader/Refract";

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume Refract is unable to load.");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return; 

        m_Material.SetFloat("_Intensity", intensity.value);
        m_Material.SetInt("_Octaves", octaves.value);
        m_Material.SetVector("_Offset", offset.value);
        m_Material.SetFloat("_Scale", scale.value);
        m_Material.SetFloat("_Lacunarity", lacunarity.value);
        m_Material.SetFloat("_Gain", gain.value);
        m_Material.SetFloat("_Amplitude", amplitude.value);
        m_Material.SetFloat("_Frequency", frequency.value);
        m_Material.SetFloat("_Power", power.value);
        m_Material.SetTexture("_InputTexture", source);
        HDUtils.DrawFullScreen(cmd, m_Material, destination);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}
