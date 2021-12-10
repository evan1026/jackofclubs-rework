using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {

    public float PlayerSpeed = 12f;
    public float MouseSensitivity = 500f;
    public float Gravity = 1f;
    public float JumpStrength = 5f;

    private CharacterController controller;
    private Camera cam;

    private float pitch = 0;
    public float verticalSpeed = 0;
    private Vector2 inputMovement;
    private bool jumping = false;

    // Start is called before the first frame update
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update() {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        jumping = Input.GetKey(KeyCode.Space);

        Vector3 move = transform.right * x + transform.forward * z;
        move.Normalize();
        inputMovement = new Vector2(move.x, move.z);

        controller.Move(move * Time.deltaTime * PlayerSpeed);

        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90, 90);

        transform.Rotate(Vector3.up, mouseX, Space.World);
        cam.transform.localRotation = Quaternion.AngleAxis(pitch, Vector3.right);
    }

    private void FixedUpdate() {
        
        if (CharacterGrounded()) {
            verticalSpeed = 0;
            if (jumping) {
                Debug.Log("Jump");
                verticalSpeed = JumpStrength;
            }
        }
        verticalSpeed -= Gravity;

        Vector3 velocity = new Vector3(inputMovement.x, verticalSpeed, inputMovement.y);
        controller.Move(velocity * Time.fixedDeltaTime);
    }

    private bool CharacterGrounded() {
        // I'm not really sure the reasoning behind subtracting radius / 2, but it makes it work better so eh
        return Physics.SphereCast(new Ray(transform.position, Vector3.down), controller.radius, controller.height / 2 - controller.radius / 2);
    }
}
