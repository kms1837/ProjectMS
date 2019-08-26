using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickSkills : MonoBehaviour
{
    [SerializeField]
    private Character target;

    private Transform[] slotObject;
    private Skill[] slotSkill;
    private Text[] stackLabel;

    void Start() {
        if (object.ReferenceEquals(target, null)) {
            return;
        }

        int index = 0;
        stackLabel = new Text[3];
        slotObject = new Transform[3];
        foreach (Transform slot in this.transform) {
            slotObject[index] = slot;
            stackLabel[index] = slot.Find("Text").GetComponent<Text>();
            index++;
        }

        slotSkill = new Skill[3];
        this.setSlotSkill(0, target.skills[0] as Skill);
        this.setSlotSkill(1, target.skills[1] as Skill);
        this.setSlotSkill(2, target.skills[2] as Skill);
    }

    public void setSlotSkill(int setSlot, Skill setSkill) {
        if (setSlot >= 0 && setSlot < 3 && setSkill != null) {
            slotSkill[setSlot] = setSkill;
            ProgressBar skillProgress = slotObject[setSlot].GetComponent<ProgressBar>();
            slotObject[setSlot].Find("Sprite").GetComponent<Image>().sprite = setSkill.iconSprite;
            slotObject[setSlot].Find("MoveBar").GetComponent<Image>().sprite = setSkill.iconSprite;
            slotObject[setSlot].Find("Bar").GetComponent<Image>().sprite = setSkill.iconSprite;
            setSkill.coolTImeBarSetting(skillProgress, new Color(255.0f, 255.0f, 255.0f));
        }
    }

    void Update() {
        if (Input.GetButtonDown("Skill1") && slotSkill[0] != null) {
            usingSkill(0);
        }

        if (Input.GetButtonDown("Skill2") && slotSkill[1] != null) {
            usingSkill(1);
        }

        if (Input.GetButtonDown("Skill3") && slotSkill[2] != null) {
            usingSkill(2);
        }
    }

    private void usingSkill(int usingSlot) {
        if (slotSkill[usingSlot] != null) {
            slotSkill[usingSlot].activation();
        }
    }
}
