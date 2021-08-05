using Assets.AudioViz;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{

    public Dropdown avfxs;
    public Slider sliderPrefab;
    public Text labelPrefab;

    private static Settings instance;

    private GameObject Canvas => transform.GetChild(0).gameObject;

    private GameObject ObjectCanvas => transform.GetChild(1).gameObject;

    public Text timeText;
    AudioVisualEffect lastObj;

    public static Settings Instance { get { return instance; } }

    Dictionary<AudioVisualEffect, List<Parameter>> parameters = new Dictionary<AudioVisualEffect, List<Parameter>>();

    public void AddParameter(AudioVisualEffect effect, Parameter param)
    {
        if (parameters.ContainsKey(effect))
            parameters[effect].Add(param);
        else
            parameters[effect] = new List<Parameter>() { param };

    }

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        avfxs.ClearOptions();
        CheckMicrophones();
        if (parameters.Count == 0)
            return;

        List<string> options = new List<string>();
        foreach (var obj in parameters)
        {
            options.Add(obj.Key.name);
            foreach (var parameter in obj.Value)
            {
                parameter.label = GameObject.Instantiate(labelPrefab, ObjectCanvas.transform);
                parameter.Slider = GameObject.Instantiate(sliderPrefab, ObjectCanvas.transform);
                parameter.Enabled = false;
            }
        }
        avfxs.AddOptions(options);
        avfxs.onValueChanged.AddListener(Refresh);
        lastObj = parameters.Keys.First();
        Refresh(0);
    }

    private void Refresh(int index)
    {
        var v = parameters.Keys.ToArray()[index];
        foreach (var p in parameters[lastObj])
            p.Enabled = false;
        foreach (var p in parameters[v])
            p.Enabled = true;
        lastObj = v;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Enabled = !Enabled;
        }
        timeText.text = Time.time + "";
        if (avfxs != null && avfxs.value > 0 && avfxs.value < parameters.Keys.Count())
        {
            var v = parameters.Keys.ToArray()[avfxs.value];
            foreach (var parameter in parameters[v])
            {
                Debug.Log(parameter.fieldInfo.Name);
                parameter.Update();
            }
        }
    }

    public void CheckMicrophones()
    {
        try
        {
            var dropdown = GetComponentInChildren<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(Microphone.devices.ToList());
        }
        catch (NullReferenceException e)
        {
            Debug.Log(e + "\nNo Dropdown found!");
        }
    }

    public bool Enabled
    {
        get { return Canvas.activeSelf; }
        set { Canvas.SetActive(value); }
    }

}
