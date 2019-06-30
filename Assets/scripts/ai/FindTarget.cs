using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindTarget : MonoBehaviour {
    public GameObject targetGroup;

    private void Start() {
        if (!targetGroup) {
            Debug.LogWarning("목표 대상을 설정하지 않으면 자동 행동이 동작하지 않을 수 있습니다.");
        }
    }

    void OnTriggerEnter2D(Collider2D target) {
        if (!targetGroup) {
            return;
        }

        if (target.transform.IsChildOf(targetGroup.transform) && (target.tag == "Character" || target.tag == "Player")) {
            Character character = this.transform.parent.GetComponent<Character>();
            character.setAggroTarget(target.transform);
            character.action = (int)Character.CharacterAction.Battle;
        };
    }

    // 목표 대상을 찾습니다.
}
