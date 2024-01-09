using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/* Sits on all InventorySlots. */

public class InventorySlot : MonoBehaviour {

	public RawImage icon;
	public Item objectInSlot;  //Current item in the slot
	public Text stackAmount;


	// Add item to the slot
	public void AddItem (Item newItem)
	{
		objectInSlot = newItem;
		GetComponentInChildren<Icon_Generator>().generateObjectImage(objectInSlot.interiorObject.transform, icon);
		icon.enabled = true;
        GetComponentInChildren<Image>().color = Color.white;

		GetComponentInChildren<Button>().interactable = true;

		if (objectInSlot.usesLeft > 0)
		{
			if (stackAmount != null) {
				stackAmount.gameObject.SetActive(true);
				stackAmount.GetComponentInChildren<Text>().text = objectInSlot.usesLeft.ToString();
			}
			
		}

		GetComponent<Button>().onClick.AddListener(() => { MapEditor.instance.spawnInteriorObject(objectInSlot);
		
		});
	}

	// Clear the slot
	public void ClearSlot ()
	{

		objectInSlot = null;
		icon.texture = null;
		icon.enabled = false;
        //Color color = new Color(0.7f, 0.7f, 0.7f, 0.9f);

		if (stackAmount.gameObject.activeSelf)
			stackAmount.gameObject.SetActive(false);
        //0 = emptySlot

        //GetComponentInChildren<Image>().color = color;
		GetComponentInChildren<Button>().interactable = false;

	}

	// If the remove button is pressed, this function will be called.
	public void RemoveItemFromInventory ()
	{
		Inventory.instance.Remove(objectInSlot);
	}

    // Use the item
    public void UseItem ()
	{
		if (objectInSlot != null)
		{
			objectInSlot.Use();
		}
	}

}
