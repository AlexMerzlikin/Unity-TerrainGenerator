using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewChanger : MonoBehaviour {

    public GameObject topCamera;
    public GameObject fpsCamera;
    public GameObject airplaneCamera;

    public List<GameObject> modeList = new List<GameObject>();

	// Use this for initialization
	void Start () {
        modeList.Add(topCamera);
        modeList.Add(fpsCamera);
        modeList.Add(airplaneCamera);
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void SetView(GameObject newView) {
        modeList.ForEach(view => view.SetActive(false));
        newView.SetActive(true);
    }
}
