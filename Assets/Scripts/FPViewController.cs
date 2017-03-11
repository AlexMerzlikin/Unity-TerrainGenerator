using System;
using UnityEngine;

public class FPViewController : MonoBehaviour {

    public float moveSpeed = 8.0f; 
    public float jumpForce = 30f;
    public float currentTargetSpeed = 8f;

    public float groundCheckDistance = 0.01f;

    public Camera cam;
    private Rigidbody _rigidbody;
    private CapsuleCollider capsuleCollider;
    private Vector3 groundContactNormal;
    private bool hasJumped;
    private bool isPreviouslyGrounded;
    private bool isJumping;
    private bool isGrounded;

    public float minimumX = -90f;
    public float maximumX = 90f;

    private Quaternion characterRotation;
    private Quaternion cameraRotation;

    /// <summary>
    /// Cache rigidbody and capsule collider of the player.
    /// Set initial character and camera rotation.
    /// </summary>
    private void Start() {
        _rigidbody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        characterRotation = transform.localRotation;
        cameraRotation = cam.transform.localRotation;
    }

    /// <summary>
    /// Get jump input if player is not in the air already.
    /// Read mouse input and apply rotation to character and camera.
    /// </summary>
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

    /// <summary>
    /// Restrics mouse look at minumumX and maximumX to simulate real ability of a person to look along X axis.
    /// </summary>
    /// <param name="currentRotation">Current camera rotation</param>
    /// <returns></returns>
    Quaternion ClampRotationAroundXAxis(Quaternion currentRotation) {
        currentRotation.x /= currentRotation.w;
        currentRotation.y /= currentRotation.w;
        currentRotation.z /= currentRotation.w;
        currentRotation.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(currentRotation.x);
        angleX = Mathf.Clamp(angleX, minimumX, maximumX);
        currentRotation.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return currentRotation;
    }

    /// <summary>
    /// Check if the player is grounded.
    /// Apply WASD input. If the player velocity is smaller than target speed, add force to player's rigidbody.
    /// If player is grounded, apply drag to stop the player when WASD input is zero. 
    /// If the player has jumped or not grounded drag is set to 0, so it can jump or slide down the slopes (not sticking to the mountains)
    /// </summary>
    private void FixedUpdate() {
        GroundCheck();
        Vector2 input = GetInput();
        if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && isGrounded) {
            Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, groundContactNormal).normalized;

            if (input != Vector2.zero) {
                currentTargetSpeed = moveSpeed;
            }

            desiredMove.x = desiredMove.x * currentTargetSpeed;
            desiredMove.z = desiredMove.z * currentTargetSpeed;
            desiredMove.y = desiredMove.y * currentTargetSpeed;
            if (_rigidbody.velocity.sqrMagnitude <
                (currentTargetSpeed * currentTargetSpeed)) {
                _rigidbody.AddForce(desiredMove, ForceMode.Impulse);
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

    /// <summary>
    /// Get WASD input
    /// </summary>
    /// <returns></returns>
    private Vector2 GetInput() {

        Vector2 input = new Vector2 {
            x = Input.GetAxis("Horizontal"),
            y = Input.GetAxis("Vertical")
        };
        return input;
    }


    /// <summary>
    /// Since sphere collider is used for character collider cast sphere down the player to check if they grounded or in the air (e.g. jumped)
    /// </summary>
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
