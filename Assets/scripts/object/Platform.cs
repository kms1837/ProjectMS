using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter2D (Collision2D collision) {
        /*Debug.Log("[a]");
        Debug.Log(this.transform.position);
        Debug.Log("[b]");
        Debug.Log(collision.ToString());*/
    }
}
