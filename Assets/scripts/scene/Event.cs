using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event : MonoBehaviour
{
    public GameObject dialogueUI;

    void Start()
    {
        PlayerStats.EventFilePath = "test";
        dialogueUI.SetActive(true);
        dialogueUI.GetComponent<Dialogue>().startDialogue(PlayerStats.EventFilePath);
    }
    
    void Update()
    {
        
    }
}
