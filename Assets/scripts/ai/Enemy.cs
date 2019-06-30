﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    private Character character;

    /*
     1. 대상을 찾음
     2. 공격범위까지 다가감
     3. 대상을 공격
     */

    private void Start() {
        character = this.GetComponent<Character>();
        character.infomation.movementSpeed = character.infomation.movementSpeed / 2;
        character.currentHealthPoint = 30;
    }

    private void movement() {
        if (character.aggroTarget) {
            float directionCheck = this.transform.position.x - character.aggroTarget.position.x;
            character.direction = directionCheck >= 0 ? -1 : 1;
            character.move();
        }
    }

    // Update is called once per frame
    private void FixedUpdate() {
        movement();
    }
}