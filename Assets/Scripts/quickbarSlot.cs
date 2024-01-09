using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/* Sits on all InventorySlots. */

public class quickbarSlot : MonoBehaviour {

	public Image icon;
	public InteriorObject objectInSlot;   // Current item in the slot
	//public GameObject stack;


	// Add item to the slot
	public void AddObject (InteriorObject newObject)
	{

		objectInSlot = newObject;
		//icon.sprite = newObject.sprite;
		icon.color = Color.white;
		icon.enabled = true;

		if (newObject.Amount > 0)
		{
			
			
		}

	}

	// Clear the slot
	public void ClearSlot ()
	{

		objectInSlot = null;
		icon.sprite = null;
		icon.enabled = false;
        //Color color = new Color(0.7f, 0.7f, 0.7f, 0.9f);
		//if (stack.activeSelf)
		//	stack.SetActive(false);
        //0 = emptySlot      

	}

	

}
