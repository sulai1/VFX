using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.AudioViz
{

    [AttributeUsage(AttributeTargets.Field)]
    public class Parameter : Attribute
    {
        private readonly float min;
        private readonly float max;

        public FieldInfo fieldInfo;
        public object obj;

        Slider s;
        public Slider Slider { get => s; set { s = value; s.minValue = min; s.maxValue = max; } }

        bool enabled;
        public bool Enabled
        {
            get => enabled;

            internal set
            {
                enabled = value;
                Slider.gameObject.SetActive(enabled);
                label.gameObject.SetActive(enabled);
            }
        }

        public Text label;

        public Parameter() : this(0, 1)
        {
        }
        public Parameter(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public void Update()
        {
            if (Slider != null)
            {
                SetValue(Slider.value);
                Slider.value = GetValue();
                label.text = $"{fieldInfo.Name}({min},{max}) {Slider.value}";
            }
            else
                Debug.LogWarning("No Slider");
        }

        public float GetValue()
        {
            var val = fieldInfo.GetValue(obj);
            float value = 0;
            switch (val)
            {
                case int i:
                    value = i;
                    break;
                case float f:
                    value = f;
                    break;
                case bool b:
                    value = b ? 1 : 0;
                    break;
                default:
                    Debug.Log(val.GetType() + " not suported");
                    break;
            }
            return value;
        }

        public void SetValue(float value)
        {
            var val = fieldInfo.GetValue(obj);
            switch (val)
            {
                case int _:
                    val = Mathf.FloorToInt(value);
                    break;
                case float _:
                    val = value;
                    break;
                case bool _:
                    val = value > 0.5f ? 1 : 0;
                    break;
                default:
                    Debug.Log(val.GetType() + " not suported");
                    break;
            }
            fieldInfo.SetValue(obj, val);
        }
    }
}
