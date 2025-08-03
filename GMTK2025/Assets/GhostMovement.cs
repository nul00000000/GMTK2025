using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TimeThings;
using Unity.VisualScripting;

public class GhostMovement : MonoBehaviour {

    public Rigidbody ghostBody;
    public Transform cameraTransform;
    [SerializeField] Animator animator;
    [SerializeField] Transform gunTip;
    [SerializeField] TrailRenderer trail;
    [SerializeField] AudioSource shrillAudio;
    [System.NonSerialized]
    public Transform player;

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

    private bool lastPlayerSeen = false;
    private float playerSeenSince = 0;

    public void StartReplay() {
        started = true;
        startTime = Time.fixedTime;
        lastActionTime = 0;
    }

    public void SetLastActionTime(float time) {
        this.lastActionTime = time;
    }

    // Update is called once per frame

    private void Shoot() {
        
    }


    public void Shoot(Vector3 hit) {
        
        TrailRenderer renderer = Instantiate(trail, gunTip.position, Quaternion.identity);
        StartCoroutine(SpawnTrail(renderer, hit));
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hit) {
        float time = 0;
        Vector3 start = trail.transform.position;

        while (time < 1) {
            trail.transform.position = Vector3.Lerp(start, hit, time);

            time += Time.deltaTime * 10;
            yield return null;
        }

        Destroy(trail.gameObject);
    }

    void Update() {
        if(started && !EasyGameState.gameLost) {
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

            //test if path is clear
            // bool didHit = Physics.CapsuleCast(pos + new Vector3(0, 0.5f, 0), pos + new Vector3(0, 1.5f, 0), 0.1f, Vector3.Normalize(record[second].pos - record[first].pos), Vector3.Distance(pos, record[second].pos));
            bool didHit = false;

            // Yaw rotates the body (left/right)
            Vector3 rot = ghostBody.transform.localEulerAngles;
            rot.y = Mathf.Lerp(record[first].rotY, record[second].rotY, (currentTime - record[first].time) / (record[second].time - record[first].time));
            ghostBody.transform.localEulerAngles = rot;

            ghostBody.MovePosition(pos);
            Vector3 posDiff = record[first].pos - record[second].pos;
            if (Mathf.Abs(posDiff.magnitude) > .01) {
                animator.Play("Run");
            } else {
                animator.Play("Idle");
            }

            //actions
            int actionIndex = -1;
            for(int i = 0; i < actionRecord.Count; i++) {
                if(actionRecord[i].time > lastActionTime && currentTime > actionRecord[i].time) {
                    actionIndex = i;
                    lastActionTime = actionRecord[i].time;
                    break;
                }
            }

            Vector3 lostCameraPosition = transform.position + 1.2f * -transform.forward;
            lostCameraPosition.y = transform.position.y + 2.2f;

            if(actionIndex != -1) {
                ActionKeyframe action = actionRecord[actionIndex];
                if(action.type == 0) {
                    if(action.interacted.tag == "Wanderer") {
                        NPCMovement npc = action.interacted.GetComponent<NPCMovement>();
                        if(npc.dead) {
                            Debug.Log("NPC was already dead on loop " + ghostNum);
                            // buildings.ResetToLoop(ghostNum);
                            EasyGameState.DoGameLost(ghostNum, lostCameraPosition);
                        } else {
                            npc.DoKill();
                            Shoot(npc.transform.position + new Vector3(0, 1.5f, 0));
                        }
                    }
                } else if(action.type == 1) {
                    if(action.interacted.tag == "Door") {
                        DoorScripts door = action.interacted.GetComponent<DoorScripts>();
                        if(door.open) { //door was already open
                            Debug.Log("Door was already open on loop " + ghostNum);
                            // buildings.ResetToLoop(ghostNum);
                            EasyGameState.DoGameLost(ghostNum, lostCameraPosition);
                        } else {
                            door.Toggle();
                        }
                    }
                } else if(action.type == 2) {
                    if(action.interacted.tag == "Door") {
                        DoorScripts door = action.interacted.GetComponent<DoorScripts>();
                        if(!door.open) { //door was already open
                            Debug.Log("Door was already closed on loop " + ghostNum);
                            EasyGameState.DoGameLost(ghostNum, lostCameraPosition);
            
                            // buildings.ResetToLoop(ghostNum);
                        } else {
                            door.Toggle();
                        }
                    }
                }
            }
            if(didHit) {
                Debug.Log("witawawy clipping rn");
                // buildings.ResetToLoop(ghostNum);
                EasyGameState.DoGameLost(ghostNum, lostCameraPosition);
            }
            //check if player is visible
            bool seen = false;
            if(Vector3.Dot(Vector3.Normalize(player.position - pos), Quaternion.Euler(new Vector3(Mathf.Lerp(record[first].rotX, record[second].rotX, (currentTime - record[first].time) / (record[second].time - record[first].time)), 
                    Mathf.Lerp(record[first].rotY, record[second].rotY, (currentTime - record[first].time) / (record[second].time - record[first].time)), 0)) * new Vector3(0, 0, 1)) > 0.4) {
                RaycastHit playerSee;
                bool canSeePlayer = Physics.Raycast(pos, Vector3.Normalize(player.position - pos), out playerSee);
                float dist = Vector3.Distance(player.position, pos);
                if(playerSee.distance > dist) {
                    // buildings.ResetToLoop(ghostNum);
                    seen = true;
                }
            }
            if(seen && !lastPlayerSeen) {
                playerSeenSince = Time.fixedTime;
                Debug.Log("Player entered vision of ghost " + ghostNum);
                shrillAudio.volume = 1.0f * EasyGameState.getPrefVolume();
                shrillAudio.Play();
                
            } else if(seen && Time.fixedTime > playerSeenSince + 1.5) {
                EasyGameState.DoGameLost(ghostNum, lostCameraPosition);
            }
            player.gameObject.GetComponent<PlayerMovement>().SetIndicatorEnabled(seen);
            player.gameObject.GetComponent<PlayerMovement>().SetIndicatorPointTowards(transform.position);
            lastPlayerSeen = seen;
        }
    }

    void LateUpdate() {
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
        if (cameraTransform != null) {
            Vector3 camEuler = cameraTransform.localEulerAngles;
            camEuler.x = (-Mathf.Lerp(record[first].rotX, record[second].rotX, (currentTime - record[first].time) / (record[second].time - record[first].time))) + 10;
            camEuler.y = 0f;
            camEuler.z = 0f;
            cameraTransform.localEulerAngles = camEuler;
        }
    }
}
