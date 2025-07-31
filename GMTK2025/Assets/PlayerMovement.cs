using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TimeThings;

public class PlayerMovement : MonoBehaviour {

    public GhostMovement ghostPrefab;

    public CharacterController controller;
    public Transform cameraTransform;

    public float maxPitch = 90.0f;

    public float mouseSensitivity = 5;

    public float movementSpeed = 4;
    public float jumpForce = 5;

    public float loopLength = 60 * 4;
    public float gravity = 9.81f;
    public float yVelocity = 0;

    private float pitch;
    private float yaw;

    private bool paused = true;

    private bool grounded = true;

    private float startTime;

    private int numGhosts;

    private List<MovementKeyframe> record;

    private List<GhostMovement> ghosts;

    Quaternion baseRotation;
    void Start() {
        record = new List<MovementKeyframe>();
        ghosts = new List<GhostMovement>();

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
        if(Input.GetKeyUp(KeyCode.Escape)) {
            paused = !paused;
            if(paused) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
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
            Debug.Log(yVelocity);
            if(Input.GetKeyDown(KeyCode.Space) && controller.isGrounded) {
                yVelocity = jumpForce;
            }
        }

        if(record.Count == 0 || Time.fixedTime > record[record.Count - 1].time + 0.1) {
            record.Add(new MovementKeyframe(Time.fixedTime - startTime, 0, transform.position, pitch, yaw));
        }

        if((Time.fixedTime - startTime) / loopLength > numGhosts + 1) {
            GhostMovement ghost = (GhostMovement) Instantiate(ghostPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            ghost.record = new List<MovementKeyframe>(record);
            ghost.StartReplay();
            ghosts.Add(ghost);
            for(int i = 0; i < numGhosts; i++) {
                ghosts[i].record = new List<MovementKeyframe>(record);
            }
            numGhosts++;
        }

        if(Input.GetMouseButtonDown(0)) {
        
            Debug.Log("CUM");
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
