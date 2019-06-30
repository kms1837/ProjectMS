using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour {
    private GameObject Map;
    private GameObject npcGroup;
    private GameObject enemyGroup;
    private int currentLayer; // 시작레이어(0-밖, 1-안)

    private void childsLayerActive(GameObject group) {
        foreach (Transform child in group.transform) {
            if (child.GetComponent<Character>().layer == currentLayer) {
                child.gameObject.SetActive(true);
            }
            else {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void childsLayerActive() {
        childsLayerActive(npcGroup);
        childsLayerActive(enemyGroup);
    }

    public void layerExchange() {
        GameObject indoor = Map.transform.Find("Indoor").gameObject;
        GameObject outdoor = Map.transform.Find("Outdoor").gameObject;
        indoor.SetActive(outdoor.activeSelf ? true : false);
        outdoor.SetActive(outdoor.activeSelf ? false : true);

        currentLayer = currentLayer == 1 ? 0 : 1;
        childsLayerActive();
    }

    void Start () {
        Map = GameObject.Find("Map");
        npcGroup = GameObject.Find("NpcGroup");
        enemyGroup = GameObject.Find("EnemyGroup");
        childsLayerActive();
    }

	void Update () {
		
	}
}
