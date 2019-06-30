using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : MonoBehaviour {
    public int row;
    public int margin;
    public GameObject baseItemBox;

    private Transform items;

	void Start () {
        int currentRow = 1;
        float currentX, currentY;
        float firstX;
        Rect boxBoxRect = baseItemBox.transform.Find("Box").GetComponent<RectTransform>().rect;
        Rect itemsRect; 
        items = this.transform.Find("Items");
        itemsRect = items.GetComponent<RectTransform>().rect;

        firstX = (-1 * (itemsRect.width / 2)) + (boxBoxRect.width/2);
        currentY = (itemsRect.height/2) - (boxBoxRect.height / 2);
        currentX = firstX;

        while (currentRow <= row) {
            Transform newItemBox = Instantiate(baseItemBox, items).transform;
            newItemBox.localPosition = new Vector2(currentX, currentY);

            currentX += (boxBoxRect.width + margin);

            if ((itemsRect.width/2) < currentX) {
                currentRow++;
                currentX = firstX;
                currentY -= (boxBoxRect.height + margin);
            }
        }
    }
	
	void Update () {
		
	}
}
