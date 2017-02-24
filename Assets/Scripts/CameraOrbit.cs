using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour {

    public float speed;
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(Vector3.zero);
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
}
