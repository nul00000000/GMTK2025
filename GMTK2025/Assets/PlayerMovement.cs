using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TimeThings;
using Unity.VisualScripting;
using System;

public class PlayerMovement : MonoBehaviour {

    [SerializeField] GameObject pauseMenu;
    public CharacterController controller;
    public Transform cameraTransform;

    public float maxPitch = 90.0f;

    public float mouseSensitivity = 5;

    public float movementSpeed = 4;
    public float jumpForce = 5;

    public float gravity = 9.81f;
    public float yVelocity = 0;

    private float pitch;
    private float yaw;

    private bool paused = true;

    private float startTime;

    [System.NonSerialized]
    public List<MovementKeyframe> record;
    public List<ActionKeyframe> actionRecord;

    Quaternion baseRotation;

    public void SetToTime(float timeFromStart, float newStartTime) {
        int first = record.Count - 2;
        int second = record.Count - 1;
        for(int i = 1; i < record.Count; i++) {
            if(record[i].time > timeFromStart) {
                first = i - 1;
                second = i;
                break;
            }
        }

        Vector3 pos = Vector3.Lerp(record[first].pos, record[second].pos, (timeFromStart - record[first].time) / (record[second].time - record[first].time));

        controller.enabled = false;
        controller.transform.position = pos;
        controller.enabled = true;

        // Yaw rotates the body (left/right)
        Vector3 rot = transform.localEulerAngles;
        rot.y = Mathf.Lerp(record[first].rotY, record[second].rotY, (timeFromStart - record[first].time) / (record[second].time - record[first].time));
        transform.localEulerAngles = rot;

        if (cameraTransform != null) {
            Vector3 camEuler = cameraTransform.localEulerAngles;
            camEuler.x = Mathf.Lerp(record[first].rotX, record[second].rotX, (timeFromStart - record[first].time) / (record[second].time - record[first].time));
            camEuler.y = 0f;
            camEuler.z = 0f;
            cameraTransform.localEulerAngles = camEuler;
        }

        for(int i = record.Count - 1; i >= 0; i--) {
            if(record[i].time > timeFromStart) {
                record.RemoveAt(i);
            }
        }

        for(int i = actionRecord.Count - 1; i >= 0; i--) {
            if(actionRecord[i].time > timeFromStart) {
                actionRecord.RemoveAt(i);
            }
        }

        startTime = newStartTime;
    }

    void Start() {
        record = new List<MovementKeyframe>();
        actionRecord = new List<ActionKeyframe>();

        startTime = Time.fixedTime;

        baseRotation = cameraTransform.localRotation;
        
    }

    public void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        paused = false;
    }

    // Update is called once per frame
    void Update() {
        mouseSensitivity = EasyGameState.getPrefSensitivity();

        if(Input.GetKeyUp(KeyCode.Escape)) {
            paused = !paused;
            EasyGameState.gamePaused = paused; // lol

            
            if(EasyGameState.gamePaused) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                pauseMenu.SetActive(true);
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                pauseMenu.SetActive(false);
            }
        }
        if(!paused) {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            yaw += mouseX;
            yVelocity -= gravity * Time.deltaTime;
            // Yaw rotates the body (left/right)
            transform.Rotate(Vector3.up, mouseX, Space.Self);

            // Pitch rotates the camera (up/down), clamped
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

            if (cameraTransform != null) {
                cameraTransform.localRotation = baseRotation * Quaternion.Euler(pitch, 0f, 0f);
                // Vector3 camEuler = cameraTransform.localEulerAngles;
                // camEuler.x = pitch;
                // camEuler.y = 0f;
                // camEuler.z = 0f;
                // cameraTransform.localEulerAngles = camEuler;
            }

            //move
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");

            Vector3 r = cameraTransform.right;
            Vector3 f = new Vector3(-r.z, 0, r.x);

            Vector3 movement = f * inputZ * movementSpeed + r * inputX * movementSpeed;
            movement.y = yVelocity;
            controller.Move(movement * Time.deltaTime);
            if(Input.GetKeyDown(KeyCode.Space) && controller.isGrounded) {
                yVelocity = jumpForce;
            }
        }

        if(record.Count == 0 || Time.fixedTime - startTime > record[record.Count - 1].time + 0.1) {
            record.Add(new MovementKeyframe(Time.fixedTime - startTime, transform.position, pitch, yaw));
        }

        if(Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            bool wasHit = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 10000);
            if(wasHit) {
                if(hit.collider.transform.root.gameObject.tag == "Wanderer") {
                    hit.collider.transform.root.gameObject.GetComponent<NPCMovement>().DoKill();
                    actionRecord.Add(new ActionKeyframe(Time.fixedTime - startTime, 0, hit.collider.transform.root.gameObject));
                }
            }
        }
    }

    // public void OnColliderEnter(Collider other) {
    //     Debug.Log("killing myself");
    //     if (other.gameObject.tag == "Ground") {
    //         grounded = true;
    //         Debug.Log("Grounded");
    //     } else {
    //         grounded = false;
    //         Debug.Log("Not Grounded!");
    //     }
    // }

}
