using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/* This object manages the inventory UI. */

public class EditorUI : MonoBehaviour {

    public Transform itemsParent;	// The parent object of all the items.
    
    private int slotIndex = 0;
   // public Button selectFloorCategory, selectWallCategory, selectFurnitureCategory;


    public Item.ItemSubCategory selectedSubCategory = Item.ItemSubCategory.walls;
    public Text categoryTitle, modeText;

    public List<GameObject> categoriesUI;

    public GameObject _inventorySlots, _categoryButtons, _categories;
    public Button toggleWall;

   


    private void OnLevelWasLoaded(int level)
    {

        // categoriesParent.Find("helm").GetComponentInChildren<Button>().onClick.AddListener(delegate { selectCategory("helm"); });         
        // selectFloorCategory.onClick.AddListener(() => { selectCategory(""); });
        // selectWallCategory.onClick.AddListener(() => { selectCategory("Wall"); });
       

    }


    void Start ()
	{

        Inventory.instance.onItemChangedCallback += UpdateUI;
        MapEditor.onGameModeChanged += toggleEditorUI;
        toggleWall.onClick.AddListener(() => { MapEditor.instance.toggleWalls(); });
        UpdateUI();	

	}

	// Update the inventory UI by:
	//		- Adding items
	//		- Clearing empty slots
	// This is called using a delegate on the Inventory.
	public void UpdateUI ()
	{       //pages and what item indexes to load
         // 1 [0 - 21]
         //  2 [22 - 43]
         // 3 [44 - 65]
		  InventorySlot[] slots = itemsParent.GetComponentsInChildren<InventorySlot>();

          for (int i = 0; i < slots.Length; i++)
          {
             slots[i].ClearSlot();
          }

                slotIndex = 0;
               

            foreach (Item item in Inventory.instance.inventoryItems)
            {

            if (item.currentItemCategory == MapEditor.instance.currentEditorCategory && item.currentItemSubCategory == selectedSubCategory)
                {
                    slots[slotIndex].AddItem(item);
                    slotIndex += 1;
                }

            }
            

        
        slotIndex = 0;
	}


   

    public void toggleEditorUI() {

        modeText.text = MapEditor.instance.currentMode.ToString() + " mode" ;

        if (MapEditor.instance.currentMode == MapEditor.Mode.player)
        {

            _inventorySlots.SetActive(false);
            _categories.SetActive(false);
            _categoryButtons.SetActive(false);

        }
        else {
            _inventorySlots.SetActive(true);
            _categories.SetActive(true);
            _categoryButtons.SetActive(true);
            


        }

    }

    public void selectCategory(string category)
    {
        
        MapEditor.editorCategory parsed_enum = (MapEditor.editorCategory)System.Enum.Parse(typeof(MapEditor.editorCategory), category);
        MapEditor.instance.currentEditorCategory = parsed_enum;

        foreach (GameObject a in categoriesUI)
            a.SetActive(false);

        switch (MapEditor.instance.currentEditorCategory) {
            case MapEditor.editorCategory.floor:
                categoriesUI[0].SetActive(true);
                MapEditor.instance.toggleAllObjectsTransparent(true);
                break;
            case MapEditor.editorCategory.walls:
                categoriesUI[1].SetActive(true);
                MapEditor.instance.toggleAllObjectsTransparent(true);
                selectSubCategory("walls");
                break;
            case MapEditor.editorCategory.furniture:
                categoriesUI[2].SetActive(true);
                MapEditor.instance.toggleAllObjectsTransparent(false);
                selectSubCategory("livingroom");

                break;

        }


        
        UpdateUI();
        
    }

    public void selectSubCategory(string category)
    {


        Item.ItemSubCategory parsed_enum = (Item.ItemSubCategory)System.Enum.Parse(typeof(Item.ItemSubCategory), category);
        selectedSubCategory = parsed_enum;


        categoryTitle.text = category;

        UpdateUI();

    }


}
