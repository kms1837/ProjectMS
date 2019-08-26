using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickStatus : MonoBehaviour {
    public GameObject target;
    private Character targetStatus;

    private ProgressBar hpBar;
    private ProgressBar mpBar;
    private ProgressBar expBar;

    private Text hpText;
    private Text mpText;
    private Text expText;
    
    void Start() {
        targetStatus = target.GetComponent<Character>();
        hpBar = this.transform.Find("HPBar").GetComponent<ProgressBar>();
        mpBar = this.transform.Find("MPBar").GetComponent<ProgressBar>();
        expBar = this.transform.Find("EXPBar").GetComponent<ProgressBar>();
        hpBar.init(targetStatus.infomation.healthPoint, new Color(1, 0, 0));
        mpBar.init(targetStatus.infomation.manaPoint, new Color(0, 0, 1));
        expBar.init(0, new Color(0, 1, 0));

        hpText = this.transform.Find("HPText").GetComponent<Text>();
        mpText = this.transform.Find("MPText").GetComponent<Text>();
        expText = this.transform.Find("EXPText").GetComponent<Text>();
    }
    
    void Update() {
        hpBar.setCurrent(targetStatus.currentHealthPoint);
        mpBar.setCurrent(targetStatus.currentManaPoint);

        hpText.text = targetStatus.currentHealthPoint + " / " + targetStatus.infomation.healthPoint.ToString();
        mpText.text = targetStatus.currentManaPoint + " / " + targetStatus.infomation.manaPoint.ToString();
    }
}
