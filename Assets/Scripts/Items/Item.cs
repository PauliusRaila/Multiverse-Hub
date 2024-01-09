using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject {

	new public string name = "New Item";	// Name of the item
	public Sprite sprite = null;		    // Item icon
    public Mesh mesh;
    public Material mat;

    public InteriorObject interiorObject;

    public string itemID;
    public int usesLeft;

    public enum ItemSubCategory { walls , livingroom, bathroom, kitchen, electronics, decorations, misc }

    public ItemSubCategory currentItemSubCategory = ItemSubCategory.walls;

    public MapEditor.editorCategory currentItemCategory = MapEditor.editorCategory.furniture;
    private string _instanceID = null;

    public string instanceID
    {

        get { return _instanceID; }
        set
        {
            if (_instanceID == null)
            {
                _instanceID = value;
            }
        }

    }


    // Called when the item is pressed in the inventory
    public virtual void Use ()
	{
		// Use the item
		// Something may happen
	}

    public virtual void Consume()
    {
        // Use the item
        // Something may happen
    }

    public virtual void SelectItem()
    {
        

    }

    // Call this method to remove the item from inventory
    public void RemoveFromInventory ()
	{
		//Inventory.instance.Remove(this);
	}

}
