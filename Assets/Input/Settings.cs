using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    private Canvas canvas;
    public Text timeText;

    // Start is called before the first frame update
    void Start()
    {
        canvas=gameObject.GetComponent<Canvas>();
        CheckMicrophones();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Enabled = !Enabled;
        }
        timeText.text = Time.time+"";
    }
    public void CheckMicrophones()
    {

        var dropdown = GetComponentInChildren<Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(Microphone.devices.ToList());
    }


    public bool Enabled
    {
        get { return canvas.enabled; }
        set { canvas.enabled=value; }
    }

}
