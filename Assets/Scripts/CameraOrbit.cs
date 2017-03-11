using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour {

    public float speed;
	
	// Update is called once per frame
	void Update () {
        Orbit();
    }

    /// <summary>
    /// Moves the camera constantly right. 
    /// Orbit effect is achieved by applying LookAt method.
    /// </summary>
    private void Orbit() {
        transform.LookAt(Vector3.zero);
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
}
