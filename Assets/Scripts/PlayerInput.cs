using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ReadOnlyAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var previousGUIState = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = previousGUIState;
    }
}

public class PlayerInput : MonoBehaviour {

    public float PlayerSpeed = 12f;
    public float MouseSensitivity = 500f;
    public float Gravity = 1f;
    public float JumpStrength = 5f;
    public float MaxSelectDistance = 5f;
    public float CastLength = 0;

    public GameObject BlockHighlightPrefab;
    public WorldComponent world;

    private Camera cam;
    private GameObject blockHighlight;
    private Rigidbody rigidBody;
    
    private float pitch = 0;
    [ReadOnly] public Vector2 inputMovement;
    [ReadOnly] public bool jumping = false;
    [ReadOnly] public bool Grounded = false;

    private int terrainLayerMask;

    // Start is called before the first frame update
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
        rigidBody = GetComponent<Rigidbody>();

        terrainLayerMask = LayerMask.GetMask("Terrain");
    }

    // Update is called once per frame
    void Update() {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        jumping = Input.GetKey(KeyCode.Space);

        Vector3 move = transform.right * x + transform.forward * z;
        move.Normalize();
        inputMovement = new Vector2(move.x, move.z) * PlayerSpeed;

        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90, 90);

        transform.Rotate(Vector3.up, mouseX, Space.World);
        cam.transform.localRotation = Quaternion.AngleAxis(pitch, Vector3.right);

        UpdateBlockHighlight();
    }

    private void FixedUpdate() {

        float verticalSpeed = rigidBody.velocity.y;

        if (CharacterGrounded()) {
            Grounded = true;

            if (jumping && verticalSpeed <= 0) {
                verticalSpeed = JumpStrength;
            }
        } else {
            Grounded = false;
        }
        
        rigidBody.velocity = new Vector3(inputMovement.x, verticalSpeed, inputMovement.y);
    }

    private bool CharacterGrounded() {
        return Physics.CheckSphere(transform.position - new Vector3(0, .6f, 0), .4f + CastLength, terrainLayerMask);
    }

    private void UpdateBlockHighlight() {
        RaycastHit raycastResult;
        bool hit = Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out raycastResult, MaxSelectDistance);

        if (hit) {
            Vector3 blockCoord = raycastResult.point - raycastResult.normal / 2;

            blockCoord.x = Mathf.Floor(blockCoord.x);
            blockCoord.y = Mathf.Floor(blockCoord.y);
            blockCoord.z = Mathf.Floor(blockCoord.z);

            if (world.world.GetBlock(new Vector3Int((int)blockCoord.x, (int)blockCoord.y, (int)blockCoord.z))?.type == Block.Type.Solid) { 
                if (blockHighlight == null) {
                    blockHighlight = Instantiate(BlockHighlightPrefab);
                }
                blockHighlight.transform.position = blockCoord;
            }
        } else {
            if (blockHighlight != null) {
                Destroy(blockHighlight);
                blockHighlight = null;
            }
        }
    }
}
