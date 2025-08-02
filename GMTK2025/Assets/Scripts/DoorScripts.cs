using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScripts : MonoBehaviour
{
    // Start is called before the first frame update
    private Boolean open = false;
    private float lerpStart = -1;
    private Quaternion startRotation = Quaternion.identity; 
    public int openDirection = -1;

    public void Toggle() {
        open = !open;
        lerpStart = Time.time;
        startRotation = transform.rotation;
        UpdateState();
    }

    private void UpdateState() {
        
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lerpStart > .5) {
            return;
        }

        if (open) {
            transform.rotation = Quaternion.Lerp(startRotation, Quaternion.Euler(0, 90 * openDirection, 0), (Time.time - lerpStart) / .5f);
        } else {
            transform.rotation = Quaternion.Lerp(startRotation, Quaternion.Euler(0, 0, 0), (Time.time - lerpStart) / .5f);
        }
    }
}
