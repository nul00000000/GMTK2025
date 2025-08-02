using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScripts : MonoBehaviour
{
    // Start is called before the first frame update
    [System.NonSerialized]
    public Boolean open = false;
    private float lerpStart = -1;
    private Quaternion startRotation = Quaternion.identity; 
    public int openDirection = -1;

    //returns true if door is now open
    public bool Toggle() {
        open = !open;
        lerpStart = Time.time;
        startRotation = transform.localRotation;
        UpdateState();
        return open;
    }

    public void ResetTo(bool toOpen) {
        open = toOpen;
        lerpStart = Time.time - 100;
        if (toOpen) {
            transform.localRotation = Quaternion.Euler(0, 90 * openDirection, 0);
        } else {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
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
            transform.localRotation = Quaternion.Lerp(startRotation, Quaternion.Euler(0, 90 * openDirection, 0), (Time.time - lerpStart) / .5f);
        } else {
            transform.localRotation = Quaternion.Lerp(startRotation, Quaternion.Euler(0, 0, 0), (Time.time - lerpStart) / .5f);
        }
    }
}
