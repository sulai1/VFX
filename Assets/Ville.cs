using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Volume))]
public class Ville : MonoBehaviour
{
    private Volume volume;

    public float intensity = 0;

    // Start is called before the first frame update
    void Start()
    {
        volume = GetComponent<Volume>();
    }

    // Update is called once per frame
    void Update()
    {
        volume.profile.TryGet<ChromaticAberration>(out ChromaticAberration ca);
        if(ca != null)
            ca.intensity.value = intensity;
    }
}
