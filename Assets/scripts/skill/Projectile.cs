using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Ability infomation;
    public int groupNumber;
    public float direction = 1; // 방향

    public void hit(Character target) {
        target.hit(this.infomation.power, this.direction);
        Destroy(this.gameObject);
    }

    private void move() {
        this.transform.position += new Vector3((this.direction * infomation.movementSpeed), 0, 0);
    }

    void Awake() {
        this.infomation = new Ability();
        this.infomation.movementSpeed = 0.3f;
    }
    
    void Update() {
        this.move();
    }

    private void OnBecameInvisible() {
        Invoke("InvisibleDestroy", 2.0f); // 카메라 밖으로 나가면 2초 유예 시간을 주고 삭제한다.
    }

    void InvisibleDestroy() {
        Destroy(this.gameObject);
    }
}
