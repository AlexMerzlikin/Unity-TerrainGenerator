using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewChanger : MonoBehaviour {

    public List<GameObject> modeList = new List<GameObject>();

    /// <summary>
    /// Disables every viewm then enables the one passed in the argument
    /// </summary>
    /// <param name="newView">The chosen through UI view</param>
    public void SetView(GameObject newView) {
        modeList.ForEach(view => view.SetActive(false));
        newView.SetActive(true);
    }
}
