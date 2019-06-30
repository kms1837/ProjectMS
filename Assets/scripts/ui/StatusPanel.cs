using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusPanel : MonoBehaviour {
    public GameObject target;
    private Character targetStatus;

    private StatusBar hpBar;
    private StatusBar mpBar;
    private StatusBar expBar;

    private Text hpText;
    private Text mpText;
    private Text expText;

    private Text movementText;
    private Text defText;
    private Text rangeText;
    private Text phyText;
    private Text magicText;

    // Use this for initialization
    void Start () {
        targetStatus = target.GetComponent<Character>();
        Transform hpForm = this.transform.Find("HPForm");
        hpBar = hpForm.Find("HPBar").GetComponent<StatusBar>();
        hpBar.init(targetStatus.infomation.healthPoint, new Color(1, 0, 0));
        hpText = hpForm.Find("HPText").GetComponent<Text>();

        Transform mpForm = this.transform.Find("MPForm");
        mpBar = mpForm.Find("MPBar").GetComponent<StatusBar>();
        mpBar.init(targetStatus.infomation.manaPoint, new Color(0, 0, 1));
        mpText = mpForm.Find("MPText").GetComponent<Text>();

        Transform expForm = this.transform.Find("EXPForm");
        expBar = expForm.Find("EXPBar").GetComponent<StatusBar>();
        expBar.init(0, new Color(0, 1, 0));
        expText = expForm.Find("EXPText").GetComponent<Text>();

        Transform movementForm = this.transform.Find("MovementForm");
        movementText = movementForm.Find("PointText").GetComponent<Text>();

        Transform defForm = this.transform.Find("DefForm");
        defText = defForm.Find("PointText").GetComponent<Text>();

        Transform rangeForm = this.transform.Find("RangeForm");
        rangeText = rangeForm.Find("PointText").GetComponent<Text>();

        Transform phyForm = this.transform.Find("PhyForm");
        phyText = phyForm.Find("PointText").GetComponent<Text>();

        Transform magicForm = this.transform.Find("MagicForm");
        magicText = magicForm.Find("PointText").GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
        hpBar.setCurrent(targetStatus.currentHealthPoint);
        mpBar.setCurrent(targetStatus.currentManaPoint);

        hpText.text = targetStatus.currentHealthPoint + " / " + targetStatus.infomation.healthPoint.ToString();
        mpText.text = targetStatus.currentManaPoint + " / " + targetStatus.infomation.manaPoint.ToString();

        movementText.text = targetStatus.infomation.movementSpeed.ToString();
        defText.text = targetStatus.infomation.defencePoint.ToString();
        rangeText.text = targetStatus.infomation.range.ToString();
        phyText.text = targetStatus.infomation.healthPower.ToString();
        magicText.text = targetStatus.infomation.magicPower.ToString(); ;
}
}
