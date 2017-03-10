using System;
using UnityEngine;

public class FPViewController : MonoBehaviour {

    public float moveSpeed = 8.0f;   // Speed when walking forward
    public float jumpForce = 30f;
    public float currentMoveSpeed = 8f;

    public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )

    public Camera cam;
    private Rigidbody _rigidbody;
    private CapsuleCollider capsuleCollider;
    private Vector3 groundContactNormal;
    private bool hasJumped;
    private bool isPreviouslyGrounded;
    private bool isJumping;
    private bool isGrounded;

    public float mouseLookMinimumX = -90f;
    public float mouseLookMaximumX = 90f;

    private Quaternion characterRotation;
    private Quaternion cameraRotation;

    private void Start() {
        _rigidbody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        characterRotation = transform.localRotation;
        cameraRotation = cam.transform.localRotation;
    }

    private void Update() {
        if (Input.GetButtonDown("Jump") && !hasJumped) {
            hasJumped = true;
        }

        float yRot = Input.GetAxis("Mouse X");
        float xRot = Input.GetAxis("Mouse Y");

        characterRotation *= Quaternion.Euler(0f, yRot, 0f);
        cameraRotation *= Quaternion.Euler(-xRot, 0f, 0f);

        cameraRotation = ClampRotationAroundXAxis(cameraRotation);
        transform.localRotation = characterRotation;
        cam.transform.localRotation = cameraRotation;
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q) {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, mouseLookMinimumX, mouseLookMaximumX);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    private void FixedUpdate() {
        GroundCheck();
        Vector2 input = ReadInput();

        if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && isGrounded) {
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 moveDirection = cam.transform.forward * input.y + cam.transform.right * input.x;
            moveDirection = Vector3.ProjectOnPlane(moveDirection, groundContactNormal).normalized;

            if (input != Vector2.zero) {
                currentMoveSpeed = moveSpeed;
            }

            moveDirection.x = moveDirection.x * currentMoveSpeed;
            moveDirection.z = moveDirection.z * currentMoveSpeed;
            moveDirection.y = moveDirection.y * currentMoveSpeed;
            if (_rigidbody.velocity.sqrMagnitude <
                (currentMoveSpeed * currentMoveSpeed)) {
                _rigidbody.AddForce(moveDirection, ForceMode.Impulse);
            }
        }

        if (isGrounded) {
            _rigidbody.drag = 5f;
            if (hasJumped) {
                _rigidbody.drag = 0f;
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
                _rigidbody.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);
                isJumping = true;
            }
        }
        else {
            _rigidbody.drag = 0f;
        }
        hasJumped = false;
    }



    private Vector2 ReadInput() {
        Vector2 input = new Vector2 {
            x = Input.GetAxis("Horizontal"),
            y = Input.GetAxis("Vertical")
        };
        return input;
    }

    /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
    private void GroundCheck() {
        isPreviouslyGrounded = isGrounded;
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capsuleCollider.radius * 1.0f, Vector3.down, out hitInfo,
                               ((capsuleCollider.height / 2f) - capsuleCollider.radius) + groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
            isGrounded = true;
            groundContactNormal = hitInfo.normal;
        }
        else {
            isGrounded = false;
            groundContactNormal = Vector3.up;
        }
        if (!isPreviouslyGrounded && isGrounded && isJumping) {
            isJumping = false;
        }
    }
}
