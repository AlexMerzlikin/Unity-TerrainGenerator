using System;
using UnityEngine;

public class AeroplaneController : MonoBehaviour {
    private float maxEnginePower = 40f;     
    private float liftPower = 0.002f;              
    private float airBrake = 3f;        
    private float dragIncreaseFactor = 0.001f;

    private float throttle;
    private bool airBrakes;
    private float moveSpeed;
    private float enginePower;
    private float roll;
    private float pitch;
    private float rollInput;
    private float pitchInput;
    private float yawInput;
    private float throttleInput;

    private float initialDrag;         
    private float initialAngularDrag;  
    private float aeroFactor;
    private Rigidbody _rigidbody;

    private bool isCrashed;

    /// <summary>
    /// Cache rigidbody.
    /// Store original drag settings, these are modified during flight.
    /// </summary>
    private void Start() {
        _rigidbody = GetComponent<Rigidbody>();
        initialDrag = _rigidbody.drag;
        initialAngularDrag = _rigidbody.angularDrag;
    }

    /// <summary>
    /// Read inputs from mouse and WASD buttons and call the Move method every fixed frame.
    /// </summary>
    private void FixedUpdate() {
        // Read input for the pitch, yaw, roll and throttle of the aeroplane.
        this.rollInput = Input.GetAxis("Mouse X");
        this.pitchInput = Input.GetAxis("Mouse Y");
        this.yawInput = Input.GetAxis("Horizontal");
        this.throttleInput = Input.GetAxis("Vertical");

        Move();
    }

    /// <summary>
    /// Check if the plane is not crashed.
    /// Calculate forces and apply them to the plane.
    /// </summary>
    public void Move() {
        if (!isCrashed) {
            ClampInputs();
            CalculateRollAndPitchAngles();
            CalculateForwardSpeed();
            ControlThrottle();
            CalculateDrag();
            ApplyAeroFactor();
            ApplyForce();
            ApplyTorque();
        }
    }

    /// <summary>
    /// Clamp the inputs to -1 to 1 range to forbid weird amounts passed through mouse/keyboard
    /// </summary>
    private void ClampInputs() {
        rollInput = Mathf.Clamp(rollInput, -1, 1);
        pitchInput = Mathf.Clamp(pitchInput, -1, 1);
        yawInput = Mathf.Clamp(yawInput, -1, 1);
        throttleInput = Mathf.Clamp(throttleInput, -1, 1);
    }

    /// <summary>
    /// Calculate the flat forward direction (with no y component).
    /// If the flat forward vector is non-zero (which would only happen if the plane was pointing exactly straight upwards)
    /// calculate current roll angle and current pitch angle and apply the calculations to appropriate properties.
    /// </summary>
    private void CalculateRollAndPitchAngles() {
        var flatForward = transform.forward;
        flatForward.y = 0;
        if (flatForward.sqrMagnitude > 0) {
            flatForward.Normalize();
            var localFlatForward = transform.InverseTransformDirection(flatForward);
            pitch = Mathf.Atan2(localFlatForward.y, localFlatForward.z);
            var flatRight = Vector3.Cross(Vector3.up, flatForward);
            var localFlatRight = transform.InverseTransformDirection(flatRight);
            roll = Mathf.Atan2(localFlatRight.y, localFlatRight.x);
        }
    }

    /// <summary>
    /// Calculate the speed in the planes's forward direction and set the moveSpeed property to that value.
    /// </summary>
    private void CalculateForwardSpeed() {
        var localVelocity = transform.InverseTransformDirection(_rigidbody.velocity);
        moveSpeed = Mathf.Max(0, localVelocity.z);
    }

    /// <summary>
    /// Clamps throttle amount passed in input by the player between 0 and 1.
    /// Multiply it with maxEnginePower to determine the current engine power.
    /// </summary>
    private void ControlThrottle() {
        throttle = Mathf.Clamp01(throttle + throttleInput * Time.deltaTime);
        enginePower = throttle * maxEnginePower;
    }

    /// <summary>
    /// The plane's drag is increses with speed.
    /// If the player uses air brakes, than the plane's drag is changed appropriately to reduce plane's velocity
    /// Forward speed affects angular drag. The higher speed means higher drag. Otherwise the plane would spin to much.
    /// </summary>
    private void CalculateDrag() {
        float extraDrag = _rigidbody.velocity.magnitude * dragIncreaseFactor;
        _rigidbody.drag = (airBrakes ? (initialDrag + extraDrag) * airBrake : initialDrag + extraDrag);
        _rigidbody.angularDrag = initialAngularDrag * moveSpeed;
    }

    /// <summary>
    /// Apply the aerofactor to make the plane move in the direction it is facing with the current move speed,
    /// without it the plane would be affected by gravity so much, that it would fall down every time.
    /// Apply calculated rotation and velocity to the rigidbody.
    /// </summary>
    private void ApplyAeroFactor() {
        if (_rigidbody.velocity.magnitude > 0) {
            aeroFactor = Vector3.Dot(transform.forward, _rigidbody.velocity.normalized);
            var currentVelocity = Vector3.Lerp(_rigidbody.velocity, transform.forward * moveSpeed,
                                           aeroFactor * moveSpeed * Time.deltaTime);
            _rigidbody.velocity = currentVelocity;
            _rigidbody.rotation = Quaternion.Slerp(_rigidbody.rotation,
                                                  Quaternion.LookRotation(_rigidbody.velocity, transform.up),
                                                  Time.deltaTime);
        }
    }

    /// <summary>
    /// Accumulate forces in "forces" variable.
    /// Apply engine power times the forward direction force.
    /// Calculate the current lift direction and normalize it.
    /// Calculate lift power based on aero factor, move speed and previous lift power.
    /// Apply lift power times lift direction.
    /// Add resulting forces vector to the rigidbody. 
    /// </summary>
    private void ApplyForce() {
        var forces = Vector3.zero;
        forces += enginePower * transform.forward;
        var liftDirection = Vector3.Cross(_rigidbody.velocity, transform.right).normalized;
        var liftPower = moveSpeed * moveSpeed * this.liftPower * aeroFactor;
        forces += liftPower * liftDirection;
        _rigidbody.AddForce(forces);
    }

    /// <summary>
    /// Accumulate torque forces in torque variable;
    /// Add pitch, yaw and roll to torque based on input of these values multiplied by according directions (right, up, forward).
    /// The total torque is multiplied by the move speed and aero factor.
    /// </summary>
    private void ApplyTorque() {
        var torque = Vector3.zero;
        torque += pitchInput * transform.right;
        torque += yawInput * transform.up;
        torque += -rollInput * transform.forward;
        _rigidbody.AddTorque(torque * moveSpeed * aeroFactor);
    }

    /// <summary>
    /// Is called when the plane collides with anything. In our case with the terrain.
    /// </summary>
    void OnCollisionEnter() {
        Debug.Log("Crashed");
        isCrashed = true;
    }
}
