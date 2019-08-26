using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour {
    private GameObject Map;
    private GameObject npcGroup;
    private GameObject enemyGroup;
    public GameObject quickUIGroup;
    private int currentLayer; // 시작레이어(0-밖, 1-안)

    [SerializeField]
    private Transform cameraTarget; // 카메라가 따라다닐 대상
    private GameObject background;

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

        Color backgroundColor;
        backgroundColor = outdoor.activeSelf ? new Color(0.33f, 0.58f, 0.98f) : new Color(0,0,0);
        Camera.main.backgroundColor = backgroundColor;
        background.SetActive(outdoor.activeSelf);

        currentLayer = currentLayer == 1 ? 0 : 1;
        childsLayerActive();
    }

    void Start () {
        Map = GameObject.Find("Map");
        npcGroup = GameObject.Find("NpcGroup");
        enemyGroup = GameObject.Find("EnemyGroup");
        quickUIGroup = GameObject.Find("Canvas").transform.Find("QuickUI").gameObject;

        background = Camera.main.transform.Find("Background").gameObject;
        childsLayerActive();
    }

	void Update () {
        if (cameraTarget != null) {
            Camera.main.transform.position = cameraTarget.position - Vector3.forward;
        }
    }
}
