using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToMove : MonoBehaviour {

    [SerializeField]
    private bool isEnabled = true;
    private bool isStarted = false;
    private Vector3 targetPosition;
    private Vector3 clickPosition;

    private float timeForClickToMove = 0f;
    private float speed = 20f;
    private float playerHeight = 0.8f;
    private float cameraHeight = 1.6f;
    private Transform fpViewCamera;

    /// <summary>
    /// Cache camera for the first person view
    /// </summary>
    void Start() {
        fpViewCamera = GameObject.Find("MainCamera").transform;
    }

    /// <summary>
    /// Detect mouse click and check conditions for click to move action.
    /// If the click to move action is initiated, move the player to target position over time.
    /// </summary>
    void FixedUpdate () {
        if (Input.GetMouseButtonDown(0) && isEnabled && GameObject.Find("FPView") && !isStarted) {
            InitiateClickToMove();            
        }

        if (isStarted) {
            MoveToTarget();
        }
    }

    /// <summary>
    /// Is called when player clicks terrain && click to move is enabled && FPView is active && another click to move transition is not active
    /// Sets current player position and click position to initiate translation between these two positions.
    /// </summary>
    private void InitiateClickToMove() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            isStarted = true;
            targetPosition = new Vector3(hit.point.x, hit.point.y + playerHeight, hit.point.z);
            clickPosition = transform.position;
        }
    }

    /// <summary>
    /// Moves the player towards click position via Vector3.MoveTowards method.
    /// Since MoveTowards doesn't take into account terrain and just lineary changes player position
    /// The distance to the ground is checked and position.y is decreased by this amount to simulate 
    /// actual movement across the terrain. When the target position is reached, the bool variable is set to false and timer to 0. 
    /// </summary>
    private void MoveToTarget() {
        RaycastHit groundHit;
        var distanceToGround = 0f;
        if (Physics.Raycast(transform.position, -Vector3.up, out groundHit)) {
            distanceToGround = groundHit.distance - playerHeight;
        }

        timeForClickToMove += Time.deltaTime;
        transform.position = Vector3.MoveTowards(clickPosition, targetPosition, timeForClickToMove * speed);

        fpViewCamera.position = new Vector3(transform.position.x,
            transform.position.y + cameraHeight - distanceToGround,
            transform.position.z);

        if (transform.position.Equals(targetPosition)) {
            isStarted = false;
            timeForClickToMove = 0;
        }
    }
}
