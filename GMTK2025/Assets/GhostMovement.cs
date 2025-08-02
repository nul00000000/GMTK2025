using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TimeThings;

public class GhostMovement : MonoBehaviour {

    public Rigidbody player;
    public Transform cameraTransform;

    public float maxPitch = 90.0f;

    public float mouseSensitivity = 5;

    public float movementSpeed = 5;
    public float jumpForce = 20;

    private bool started = false;
    [System.NonSerialized]
    public float startTime;

    [System.NonSerialized]
    public List<MovementKeyframe> record;
    [System.NonSerialized]
    public List<ActionKeyframe> actionRecord;

    [System.NonSerialized]
    public WorldController buildings;
    [System.NonSerialized]
    public int ghostNum;

    private float lastActionTime;

    public void StartReplay() {
        started = true;
        startTime = Time.fixedTime;
        lastActionTime = 0;
    }

    public void SetLastActionTime(float time) {
        this.lastActionTime = time;
    }

    // Update is called once per frame
    void Update() {
        if(started) {
            //movement
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

            //actions
            int actionIndex = -1;
            for(int i = 0; i < actionRecord.Count; i++) {
                if(actionRecord[i].time > lastActionTime && currentTime > actionRecord[i].time) {
                    actionIndex = i;
                    lastActionTime = actionRecord[i].time;
                    break;
                }
            }
            if(actionIndex != -1) {
                ActionKeyframe action = actionRecord[actionIndex];
                if(action.type == 0) {
                    if(action.interacted.tag == "Wanderer") {
                        NPCMovement npc = action.interacted.GetComponent<NPCMovement>();
                        if(npc.dead) {
                            Debug.Log("NPC was already dead on loop " + ghostNum);
                            buildings.ResetToLoop(ghostNum);
                        } else {
                            npc.DoKill();
                        }
                    }
                } else if(action.type == 1) {
                    if(action.interacted.tag == "Door") {
                        DoorScripts door = action.interacted.GetComponent<DoorScripts>();
                        if(door.open) { //door was already open
                            Debug.Log("Door was already open on loop " + ghostNum);
                            buildings.ResetToLoop(ghostNum);
                        } else {
                            door.Toggle();
                        }
                    }
                } else if(action.type == 2) {
                    if(action.interacted.tag == "Door") {
                        DoorScripts door = action.interacted.GetComponent<DoorScripts>();
                        if(!door.open) { //door was already open
                            Debug.Log("Door was already closed on loop " + ghostNum);
                            buildings.ResetToLoop(ghostNum);
                        } else {
                            door.Toggle();
                        }
                    }
                }
            }
        }
    }
}
