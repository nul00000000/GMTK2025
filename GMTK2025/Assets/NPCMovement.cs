using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TimeThings;

public class NPCMovement : MonoBehaviour
{
    public Rigidbody player;
    public Collider body;
    public Transform cameraTransform;

    public GameObject deathParticles;

    public float moveDistance = 2.5f;

    private float pitch;

    private bool started = false;
    private float startTime;

    public List<MovementKeyframe> record;

    private int seed = 1234567;
    private static bool seedSet = false;

    public void Start() {
        if(!seedSet) {
            Random.InitState(seed);
            seedSet = true;
        }
        record = new List<MovementKeyframe>();
        float x = Random.value * 20 - 10;
        float y = 0;
        float z = Random.value * 20 - 10;
        float rotY = Random.value * 360;
        record.Add(new MovementKeyframe(0, 0, new Vector3(x, y, z), 0, rotY));
        for(int i = 0; i < 120; i++) {
            float time = i * 2;
            if(Random.value < 0.5) { //new position
                float tx = Mathf.Sin(rotY * 3.14159f / 180);
                float tz = Mathf.Cos(rotY * 3.14159f / 180);

                RaycastHit hit;
                // Debug.Log(body.bounds.center.ToString() + " " + body.bounds.extents.ToString() + " " + new Vector3(tx, 0, tz).ToString() + " " + moveDistance.ToString());
                bool hitDetect = Physics.BoxCast(new Vector3(x, 1, z), body.bounds.extents, new Vector3(tx, 0, tz), out hit, Quaternion.identity, moveDistance);

                if(hitDetect) {
                    //do a rotation anyway
                    rotY += Random.value * 360 - 180;
                } else {
                    x += tx * moveDistance;
                    z += tz * moveDistance;
                }
                // rotY += Random.value * 360 - 180;
            } else {
                rotY += Random.value * 360 - 180;
            }

            record.Add(new MovementKeyframe(time, 0, new Vector3(x, y, z), 0, rotY));
        }
        StartReplay();
    }

    public void StartReplay() {
        started = true;
        startTime = Time.fixedTime;
    }

    public void DoKill() {
        GameObject go = Instantiate(deathParticles, transform.position, Quaternion.identity);
        Destroy(go, 2.0f);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update() {
        if(started) {
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

            player.MovePosition(pos);
        }
    }
}
