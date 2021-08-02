using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _FirstPersonController : MonoBehaviour
{
    public Camera cam;

    [Header("Camera Movement")]
    public bool mouseLookEnabled = true;
    public float xSensitivity = 1f;
    public float ySensitivity = 1f;
    float vertical = 0f;
    public bool invertVertical = false;

    [Header("Movement")]
    public float walkingForce = 3f;
    public float runningForce = 4f;
    public float counterMovement = 0.95f; //dampen the movement a bit so the player doesn't fly off

    public bool grounded = false;
    public float jumpForce = 10f;

    Rigidbody rig;
    // Start is called before the first frame update
    void Awake()
    {
        rig = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0, System.Convert.ToInt32(mouseLookEnabled) * ySensitivity * Input.GetAxis("Mouse X")));
        vertical = Mathf.Clamp(vertical + (System.Convert.ToInt32(mouseLookEnabled) * xSensitivity * -Input.GetAxis("Mouse Y") * (Convert.ToInt32(!invertVertical) * 2 - 1)), -90, 90);
        cam.transform.localRotation = Quaternion.Euler(vertical, 0, 0);

        RaycastHit hit; //check if player is touching ground
        if (Physics.SphereCast(transform.position, 0.1f, Vector3.down, out hit, 1))
            grounded = true;
        else
            grounded = false;

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Acceleration);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float forceToUse = Input.GetKey(KeyCode.LeftShift) ? runningForce : walkingForce;
        rig.AddForce(transform.TransformDirection(Vector3.right * Input.GetAxisRaw("Horizontal") + Vector3.forward * Input.GetAxisRaw("Vertical")).normalized * forceToUse, ForceMode.Acceleration);
        rig.velocity = new Vector3(rig.velocity.x * counterMovement, rig.velocity.y, rig.velocity.z * counterMovement);
    }
}
