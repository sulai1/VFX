using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class Spectrum : MonoBehaviour
{

    public AudioProcessor processor;

    VisualEffect vfx;
    Texture2D tex;

    // Start is called before the first frame update
    void Start()
    {
        tex = new Texture2D(10, 1, TextureFormat.ARGB32, false);
        vfx = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!processor)
            return;
        tex.SetPixelData(processor.LogSpectrum, 0, 0);
        vfx.SetTexture("Spectrum", tex);
        vfx.SendEvent("Emit");
        tex.Apply();
    }
}
