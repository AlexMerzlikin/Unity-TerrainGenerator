using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToMove : MonoBehaviour {

    public bool isEnabled = true;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0) && isEnabled && GameObject.Find("FPSView")) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Vector3 targetPosition;

            if (Physics.Raycast(ray, out hit)) {
                targetPosition = hit.point;
                transform.position = targetPosition;
            }
        }
    }
}
