using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TimeThings;
using Unity.VisualScripting;
using System;
using TMPro;

public class PlayerMovement : MonoBehaviour {

    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject startScreen;
    [SerializeField] TMP_Text caughtText;
    [SerializeField] AudioSource shootSound;
    [SerializeField] AudioSource clickSound;

    [SerializeField] FPScript fpsController;
    public WorldController buildings;
    public CharacterController controller;
    public Transform cameraTransform;

    public GameObject indicator;

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
    private bool initialTab = false;

    [System.NonSerialized]
    public List<MovementKeyframe> record;
    [System.NonSerialized]
    public List<ActionKeyframe> actionRecord;

    Quaternion baseRotation;
    public LayerMask doorLayer;

    private bool shouldShowIndicator = false;

    public void playClick() {
        clickSound.volume = 1 * EasyGameState.getPrefVolume();
        clickSound.Play();
    }
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

    public void SetIndicatorEnabled(bool enabled) {
        if(enabled) {
            shouldShowIndicator = true;
        }
    }

    public void SetIndicatorPointTowards(Vector3 pos) {
        Vector3 worldDir = pos - transform.position;
        worldDir = Vector3.ProjectOnPlane(worldDir, Vector3.up).normalized;

        Vector3 playerFwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        float relAngle = Vector3.SignedAngle(playerFwd, worldDir, Vector3.up);
        indicator.transform.localEulerAngles = new Vector3(0, 0, -relAngle + 90);
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

    private IEnumerator gameText(TMP_Text obj) {
        float startTime = Time.time;
        while (Time.time - startTime < 8) {
            obj.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), (Time.time - startTime) / 8);

            yield return null;
        }
    }
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

        if(Input.GetKeyDown(KeyCode.Tab) && !initialTab) {
            initialTab = true;
            startScreen.SetActive(false);
            StartCoroutine(gameText(caughtText));
        }

        if(!paused && !EasyGameState.gameLost) {
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
            record.Add(new MovementKeyframe(Time.fixedTime - startTime, transform.position, pitch, yaw + 90));
        }

        RaycastHit doorHit;

        bool wasHit = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out doorHit, 3, doorLayer);
        if (wasHit) {
            if (Input.GetKeyDown(KeyCode.E)) {
                bool open = doorHit.collider.gameObject.GetComponent<DoorScripts>().Toggle();
                actionRecord.Add(new ActionKeyframe(Time.fixedTime - startTime, open ? 1 : 2, doorHit.collider.gameObject));
            }
        }
// bandaid
        RaycastHit hit;
        bool wasHit2 = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 10000);

        if (Input.GetMouseButtonDown(0) && !fpsController.watching) {
            fpsController.Shoot(hit);
            shootSound.volume = 1 * EasyGameState.getPrefVolume();
            shootSound.Play();

            if(wasHit2 && hit.collider.transform.root.gameObject.tag == "Wanderer" && !hit.collider.transform.root.gameObject.GetComponent<NPCMovement>().dead) {
                hit.collider.transform.root.gameObject.GetComponent<NPCMovement>().DoKill();
                actionRecord.Add(new ActionKeyframe(Time.fixedTime - startTime, 0, hit.collider.transform.root.gameObject));
                if(hit.collider.transform.root.gameObject.GetComponent<NPCMovement>().isTimeKeeper) {
                    buildings.RegisterTimekeeperKill(hit.collider.transform.root.gameObject.GetComponent<NPCMovement>().timekeeperNum);
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

    void LateUpdate() {
        indicator.transform.parent.gameObject.SetActive(shouldShowIndicator);
        shouldShowIndicator = false;
    }

}
