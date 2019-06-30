using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public enum PortalType { Direct, Key };
    // Direct - 닿는 즉시 작동, Key - 행동키를 눌러야 작동

    private bool portalReady; // 연속 작동 방지
    private InGameManager manager;

    [Header("0 - Direct, 1- Action key")]
    [Range(0, 1)]
    public int type;

    [Header("null로 두면 실내 실외 전환")]
    public string moveScene;

    // Use this for initialization
    void Start () {
        portalReady = true;
        manager = GameObject.Find("GameSystem").GetComponent<InGameManager>();
    }
	
	// Update is called once per frame
	void Update () {
	}

    private void portal() {
        if (portalReady) {
            if (moveScene.Length > 0) {
                SceneManager.LoadScene(moveScene);
            }
            else {
                manager.layerExchange();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag != "Player") {
            return;
        }

        if (this.type == (int)Portal.PortalType.Direct) {
            portal();
            portalReady = false;
        }
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.tag != "Player") {
            return;
        }

        if (this.type == (int)Portal.PortalType.Key) {
            if (Input.GetButtonDown("Action")) {
                portal();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.tag != "Player") {
            return;
        }

        portalReady = true;
    }
}
