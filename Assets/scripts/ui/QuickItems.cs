using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickItems :MonoBehaviour
{
    [SerializeField]
    private Character target;

    private Transform[] slotObject;
    private Item[] slotItem;
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

        slotItem = new Item[3];
        setSlotItem(0, target.inventory[0] as Item);
        setSlotItem(1, target.inventory[1] as Item);
        setSlotItem(2, target.inventory[2] as Item);
    }

    public void setSlotItem(int setSlot, Item setItem) {
        if (setSlot >= 0 && setSlot < 3 && setItem != null) {
            slotItem[setSlot] = setItem;
            stackLabel[setSlot].text = setItem.ItemStack > 0 ? setItem.ItemStack.ToString() : "";

            slotObject[setSlot].GetComponent<ProgressBar>().init(1, new Color(255.0f,255.0f,255.0f));
        }
    }

    void Update() {
        if (Input.GetButtonDown("Item1") && slotItem[0] != null) {
            usingItem(0);
        }

        if (Input.GetButtonDown("Item2") && slotItem[1] != null) {
            usingItem(1);
        }

        if (Input.GetButtonDown("Item3") && slotItem[2] != null) {
            usingItem(2);
        }
    }

    private void usingItem(int usingSlot) {
        if (slotItem[usingSlot] != null) {
            slotItem[usingSlot].use();
            stackLabel[usingSlot].text = slotItem[usingSlot].ItemStack > 0 ? slotItem[usingSlot].ItemStack.ToString() : "";

            if (slotItem[usingSlot].ItemStack <= 0) {
                slotObject[usingSlot].Find("Bar").gameObject.SetActive(false);
                slotObject[usingSlot].Find("MoveBar").gameObject.SetActive(false);
            }
        }   
    }
}
