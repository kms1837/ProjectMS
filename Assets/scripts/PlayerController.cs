using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private Character player;
    
    void Start () {
        player = this.GetComponent<Character>();
    }

    void movement() {
        float direction = Input.GetAxis("Horizontal");
        player.direction = direction;

        if (direction != 0) {
            if (Input.GetButtonDown("Run")) {
                player.isRun = true;
            }

            if (Input.GetButtonUp("Run")) {
                player.isRun = false;
            }
            player.move();
        }
    }

    void jump() {
        if (Input.GetButtonDown("Jump")) {
            player.jump();
        }
    }

    void attack() {
        if (Input.GetButtonDown("Attack")) {
            player.attack();
        }
    }

    void slide() {
        float direction = Input.GetAxis("Vertical");

        if (direction < 0) {
            if (Input.GetButtonDown("Action")) {
                player.slide();
            }
        }
    }

    private void keyAction() {
        movement();
        jump();
        attack();
        slide();
    }

    private void FixedUpdate() {
        keyAction();
        player.checkGround();
    }

    void Update () {
        //keyAction();
    }
}
