using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class BeatWave : MonoBehaviour
{
    // Start is called before the first frame update
    public void SetTexture(Texture2D texture,int index)
    {
        var vfx = GetComponent<VisualEffect>();
        vfx.SetTexture("wave", texture);
        vfx.SetInt("index", index);
        vfx.SetInt("width", texture.width);
        vfx.SetInt("height", texture.height);
    }
}
