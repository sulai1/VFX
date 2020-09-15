using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    private GameObject Canvas=> transform.GetChild(0).gameObject;
    public Text timeText;

    // Start is called before the first frame update
    void Start()
    {
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
        get { return Canvas.activeSelf; }
        set { Canvas.SetActive(value); }
    }

}
