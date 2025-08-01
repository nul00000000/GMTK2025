using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TimeThings;
using System;
using Unity.VisualScripting;

public class NPCMovement : MonoBehaviour
{
    public Rigidbody player;
    public Collider body;
    public Transform cameraTransform;

    public GameObject deathParticles;

    public float moveDistance = 2.5f;

    private float pitch;

    private bool started = false;

    [System.NonSerialized]
    public float startTime;

    public List<MovementKeyframe> record;
    private Animator animator;

    private int seed = 1234567;
    private static bool seedSet = false;
    public bool isTimeKeeper = false;
    public bool dead = false;
    
    public void InitRandomMovement(float x, float y, float z) {
        float rotY = UnityEngine.Random.value * 360;
        record.Add(new MovementKeyframe(0, new Vector3(x, y, z), 0, rotY));
        for(int i = 0; i < 120; i++) {
            float time = i * 2;
            if(UnityEngine.Random.value < 0.5) { //new position
                float tx = Mathf.Sin(rotY * 3.14159f / 180);
                float tz = Mathf.Cos(rotY * 3.14159f / 180);

                RaycastHit hit;
                // Debug.Log(body.bounds.center.ToString() + " " + body.bounds.extents.ToString() + " " + new Vector3(tx, 0, tz).ToString() + " " + moveDistance.ToString());
                bool hitDetect = Physics.BoxCast(new Vector3(x, 1, z), body.bounds.extents, new Vector3(tx, 0, tz), out hit, Quaternion.identity, moveDistance);

                if(hitDetect) {
                    //do a rotation anyway
                    rotY += UnityEngine.Random.value * 360 - 180;
                } else {
                    x += tx * moveDistance;
                    z += tz * moveDistance;
                }
                // rotY += UnityEngine.Random.value * 360 - 180;
            } else {
                rotY += UnityEngine.Random.value * 360 - 180;
            }

            record.Add(new MovementKeyframe(time, new Vector3(x, y, z), 0, rotY));
        }
    }

    public void InitKeyframes(GameObject buildings) {
        // if(!seedSet) {
        //     UnityEngine.Random.InitState(seed);
        //     seedSet = true;
        // }

        record = new List<MovementKeyframe>();
        
        if (buildings == null) {
            InitRandomMovement(transform.position.x, transform.position.y, transform.position.z);
        }

        float x = UnityEngine.Random.value * 100 - 50;
        float y = 0;
        float z = UnityEngine.Random.value * 100 - 50;

        bool inBuilding = true;
        Collider[] cols = buildings.GetComponentsInChildren<Collider>();

        while(inBuilding) {
            inBuilding = false;
            for(int i = 0; i < cols.Length; i++) {
                if(cols[i].bounds.Contains(new Vector3(x, 1, z))) {
                    inBuilding = true;
                    x = UnityEngine.Random.value * 100 - 50;
                    y = 0;
                    z = UnityEngine.Random.value * 100 - 50;
                    break;
                }
            }
        }

        InitRandomMovement(x, y, z);
    }

    public void Start() {
        animator = GetComponentInChildren<Animator>();
        StartReplay();
        // InitKeyframes(null);
    }

    public void StartReplay() {
        started = true;
        startTime = Time.fixedTime;
    }

    public void DoKill() {
        if (isTimeKeeper) {
            GameObject go = Instantiate(deathParticles, transform.position, Quaternion.identity);
            Destroy(go, 2.0f);
            gameObject.SetActive(false);
        } else {
            animator.Play("Death");
        }
        dead = true;
    }

    // Update is called once per frame
    void Update() {
        if(started && !dead) {
            float currentTime = Time.fixedTime - startTime;
            int first = record.Count - 2;
            int second = record.Count - 1;
            for(int i = 1; i < record.Count; i++) {
                if(record[i].time > currentTime) {
                    first = i - 1;
                    second = i;
                    break;
                }
            }

            Vector3 pos = Vector3.Lerp(record[first].pos, record[second].pos, (currentTime - record[first].time) / (record[second].time - record[first].time));

            // Yaw rotates the body (left/right)
            Vector3 rot = player.transform.localEulerAngles;
            rot.y = Mathf.Lerp(record[first].rotY, record[second].rotY, (currentTime - record[first].time) / (record[second].time - record[first].time));
            player.transform.localEulerAngles = rot;

            if (cameraTransform != null) {
                Vector3 camEuler = cameraTransform.localEulerAngles;
                camEuler.x = Mathf.Lerp(record[first].rotX, record[second].rotX, (currentTime - record[first].time) / (record[second].time - record[first].time));
                camEuler.y = 0f;
                camEuler.z = 0f;
                cameraTransform.localEulerAngles = camEuler;
            }


            Vector3 posDiff = record[first].pos - record[second].pos;

            if (Math.Abs(posDiff.magnitude) > .01) {
                animator.Play("Walk");
            } else {
                animator.Play("Idle");
            }
            player.MovePosition(pos);
        }
    }
}
