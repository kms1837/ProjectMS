using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Npc :MonoBehaviour {
    public string dialogueFileName; // NPC 대화 파일
    public GameObject dialogueUI; // 비어있을경우 대화 불가 NPC처리됨.

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {

    }

    private void dialogue(Character inEventTarget) {
        dialogueUI.SetActive(true);
        dialogueUI.GetComponent<Dialogue>().startDialogue(dialogueFileName, inEventTarget, this);
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.tag != "Player") {
            return;
        }

        Character player = collision.gameObject.GetComponent<Character>();

        if (Input.GetButtonDown("Action") && player.action == (int)Character.CharacterAction.Normal && dialogueUI != null) {
            player.action = (int)Character.CharacterAction.Event;
            dialogue(player);
        }
    }
}
