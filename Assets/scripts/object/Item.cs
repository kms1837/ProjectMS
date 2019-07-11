using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item :Ability
{
    const int maxStack = 9;
    public int type;
    private int itemStack;
    public int ItemStack {
        get {
            return itemStack;
        }
    }
    public ArrayList inventory;

    public enum ItemType
    {
        Equipment, Weapon, Usable
    }

    public Item(ArrayList setInventory, int setStack) {
        inventory = setInventory;
        itemStack = setStack;
    }

    public void use() {
        itemStack--;

        if (itemStack <= 0) {
            inventory.Remove(this);
        }
    }
}