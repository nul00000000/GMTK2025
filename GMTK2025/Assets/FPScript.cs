using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class FPScript : MonoBehaviour
{
    [SerializeField] Animator armAnimator;
    [SerializeField] Animator gunAnimator;
    [SerializeField] Transform secondPinTransform;
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] Transform trailPoint;

    public Transform playerTransform;

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

    public void Shoot(RaycastHit hit) {
        armAnimator.Play("Shoot", 0, 0);
        
        TrailRenderer renderer = Instantiate(trailRenderer, trailPoint.position, Quaternion.identity);
        StartCoroutine(SpawnTrail(renderer, hit));
    }
    void Update()
    {   

        Vector3 worldDir = compassTarget - playerTransform.position;
        worldDir = Vector3.ProjectOnPlane(worldDir, Vector3.up).normalized;

        Vector3 playerFwd = Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up).normalized;

        float relAngle = Vector3.SignedAngle(playerFwd, worldDir, Vector3.up);
        secondPinTransform.localEulerAngles = new Vector3(0, -80, -relAngle - 90);

        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (!watching) {
                armAnimator.Play("Look");
                watching = true;
            } else {
                armAnimator.Play("Unlook");
                watching = false;
            }
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit) {
        float time = 0;
        Vector3 start = trail.transform.position;

        while (time < 1) {
            trail.transform.position = Vector3.Lerp(start, hit.point, time);

            time += Time.deltaTime * 10;
            yield return null;
        }

        Destroy(trail.gameObject);
    }
}
