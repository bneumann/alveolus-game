using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugText : MonoBehaviour {

    // Use this for initialization
    TextMesh text;

    void Start () {
        var parent = transform.parent;

        var parentRenderer = parent.GetComponent<Renderer>();
        var renderer = GetComponent<Renderer>();
        renderer.sortingLayerID = parentRenderer.sortingLayerID;
        renderer.sortingOrder = parentRenderer.sortingOrder;

        var spriteTransform = parent.transform;
        text = GetComponent<TextMesh>();
        var pos = spriteTransform.position;
        text.text = string.Format("{0}", 0);
    }
	
	// Update is called once per frame
	void Update ()
    {
        Cell c = transform.parent.GetComponent<Cell>();
        text.text = string.Format("{0}", c.BacteriaOnCell);
	}
}
