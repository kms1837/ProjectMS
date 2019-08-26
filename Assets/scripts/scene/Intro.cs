using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour {
    public Transform[] FadeImages;
    private int fadeIndex;

    void Start () {
        fadeIndex = 0;

        PlayerPrefs.SetInt("firstRun", 0);
        if (PlayerPrefs.GetInt("firstRun") == 0) {
            PlayerPrefs.SetInt("firstRun", 1);
            PlayerPrefs.Save();

            DataBase ms = new DataBase("MS");
            ms.initDataBase();
            ms.closeDB();
            ms = null;
            Debug.Log("init Database");
        }

        Fader fader = FadeImages[fadeIndex].GetComponent<Fader>();
        fader.fadeTime = 2.0f;
        fader.fadeInOutStart(nextFade);
    }
	
    void nextFade() {
        fadeIndex++;

        if (fadeIndex < FadeImages.Length) {
            Fader fader = FadeImages[fadeIndex].GetComponent<Fader>();
            fader.fadeInOutStart(nextFade);
            fader.fadeTime = 2.0f;
        }
        else {
            SceneManager.LoadScene("main_menu");
        }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
