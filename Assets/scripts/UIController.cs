using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController :MonoBehaviour
{
    public GameObject EquipmentUI;
    public GameObject InventoryUI;
    public GameObject StatusUI;
    
    void Start() {

    }
    
    void Update() {
        if (Input.GetButtonDown("Equipment")) {
            EquipmentUI.SetActive(!EquipmentUI.activeSelf);
        }

        if (Input.GetButtonDown("Inventory")) {
            InventoryUI.SetActive(!InventoryUI.activeSelf);
        }

        if (Input.GetButtonDown("Status")) {
            StatusUI.SetActive(!StatusUI.activeSelf);
        }
    }

    private void keyOpen() {

    }
}
