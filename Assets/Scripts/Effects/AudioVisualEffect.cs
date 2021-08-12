using Assets.AudioViz;
using System;
using System.Reflection;
using UnityEngine;

public abstract class AudioVisualEffect : MonoBehaviour
{
    public KeyCode key;

    protected AudioSource processor;

    public delegate void SetData(AudioSource processor);

    // Start is called before the first frame update
    void Start()
    {
        processor = GameObject.Find("AudioProcessor").GetComponent<AudioSource>();
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(key))
            Enabled = !Enabled;
    }

    public void Init()
    {
        Debug.Log("Init");
        foreach (var field in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            Debug.Log("Fail: " + field.Name);
            var att = Attribute.GetCustomAttribute(field, typeof(Parameter));
            if (att != null)
            {
                Debug.Log(GetType() + ": " + field.Name + " " + att);

                var param = att as Parameter;
                param.fieldInfo = field;
                param.obj = this;
                Settings.Instance.AddParameter(this, param);
            }
        }
    }

    public abstract bool Enabled { get; set; }

}
