using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private Character player;
    private bool wallCollision;
    private Collision2D collisionWall;

    void Start () {
        player = this.GetComponent<Character>();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.transform.tag == "Platform") {
            wallCollision = true;
            collisionWall = collision;
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.transform.tag == "Platform") {
            wallCollision = false;
            collisionWall = null;
        }
    }

    void movement() {
        float direction = Input.GetAxis("Horizontal");
        player.direction = direction;

        if (direction != 0) {
            if (!player.isGround && wallCollision) {
                if (player.isJump) {
                    //player.hangOnWall(collisionWall);
                }
                return;
                // 공중상태에서 이동시 벽에 붙는 버그 방지
            }

            if (Input.GetButtonDown("Run")) {
                player.isRun = true;
            }

            if (Input.GetButtonUp("Run")) {
                player.isRun = false;
            }
            player.move();
        }
        else {
            player.idle();
        }
    }

    void jump() {
        if (Input.GetButtonDown("Jump")) {
            player.jump();
        }

        if (Input.GetButtonUp("Jump")) {
            player.jumpEnd();
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
        if (player.action == (int)Character.CharacterAction.Event) {
            return;
        }

        jump();
        movement();
        attack();
        slide();
    }

    private void FixedUpdate() {
        keyAction();
        player.falling();
    }

    void Update () {
    }
}
