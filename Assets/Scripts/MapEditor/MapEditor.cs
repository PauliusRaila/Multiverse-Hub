using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class MapEditor : MonoBehaviour
{
    public static MapEditor instance { get; private set; }

    // [Tooltip("Item that player is currently holding.")]
    [SerializeField] public InteriorObject EDITOR_ITEM_SLOT { get; private set; }

    private InteriorObject selectedInteriorObject;
    [SerializeField] private GridStats selectedGridElement;

    private GameObject tooltip; //GET RID OF THIS PLZ
    [SerializeField] private int selectedObjectIndex = 0;

    public enum Mode { player, editor }
    public Mode currentMode = Mode.player;
    public Text currentModeText;
    public static event Action onGameModeChanged;

    public enum editorCategory {
        floor, walls, furniture 
    }
    public editorCategory currentEditorCategory;

    public float targetValue;
    [SerializeField]
    private int tempBlockedDirection = 2;

    //FOR LOADING OBJECT MODELS
    //https://github.com/atteneder/glTFast


    private void Start()
    {

        if (instance == null)
            instance = this;

        GridManager.onGridElementChanged += updateSelectedGridElement;
        onGameModeChanged += resetEditor;
    }
    
    
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            changeMode();
        }


        if (currentMode == Mode.editor && !GridManager.IsPointerOverUIObject()) {

            //Rotates object with mousewheel if editor has picked up item and it's active.
            if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
            {
                //Rotate editor picked up object.
                if (EDITOR_ITEM_SLOT != null && EDITOR_ITEM_SLOT.gameObject.activeSelf)
                {

                    EDITOR_ITEM_SLOT.SetTargetRotationValueY(true);
                    GridManager.instance.updateOccupiedGridSlots();
                    checkIfCanPlace();

                }
                else if (EDITOR_ITEM_SLOT == null && selectedInteriorObject != null) {


                    switch (currentEditorCategory) {

                        case editorCategory.walls:
                            if (selectedObjectIndex >= selectedGridElement.wallsInGrid.Count - 1)
                                selectedObjectIndex = 0;
                            else
                                selectedObjectIndex += 1;
                            break;

                        case editorCategory.furniture:
                            if (selectedObjectIndex >= selectedGridElement.objectsInGrid.Count - 1)
                                selectedObjectIndex = 0;
                            else
                                selectedObjectIndex += 1;
                            break;
                    
                    }
               

                    selectObjectInGridElement();


                }

           
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0) // back            
            {

                //Rotate editor picked up object.
                if (EDITOR_ITEM_SLOT != null && EDITOR_ITEM_SLOT.gameObject.activeSelf)
                {

                    EDITOR_ITEM_SLOT.SetTargetRotationValueY(false);
                    GridManager.instance.updateOccupiedGridSlots();
                    checkIfCanPlace();

                }
                //Change object index in selected grid element.
                else if (EDITOR_ITEM_SLOT == null && selectedInteriorObject != null)
                {


                    switch (currentEditorCategory)
                    {

                        case editorCategory.walls:
                            if (selectedObjectIndex <= 0)
                                selectedObjectIndex = selectedGridElement.wallsInGrid.Count - 1;
                            else
                                selectedObjectIndex -= 1;
                            break;

                        case editorCategory.furniture:
                            if (selectedObjectIndex <= 0)
                                selectedObjectIndex = selectedGridElement.objectsInGrid.Count - 1;
                            else
                                selectedObjectIndex -= 1;
                            break;

                    }

                }

                selectObjectInGridElement();

            }




            // pick or place object
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hitInfo;
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, 1 << LayerMask.NameToLayer("Grid"));
                if (hit)
                {
                   
                    //PICK UP LOWER SLOT OBJECT
                    // if editor item slot is empty and grid has object in it, on click pick up the object, add it to editor item slot.
                    if (EDITOR_ITEM_SLOT == null && selectedInteriorObject != null)
                    {
                        Debug.Log("Pick up ");
                        PickUpObject(selectedInteriorObject);
                        return;
                        
                    }

                    //PLACE OBJECT
                    // if editor hand has object in it and grid is empty, on click place the object, remove it from editor pickup slot.                      
                    if (EDITOR_ITEM_SLOT != null && checkIfCanPlace())
                    {
                        Debug.Log("Place object");                    
                        PlaceObject(EDITOR_ITEM_SLOT);
                        return;
                    }
                
                }
                    
                    

            }//MOUSE CLICK END

            if (Input.GetMouseButton(1) && currentMode == Mode.editor) {

                resetEditor();
            
            }

                    //Make interior gameObject active if it is not or just update position.
                    if (EDITOR_ITEM_SLOT != null && selectedGridElement != null)
                    {
                        if (!EDITOR_ITEM_SLOT.gameObject.active)
                        {                   
                            
                            EDITOR_ITEM_SLOT._currentValueX = selectedGridElement.transform.position.x;
                            EDITOR_ITEM_SLOT._currentValueZ = selectedGridElement.transform.position.z;
                            EDITOR_ITEM_SLOT.transform.position = selectedGridElement.transform.position;
                            EDITOR_ITEM_SLOT.gameObject.SetActive(true);

                            if(EDITOR_ITEM_SLOT.occupiedSlot == InteriorObject.occupationSlot.upper)
                                EDITOR_ITEM_SLOT.SetTargetValueY(2.1f);
                            else
                                EDITOR_ITEM_SLOT.SetTargetValueY(targetValue);

                }
                        else {

                            if (EDITOR_ITEM_SLOT.TargetValueX != selectedGridElement.transform.position.x)
                                EDITOR_ITEM_SLOT.SetTargetValueX(selectedGridElement.transform.position.x);

                            if (EDITOR_ITEM_SLOT.TargetValueZ != selectedGridElement.transform.position.z)
                                EDITOR_ITEM_SLOT.SetTargetValueZ(selectedGridElement.transform.position.z);

                        }
                    }

        }

        //Disable object if mouse is not on grid.
        if (EDITOR_ITEM_SLOT != null && selectedGridElement == null || EDITOR_ITEM_SLOT != null && GridManager.IsPointerOverUIObject())
        {
            EDITOR_ITEM_SLOT.gameObject.SetActive(false);
            
        }    


    } // UPDATE END



    private void changeMode()
    {

        if (currentMode == Mode.player)
        {
            currentMode = Mode.editor;
        }
        else
        {
            currentMode = Mode.player;
        }

        GridManager.instance.toggleGridVisibility();

        if (onGameModeChanged != null)
            onGameModeChanged();

    }


    private void updateSelectedGridElement(GridStats gridElement)
    {
        
        selectedGridElement = gridElement;

        if (currentMode == Mode.editor) {
            selectedObjectIndex = 0;

            if (EDITOR_ITEM_SLOT != null && selectedGridElement != null)
                checkIfCanPlace();

            if(selectedGridElement != null)
                selectObjectInGridElement();
        }
         
    }


    private void selectObjectInGridElement() {

        if (selectedGridElement == null) return;

        if (tooltip != null) Destroy(tooltip);

        if (selectedInteriorObject != null)
        {
            selectedInteriorObject.transform.GetChild(0).gameObject.layer = 0;
            selectedInteriorObject = null;
        }

        if (EDITOR_ITEM_SLOT == null)
        {
            switch (currentEditorCategory)
            {

                case editorCategory.floor:


                    break;

                case editorCategory.walls:
                    if (selectedGridElement.wallsInGrid.Count > 0)
                    {

                        selectedInteriorObject = selectedGridElement.wallsInGrid[selectedObjectIndex];

                        selectedInteriorObject.transform.GetChild(0).gameObject.layer = 7;

                    }

                    break;

                case editorCategory.furniture:

                    if (selectedGridElement.objectsInGrid.Count > 0)
                    {

                        selectedInteriorObject = selectedGridElement.objectsInGrid[selectedObjectIndex];

                        selectedInteriorObject.transform.GetChild(0).gameObject.layer = 7;
                       
                    }

                    break;

            }

            if (selectedInteriorObject != null) {


                tooltip = (GameObject)Instantiate(Resources.Load("Prefabs/UI/Tooltip"));
                tooltip.GetComponentInChildren<Text>().text = selectedInteriorObject.name;
                tooltip.transform.GetComponent<RectTransform>().position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
               
                
            }
    

        }

    }

    private void resetEditor() {

        //If Editor Mode set to Player , make objects not transparent.
        if (currentMode == Mode.player) toggleAllObjectsTransparent(false);
        else if (currentMode == Mode.editor && currentEditorCategory != editorCategory.furniture) toggleAllObjectsTransparent(true);

        if (EDITOR_ITEM_SLOT != null) Destroy(EDITOR_ITEM_SLOT.gameObject);

        if (tooltip != null) Destroy(tooltip);

        if (selectedInteriorObject != null) 
            selectedInteriorObject.transform.GetChild(0).gameObject.layer = 0;

        Debug.Log("resetEditor");

    }

    private void PickUpObject(InteriorObject interiorObject) {
     

        EDITOR_ITEM_SLOT = interiorObject;
        EDITOR_ITEM_SLOT.GetComponentInChildren<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 0.3f);
        EDITOR_ITEM_SLOT.SetModeTransparent(true);

        if (EDITOR_ITEM_SLOT.hasExtraSlot)
        {

            List<InteriorObject> tempList = new List<InteriorObject>();

            //Find objects in extra slots.
            foreach (GridStats tile in EDITOR_ITEM_SLOT.currentlyOccupiedTiles)
            {
                if (tile.objectInExtraSlot == true)
                {
                    foreach (InteriorObject obj in tile.objectsInGrid)
                    {
                        if (obj.occupiedSlot == InteriorObject.occupationSlot.extra)
                        {
                            tempList.Add(obj);

                            obj.transform.parent = EDITOR_ITEM_SLOT.transform;
                            obj.enabled = false;            


                        }
                    }
                }

            }


            foreach (InteriorObject tempObj in tempList)
            {
                foreach (GridStats a in tempObj.currentlyOccupiedTiles)
                {
                    a.removeObject(tempObj);
                    EDITOR_ITEM_SLOT.extraObjects.Add(tempObj);
                    Debug.Log("pickup extra !!");
                }

                tempObj.currentlyOccupiedTiles = new List<GridStats>();
            }

        }

        if (interiorObject.occupiedSlot == InteriorObject.occupationSlot.wall)
        {
            selectedGridElement.removeWall(interiorObject);
        }
        else
        {
            selectedGridElement.removeObject(interiorObject);
        }

        if (EDITOR_ITEM_SLOT.occupiedSlot == InteriorObject.occupationSlot.upper)
            EDITOR_ITEM_SLOT.SetTargetValueY(2.1f);
        else
            EDITOR_ITEM_SLOT.SetTargetValueY(targetValue);


        EDITOR_ITEM_SLOT.currentlyOccupiedTiles = new List<GridStats>();
        GridManager.instance.updateOccupiedGridSlots();

    }

    private void PlaceObject(InteriorObject interiorObject) {
        //If object is a wall just addWall to selected grid element.
        if (interiorObject.occupiedSlot == InteriorObject.occupationSlot.wall) {

            selectedGridElement.addWall(interiorObject);
            
        }
        //Else add object to grid element.
        else
        {
            foreach (GridStats highlightedGrid in GridManager.instance.highlightedGrids)
            {

                interiorObject.currentlyOccupiedTiles.Add(highlightedGrid);
                highlightedGrid.addObject(interiorObject);
                
            }           

        }

        tempBlockedDirection = interiorObject.objectDirection;

 

        //Find objects in extra slots.
        if (interiorObject.hasExtraSlot) {


            foreach (InteriorObject obj in interiorObject.extraObjects) { 
                Debug.Log("place extra !!");

               
                obj.transform.parent = null;

                RaycastHit hitInfo;
                Physics.Raycast(obj.transform.position, Vector3.down * 3, out hitInfo, Mathf.Infinity, 1 << LayerMask.NameToLayer("Grid"));

                obj._currentValueZ = obj.transform.position.z;
                obj._currentValueX = obj.transform.position.x;
                obj._currentValueY = obj.transform.position.y;
        
                obj.SetTargetValueX(hitInfo.transform.position.x);
                obj.SetTargetValueZ(hitInfo.transform.position.z);
                obj.SetTargetValueY(interiorObject.extraSlotHeight.localPosition.y);

                obj.enabled = true;


                obj.currentlyOccupiedTiles.Add(hitInfo.transform.GetComponent<GridStats>());
                hitInfo.transform.GetComponent<GridStats>().addObject(obj);


            }


          
            interiorObject.extraObjects = new List<InteriorObject>();
           
        }
  

        if (interiorObject.occupiedSlot == InteriorObject.occupationSlot.extra)
            foreach (InteriorObject obj in GridManager.instance.highlightedGrid.objectsInGrid) {
                if (obj.hasExtraSlot) {                   
                    interiorObject.SetTargetValueY(obj.transform.GetChild(1).position.y);
                }
                         
            }

       

        else if(interiorObject.occupiedSlot == InteriorObject.occupationSlot.upper)
            interiorObject.SetTargetValueY(1.7f);
        else 
        {          
            interiorObject.SetTargetValueY(-0.1f);
        }
        

        interiorObject.SetModeTransparent(false);
        interiorObject.GetComponentInChildren<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 1f);
        EDITOR_ITEM_SLOT = null;
        GridManager.instance.resetHighlightedGrids();

        if(currentEditorCategory == editorCategory.walls)
            spawnInteriorObject(Resources.Load<Item>("Prefabs/Interior/defaultWall"));

    }

    private bool checkIfCanPlace()
    {
        bool canPlace = true;

        //Check if any highlighted grid is occupied.
        foreach (GridStats gridElement in GridManager.instance.highlightedGrids) {
       

                switch (EDITOR_ITEM_SLOT.GetComponent<InteriorObject>().occupiedSlot)
                {

                    case InteriorObject.occupationSlot.lower:
                        if (gridElement.objectInLowerGrid == true || gridElement.bothSlotObjectInGrid == true)
                            canPlace = false;

                        break;

                    case InteriorObject.occupationSlot.upper:

                        if (gridElement.objectInUpperGrid == true || gridElement.bothSlotObjectInGrid == true)
                            canPlace = false;
                        else
                            //Check if object is against the wall
                            switch (EDITOR_ITEM_SLOT.objectDirection) {
                             //up
                            case 1:
                                    gridElement.blockedDirections.TryGetValue(2, out bool isBlocked1);
                                if (!isBlocked1)     
                                    canPlace = false;
                                

                                break;
                            //right
                            case 2:
                                    gridElement.blockedDirections.TryGetValue(3, out bool isBlocked2);
                                if (!isBlocked2)
                                    canPlace = false;



                                break;
                            //bottom
                            case 3:

                                    gridElement.blockedDirections.TryGetValue(4, out bool isBlocked3);
                                if (!isBlocked3)
                                    canPlace = false;


                                break;
                            //left
                            case 4:
                                    gridElement.blockedDirections.TryGetValue(1, out bool isBlocked4);
                                if (!isBlocked4)            
                                    canPlace = false;                               
                                    break;
                            }                          

                    break;


                case InteriorObject.occupationSlot.extra:
                    bool isThereExtraSlot = false;

                    foreach (InteriorObject interiorObject in gridElement.objectsInGrid) {
                        if (interiorObject.hasExtraSlot) {
                            isThereExtraSlot = true;

                        } 
                       
                    }

                    if (isThereExtraSlot == false)
                        canPlace = false;
                    else {
                        if (gridElement.objectInExtraSlot == true) canPlace = false;                      
                    }

                    break;

                case InteriorObject.occupationSlot.both:

                    if (gridElement.objectInLowerGrid == true || gridElement.objectInUpperGrid == true || gridElement.bothSlotObjectInGrid == true)
                        canPlace = false;

                        break;

            }
            
            
        }
      
        //Check if there is a wall between grid slots.
        if (GridManager.instance.highlightedGrids.Count > 1)
        {
            int x1, x2, y1, y2;

            for(int i = 0; i < GridManager.instance.highlightedGrids.Count; i++)
            {
                if ((i + 1) <= GridManager.instance.highlightedGrids.Count - 1)
                {
                    x1 = GridManager.instance.highlightedGrids[i].x;
                    y1 = GridManager.instance.highlightedGrids[i].y;

                    x2 = GridManager.instance.highlightedGrids[i + 1].x;
                    y2 = GridManager.instance.highlightedGrids[i + 1].y;

                    if (x2 > x1) {

                        GridManager.instance.highlightedGrids[i].blockedDirections.TryGetValue(2, out bool isBlocked);
                        if (isBlocked) canPlace = false;

                    }
                    else if (x2 < x1)
                    {

                        GridManager.instance.highlightedGrids[i].blockedDirections.TryGetValue(4, out bool isBlocked);
                        if (isBlocked) canPlace = false;

                    }
                    else if (y2 > y1)
                    {
                        
                        GridManager.instance.highlightedGrids[i].blockedDirections.TryGetValue(1, out bool isBlocked);
                        if (isBlocked) canPlace = false;

                    }
                    else if (y2 < y1)
                    {

                        GridManager.instance.highlightedGrids[i].blockedDirections.TryGetValue(3, out bool isBlocked);
                        if (isBlocked) canPlace = false;

                    }


                }
            }

        }


        if (EDITOR_ITEM_SLOT.occupiedSlot == InteriorObject.occupationSlot.wall) {

             GridManager.instance.highlightedGrids[0].blockedDirections.TryGetValue(EDITOR_ITEM_SLOT.objectDirection, out bool isBlocked);
             if (isBlocked) canPlace = false;
        }

        if (EDITOR_ITEM_SLOT.gridOccupation.Count > GridManager.instance.highlightedGrids.Count)
            canPlace = false;



        if(canPlace)
            EDITOR_ITEM_SLOT.GetComponentInChildren<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 0.3f);
        else
            EDITOR_ITEM_SLOT.GetComponentInChildren<MeshRenderer>().material.color = new Color(1f, 0, 0, 0.3f);


        return canPlace;
    }

    //PROBABLY MOVE THIS TO INVENTORY
    public void spawnInteriorObject(Item interiorObject)
    {

        if (currentMode != Mode.editor) return;

        if (EDITOR_ITEM_SLOT != null) Destroy(EDITOR_ITEM_SLOT.gameObject);    
      

            EDITOR_ITEM_SLOT = Instantiate(interiorObject.interiorObject);

            EDITOR_ITEM_SLOT.objectDirection = tempBlockedDirection;
       
           // if (EDITOR_ITEM_SLOT.GetComponent<InteriorObject>().occupiedSlot != InteriorObject.occupationSlot.wall)
            EDITOR_ITEM_SLOT.SetModeTransparent(true);

            //Spawn object with the rotation of last object.
            switch (tempBlockedDirection)
            {

                case 1:
                    EDITOR_ITEM_SLOT._currentRotValueY = -90;
                    EDITOR_ITEM_SLOT._targetRotValueY = -90;
                    EDITOR_ITEM_SLOT.transform.rotation = new Quaternion(EDITOR_ITEM_SLOT.transform.rotation.x, -90 , EDITOR_ITEM_SLOT.transform.rotation.z, EDITOR_ITEM_SLOT.transform.rotation.w);
                    break;
                case 2:
                    EDITOR_ITEM_SLOT._currentRotValueY = 0;
                    EDITOR_ITEM_SLOT._targetRotValueY = 0;
                    EDITOR_ITEM_SLOT.transform.rotation = new Quaternion(EDITOR_ITEM_SLOT.transform.rotation.x, 0, EDITOR_ITEM_SLOT.transform.rotation.z, EDITOR_ITEM_SLOT.transform.rotation.w);
                    break;
                case 3:
                    EDITOR_ITEM_SLOT._currentRotValueY = -270;
                    EDITOR_ITEM_SLOT._targetRotValueY = -270;
                    EDITOR_ITEM_SLOT.transform.rotation = new Quaternion(EDITOR_ITEM_SLOT.transform.rotation.x, -270, EDITOR_ITEM_SLOT.transform.rotation.z, EDITOR_ITEM_SLOT.transform.rotation.w);
                    break;
                case 4:

                    EDITOR_ITEM_SLOT._currentRotValueY = -180;
                    EDITOR_ITEM_SLOT._targetRotValueY = -180;
                    EDITOR_ITEM_SLOT.transform.rotation = new Quaternion(EDITOR_ITEM_SLOT.transform.rotation.x, -180, EDITOR_ITEM_SLOT.transform.rotation.z, EDITOR_ITEM_SLOT.transform.rotation.w);
                    break;

            }




        if (selectedGridElement != null) {
            EDITOR_ITEM_SLOT._currentValueX = selectedGridElement.transform.position.x;
            EDITOR_ITEM_SLOT._currentValueZ = selectedGridElement.transform.position.z;
            EDITOR_ITEM_SLOT.transform.position = selectedGridElement.transform.position;
        }
    

        EDITOR_ITEM_SLOT.gameObject.SetActive(true);

        EDITOR_ITEM_SLOT.SetTargetValueY(targetValue);

        GridManager.instance.resetHighlightedGrids();
     
    }

    public void toggleWalls()
    {

        foreach (GridStats tile in GridManager.instance.gridArray)
        {
        

            foreach (InteriorObject a in tile.wallsInGrid)
            {

                if (a.transform.GetChild(1).GetComponent<MeshRenderer>().enabled == false)
                    a.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
                else
                    a.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;

            }

        }

    }



    public void changeLowerWallColor(Color color)
    {

        foreach (GridStats tile in GridManager.instance.gridArray)
        {
            foreach (InteriorObject a in tile.wallsInGrid)
            {            
               a.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = color;
            }

        }

    }

    public void toggleAllObjectsTransparent(bool transparent)
    {

        foreach (GridStats tile in GridManager.instance.gridArray)
        {

            foreach (InteriorObject obj in tile.objectsInGrid)
            {
                if(transparent)
                    obj.GetComponentInChildren<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 0.2f);
                else
                    obj.GetComponentInChildren<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 1f);

                obj.SetModeTransparent(transparent);
              
            }

        }

    }

}
