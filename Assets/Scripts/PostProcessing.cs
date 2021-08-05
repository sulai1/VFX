using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PostProcessingSchedule
{
    public KeyCode keyCode;
    public float startTime;
    public float period;
    public bool active;
}

[RequireComponent(typeof(Volume))]
public class PostProcessing : MonoBehaviour
{
    private const float SPEED = 0.1f;
    private Volume volume;
    public float speed = 1;

    KeyCode[] keys = new KeyCode[] {
        KeyCode.Keypad0,
        KeyCode.Keypad1,
        KeyCode.Keypad2,
        KeyCode.Keypad3,
        KeyCode.Keypad4,
        KeyCode.Keypad5,
        KeyCode.Keypad6,
        KeyCode.Keypad7,
        KeyCode.Keypad8,
        KeyCode.Keypad9,
    };

    PostProcessingSchedule[] schedule;

    bool recieveInput = false;
    int activeKeys = 0;


    // Start is called before the first frame update
    void Start()
    {
        schedule = new PostProcessingSchedule[keys.Length];
        for (int i = 0; i < keys.Length; i++)
        {
            schedule[i] = new PostProcessingSchedule() { keyCode = keys[i] };
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            recieveInput = !recieveInput;
        }

        if (recieveInput)
        {
            // Debug.Log("PostProcessing " + recieveInput + " : " + (activeKeys > 0 ? string.Join(",", schedule.Where(a => a.active).Select(x => "(" + x.keyCode + ":" + (Time.time - x.startTime))) : "") + ")");
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKeyDown(keys[i]))
                {
                    schedule[i].active = !schedule[i].active;
                    if (schedule[i].active)
                    {
                        schedule[i].startTime = Time.time;
                        activeKeys++;
                    }
                    else
                        activeKeys--;
                }
            }
        }

        if (!volume)
            volume = GetComponent<Volume>();
        if (volume.profile.TryGet(out ColorAdjustments colorAdjust))
        {
            if (colorAdjust.hueShift.value >= 180)
                colorAdjust.hueShift.value = -180;
            else
                colorAdjust.hueShift.value += Time.deltaTime * speed;
        }
        if (schedule[0].active && volume.profile.TryGet(out LensDistortion lensDistortion))
        {
            float t = Mathf.Cos((Time.time - schedule[0].startTime) * SPEED + Mathf.PI) * .5f + .5f;
            Debug.Log("0:" + t);
            lensDistortion.intensity.value = t;
            lensDistortion.scale.value = (1 - t) * t * 4f+1;
        }
        if (schedule[1].active && volume.profile.TryGet(out Kaleidoskop kaleidoskop))
        {
            float t = Mathf.Cos((Time.time - schedule[0].startTime) * SPEED + Mathf.PI) * .5f + .5f;
            Debug.Log("1:" + t);
            kaleidoskop.intensity.value = t;
        }
        if (schedule[2].active && volume.profile.TryGet(out ColorAdjustments colorAdjust2))
        {
            float t = Mathf.Cos((Time.time - schedule[0].startTime) * SPEED + Mathf.PI) * .5f + .5f;
            var c = Color.HSVToRGB(t , 1, 1);
            Debug.Log("2:" + t+" - "+c);
            colorAdjust2.colorFilter.value = c;
        }
    }
}
