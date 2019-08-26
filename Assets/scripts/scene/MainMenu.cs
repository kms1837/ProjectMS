using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public Transform SceneFaderImage;

    void Start () {
        SceneFaderImage.GetComponent<Fader>().fadeOutStart(() => {
            SceneFaderImage.gameObject.SetActive(false);
        });
    }
	
	void Update () {
		
	}

    public void newGame() {
        SceneFaderImage.GetComponent<Fader>().fadeInStart(() => {
            SceneManager.LoadScene("event");
        });
    }
}
