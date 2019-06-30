using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    private Transform sky;
    private Material material;
    public float scrollSpeed;

    void Start() {
        sky = this.transform.Find("Sky");
        material = sky.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update() {
        material.mainTextureOffset = new Vector2(Time.time * scrollSpeed, 0);

    }
}
