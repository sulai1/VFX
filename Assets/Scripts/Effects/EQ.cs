using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class EQ : MonoBehaviour
{
    public float height;
    public float distance;

    private void OnEnable()
    {
        Debug.Log("Children " + transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).position = transform.position + i * Vector3.right * distance;
        }
    }

    public void Move(int index, float value)
    {
        Debug.Log($"move {transform.childCount} {index} {value}");
        if (transform.childCount > index)
        {
            Transform childTransform = transform.GetChild(index);
            var v = childTransform.position;
            VisualEffect vfx = childTransform.GetComponent<VisualEffect>();
            if (vfx == null)
            {
                v.y = transform.position.y + value * height;
                childTransform.position = v;
            }
            else
            {
                vfx.SetFloat("height", value * height);
            }
        }
    }
}
