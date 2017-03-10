using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToMove : MonoBehaviour {

    [SerializeField]
    private bool isEnabled = true;
    private bool isStarted = false;
    private Vector3 targetPosition;
    private Vector3 clickPosition;

    private float timeForClickToMove = 0.0f;
    private float speed = 30.0f;
    private float playerHeight = 0.8f;
    private float cameraHeight = 1.6f;
    private Transform fpViewCamera;

    void Start() {
        fpViewCamera = GameObject.Find("MainCamera").transform;
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (Input.GetMouseButtonDown(0) && isEnabled && GameObject.Find("FPView")) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                isStarted = true;
                targetPosition = new Vector3(hit.point.x, hit.point.y + playerHeight, hit.point.z);
                clickPosition = transform.position;
            }
        }

        if (isStarted) {
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
        }

        if (transform.position.Equals(targetPosition)) {
            isStarted = false;
            timeForClickToMove = 0;
        }
    }
}
