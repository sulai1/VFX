using Assets.AudioViz;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAVFX : AudioVisualEffect
{
    [Parameter]
    public int p;

    bool enabled = false;
    public override bool Enabled { get => enabled; set => enabled=value; }


}
