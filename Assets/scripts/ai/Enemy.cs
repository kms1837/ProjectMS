using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy :Character
{
    /*
     1. 대상을 찾음
     2. 공격범위까지 다가감
     3. 대상을 공격
     */

    private void Start() {
    }

    private void movement() {
        if (this.aggroTarget && this.action == (int)Character.CharacterAction.Battle) {
            float directionCheck = this.transform.position.x - this.aggroTarget.position.x;
            this.direction = directionCheck >= 0 ? -1 : 1;
            this.move();

            float distance = Vector2.Distance(this.transform.position,this.aggroTarget.position);
            if (this.infomation.range >= distance) {
                this.attack();
            }
        }
    }

    // Update is called once per frame
    private void FixedUpdate() {
        movement();
    }
}
