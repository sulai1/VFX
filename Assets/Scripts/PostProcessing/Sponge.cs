using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable]
public struct Agent
{
    public Vector2 position;
    public float angle;
    public Color speciesMask;
}

[Serializable, VolumeComponentMenu("Post-processing/Custom/Sponge")]
public sealed class Sponge : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(1f, 0f, 1f);

    [Tooltip("Show the Colors instead of transforming.")]
    public ClampedFloatParameter opacity = new ClampedFloatParameter(1f, 0f, 1f);
    [Tooltip("The amount that trails get blurred.")]
    public ClampedFloatParameter diffuseSpeed = new ClampedFloatParameter(0.9f, 0f, 1f);
    [Tooltip("The amount that trails decrease over time.")]
    public ClampedFloatParameter evaporateSpeed = new ClampedFloatParameter(0.9f, 0f, 1f);
    [Tooltip("Strength of the agents trail.")]
    public ClampedFloatParameter agentStrength = new ClampedFloatParameter(0, 0, 1);

    [Tooltip("The at which the agents sensors are positioned.")]
    public ClampedIntParameter sensorOffsetDst = new ClampedIntParameter(1, 1, 20);
    [Tooltip("The size at whicht the agents sample trails.")]
    public ClampedIntParameter sensorSize = new ClampedIntParameter(1, 1, 4);
    [Tooltip("The angle at which the sensors are positioned.")]
    public ClampedIntParameter sensorAngleSpacing = new ClampedIntParameter(1, 1, 360);
    [Tooltip("Negative effect for other trails.")]
    public ClampedFloatParameter repell = new ClampedFloatParameter(0.5f, 0, 1);
    [Tooltip("Effect of souurce image vs generated trails.")]
    public ClampedFloatParameter sourceStrength = new ClampedFloatParameter(0.5f, 0, 1);

    [Tooltip("Speed at which agents move.")]
    public ClampedFloatParameter moveSpeed = new ClampedFloatParameter(1f, 0f, 1f);
    [Tooltip("The speed at which the agent can turn.")]
    public ClampedFloatParameter turnSpeed = new ClampedFloatParameter(1, 0, 1);
    [Tooltip("The amount of random turning.")]
    public ClampedFloatParameter randomStrength = new ClampedFloatParameter(0, 0, 1);
    [Tooltip("A constant force that gets applied to the agents.")]
    public Vector2Parameter force = new Vector2Parameter(Vector2.zero);


    [Tooltip("Number of agents.")]
    public ClampedIntParameter numAgents = new ClampedIntParameter(256, 1, 10240);
    [Tooltip("Spawn region Center.")]
    public Vector2Parameter spawnCenter = new Vector2Parameter(Vector2.zero);
    [Tooltip("Spawn region size.")]
    public Vector2Parameter spawnOuterSize = new Vector2Parameter(Vector2.zero);
    [Tooltip("Spawn region size.")]
    public Vector2Parameter spawnInnerSize = new Vector2Parameter(Vector2.zero);
    [Tooltip("Spawn region shape.")]
    public BoolParameter spawnElipse = new BoolParameter(false);

    [Tooltip("Restricts Agents to move.")]
    public TextureParameter mask = new TextureParameter(null);

    [Tooltip("Restricts Agents to move.")]
    public TextureParameter color = new TextureParameter(null);



    [SerializeField]
    ComputeShader cs;
    [SerializeField]
    private RenderTexture trails;
    [SerializeField]
    private Agent[] agents;
    private ComputeBuffer buff;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    Material m_Material;

    const string kShaderName = "Hidden/Shader/Sponge";
    public override void Setup()
    {
        cs = Resources.Load<ComputeShader>("Compute Shader/SpongeCompute");
        if (cs == null)
            Debug.Log("Sponge cs not found ");
        trails = new RenderTexture(Screen.width, Screen.height, 4)
        {
            enableRandomWrite = true
        };
        trails.Create();

        CreateAgents((int)numAgents.value);


        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume Refract is unable to load.");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        active = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        buff?.Dispose();
        active = false;
    }

    private void CreateAgents(int groups)
    {
        int agentCount = groups * 64;
        agents = new Agent[agentCount];
        for (int i = 0; i < agentCount; i++)
        {
            Vector2 scale = new Vector2(trails.width, trails.height);
            var p = (UnityEngine.Random.insideUnitCircle + Vector2.one) * .5f * scale; 
            p = (p + scale * .5f) * .5f;
            //var p = -Vector2.one;
            agents[i] = new Agent()
            {
                position = p,
                angle = UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad,
            };
            if (i < 0.33 * agentCount)
                agents[i].speciesMask = Color.red;
            else if (i < 0.67 * agentCount)
                agents[i].speciesMask = Color.blue;
            else
                agents[i].speciesMask = Color.green;
        }
        buff = new ComputeBuffer(agents.Length, 28);
        buff.SetData(agents);
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (!IsActive())
        {
            return;
        }
        else
        {
            //Debug.Log($"Render {trails.width}/{trails.width / 8.0}, {trails.height}/{trails.height / 8.0}");
            int kernelIndex = cs.FindKernel("EaseOut");
            cs.SetTexture(kernelIndex, "Trails", trails);
            cs.SetTexture(kernelIndex, "Source", source);
            //cs.SetTexture(kernelIndex, "Destination", destination);
            cs.SetInt("width", trails.width);
            cs.SetInt("height", trails.height);
            cs.Dispatch(kernelIndex, trails.width, trails.height, 1);

            kernelIndex = cs.FindKernel("Update");
            cs.SetTexture(kernelIndex, "Trails", trails);
            cs.SetTexture(kernelIndex, "Source", source);
            cs.SetInt("width", trails.width);
            cs.SetInt("height", trails.height);
            cs.SetTexture(kernelIndex, "Mask", mask.value);
            cs.SetInt("maskWidth", mask.value.width);
            cs.SetInt("maskHeight", mask.value.height);

            cs.SetFloat("deltaTime", Time.deltaTime);
            cs.SetFloat("moveSpeed", moveSpeed.value * 100);
            cs.SetFloat("diffuseSpeed", diffuseSpeed.value * 10);
            cs.SetFloat("evaporateSpeed", evaporateSpeed.value * evaporateSpeed.value);

            cs.SetFloat("sensorOffsetDst", sensorOffsetDst.value);
            cs.SetInt("sensorSize", sensorSize.value);
            cs.SetFloat("sensorAngleSpacing", sensorAngleSpacing.value * Mathf.Deg2Rad);
            cs.SetFloat("repell", repell.value);
            cs.SetFloat("sorceStrength", sourceStrength.value);
            cs.SetFloat("turnSpeed", turnSpeed.value);
            cs.SetFloat("randomStrength", randomStrength.value);
            cs.SetFloat("agentStrength", agentStrength.value);
            cs.SetVector("force", force.value);

            //spawn
            cs.SetInt("numAgents", (int)numAgents.value * 64);
            cs.SetVector("spawnCenter", spawnCenter.value);
            cs.SetVector("spawnInnerSize", spawnInnerSize.value);
            cs.SetVector("spawnOuterSize", spawnOuterSize.value);


            cs.SetBuffer(kernelIndex, "agents", buff);
            cs.Dispatch(kernelIndex, (int)numAgents.value, 1, 1);

            //Graphics.Blit(trails, destination);
            //cmd.Blit(trails, destination);
            m_Material.SetFloat("_Intensity", intensity.value * 100);
            m_Material.SetFloat("_Opaque", opacity.value * opacity.value);
            m_Material.SetTexture("_InputTexture", source);
            m_Material.SetTexture("_Color", color.value);
            m_Material.SetVector("size", new Vector2(color.value.width,color.value.height));
            m_Material.SetTexture("_UV", trails);
            HDUtils.DrawFullScreen(cmd, m_Material, destination);
        }
    }

    public override void Cleanup()
    {
        buff.Dispose();
    }

    public bool IsActive()
    {
        return cs != null && trails != null && intensity.value > 0 && active && this.visibleInSceneView;
    }
}
