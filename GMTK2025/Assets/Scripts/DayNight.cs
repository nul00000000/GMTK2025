using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DayNight : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Transform directionalLightTransform;
    [SerializeField] float loopSeconds;

    private float timeElapsed = 0;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {   
        float coef = timeElapsed / loopSeconds;
        float angle = 360f * coef - (360 * (int) coef);
        directionalLightTransform.localRotation = Quaternion.Euler(angle + 50, 0, 0);
        timeElapsed += Time.deltaTime;
    }
}
