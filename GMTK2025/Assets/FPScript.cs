using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FPScript : MonoBehaviour
{
    [SerializeField] Animator armAnimator;
    [SerializeField] Animator gunAnimator;
    [SerializeField] Transform secondPinTransform;
    Vector3 compassTarget = Vector3.zero;
    
    private bool watching = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setCompassTarget(Vector3 val) {
        compassTarget = val;
    } 
    // Update is called once per frame
    void Update()
    {   

        Vector3 worldDir = compassTarget - transform.position;
        worldDir = Vector3.ProjectOnPlane(worldDir, Vector3.up).normalized;

        Vector3 playerFwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        float relAngle = Vector3.SignedAngle(playerFwd, worldDir, Vector3.up);
        secondPinTransform.localRotation = Quaternion.Euler(relAngle + 90, 0, 0);

        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (!watching) {
                armAnimator.Play("Look");
                watching = true;
            } else {
                armAnimator.Play("Unlook");
                watching = false;
            }
        }

        if (Input.GetMouseButtonDown(0)) {
            if (!watching) {
                armAnimator.Play("Shoot", 0, 0);
            }
        }
    }
}
