using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class GridManager : MonoBehaviour
{

    public static GridManager instance { get; private set; }

    public int rows  = 10;
    public int columns  = 10;
    public float scale = 1;

    public GameObject gridPrefab;
    public Vector3 leftBottomLocation = new Vector3(0, 0, 0);
    public GridStats[,] gridArray { get; private set; }
    public GridStats highlightedGrid { get; private set; }
    public List<GridStats> highlightedGrids { get; private set; } = new List<GridStats>();

    public static event Action<GridStats> onGridElementChanged;

    private bool isGridVisible = false;
   
    // Start is called before the first frame update
    void Awake()
    {
        //MOVE TO GAME MANAGER ??
        if (instance == null)
            instance = this;
        

        gridArray = new GridStats[columns, rows];
        if (gridPrefab) {

            GenerateGrid();

            foreach (GridStats tile in gridArray) {
                if (tile.x == columns - 1) {
                    //change?
                    InteriorObject obj = Instantiate(Resources.Load<InteriorObject>("Prefabs/Interior/fullWall"));
                    obj.gameObject.SetActive(false);
                    obj._currentValueX = tile.transform.position.x;
                    obj._currentValueZ = tile.transform.position.z;
                    obj._targetValueX = tile.transform.position.x;
                    obj._targetValueZ = tile.transform.position.z;
                    obj.objectDirection = 2;
                    obj.transform.position = tile.transform.position;
                    obj.gameObject.SetActive(true);
                    tile.addWall(obj);
                    


                }

                if (tile.y == rows - 1)
                {
                    //change?
                    InteriorObject obj = Instantiate(Resources.Load<InteriorObject>("Prefabs/Interior/fullWall"));
                    obj.gameObject.SetActive(false);
                    obj._currentValueX = tile.transform.position.x;
                    obj._currentValueZ = tile.transform.position.z;
                    obj._targetValueX = tile.transform.position.x;
                    obj._targetValueZ = tile.transform.position.z;
                    obj.objectDirection = 1;
                    obj._targetRotValueY = -90;
                    obj._currentRotValueY = -90;
                    obj.transform.position = tile.transform.position;
                    obj.gameObject.SetActive(true);
                    tile.addWall(obj);



                }

            }


         
            
        }
            
        else
            print("Missing assigned gridPrefab");

    }

    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void FixedUpdate()
    {

         
            //Highlight grid
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, 1 << LayerMask.NameToLayer("Grid"));

            //grid highlight when mouse over
            if (hit && highlightedGrid != hitInfo.collider.gameObject.GetComponent<GridStats>())
            {

                

                if (MapEditor.instance?.currentMode == MapEditor.Mode.editor)
                    foreach (GridStats a in highlightedGrids)
                       a.transform.GetComponent<MeshRenderer>().material.color = new Color(0.7f, 0.7f, 0.7f, 0.4f);
                else
                    foreach (GridStats a in highlightedGrids)
                        a.transform.GetComponent<MeshRenderer>().material.color = new Color(0.7f, 0.7f, 0.7f, 0f);

                highlightedGrids = new List<GridStats>();
                highlightedGrid = hitInfo.collider.GetComponent<GridStats>();
                highlightedGrid.GetComponent<MeshRenderer>().material.color = Color.white;
                highlightedGrids.Add(highlightedGrid);


            
                if (MapEditor.instance?.EDITOR_ITEM_SLOT != null)
                    updateOccupiedGridSlots();

                if (onGridElementChanged != null)
                    onGridElementChanged(highlightedGrid);

               
            }
            
            if (highlightedGrid != null && !hit || highlightedGrid != null  && IsPointerOverUIObject())
            {
                resetHighlightedGrids();

            if (onGridElementChanged != null)
                onGridElementChanged(highlightedGrid);

        }


        

    }

    public void updateOccupiedGridSlots() {

        //Debug.Log("updateOccupiedGridSlots");

        foreach (GridStats a in highlightedGrids)
            a.transform.GetComponent<MeshRenderer>().material.color = new Color(0.7f, 0.7f, 0.7f, 0.4f);

        highlightedGrids = new List<GridStats>();

        InteriorObject item = MapEditor.instance.EDITOR_ITEM_SLOT;

        //Check what positions does object occupy
        foreach (Vector2 pos in item.gridOccupation)
        {
            if (highlightedGrid == null) return;
            int x = highlightedGrid.x;
            int y = highlightedGrid.y;          

            //Depending on item rotation highlight correct grid slots
            switch (item.objectDirection)
            {

                case 2:
                   

                    if ((x - (int)pos.x) < gridArray.GetLength(0) && (y + (int)pos.y) < gridArray.GetLength(1) && (x - (int)pos.x) >= 0 && (y + (int)pos.y) >= 0)
                    {                

                        gridArray[x - (int)pos.x, y + (int)pos.y].GetComponent<MeshRenderer>().material.color = Color.white;
                        highlightedGrids.Add(gridArray[x - (int)pos.x, y + (int)pos.y]);

                    }

                    break;

                case 3:

                    if ((x + (int)pos.y) < gridArray.GetLength(0) && (y + (int)pos.x) < gridArray.GetLength(1) && (x + (int)pos.y) >= 0 && (y + (int)pos.x) >= 0)
                    {

                        gridArray[x + (int)pos.y, y + (int)pos.x].GetComponent<MeshRenderer>().material.color = Color.white;
                        highlightedGrids.Add(gridArray[x + (int)pos.y, y + (int)pos.x]);

                    }

                    break;


                case 4:

                    if ((x + (int)pos.x) < gridArray.GetLength(0) && (y - (int)pos.y) < gridArray.GetLength(1) && (x + (int)pos.x) >= 0 && (y - (int)pos.y) >= 0)
                    {

                        gridArray[x + (int)pos.x, y - (int)pos.y].GetComponent<MeshRenderer>().material.color = Color.white;
                        highlightedGrids.Add(gridArray[x + (int)pos.x, y - (int)pos.y]);

                    }

                    break;


                case 1:


                    if ((x - (int)pos.y) < gridArray.GetLength(0) && (y - (int)pos.x) < gridArray.GetLength(1) && (x - (int)pos.y) >= 0 && (y - (int)pos.x) >= 0)
                    {

                        gridArray[x - (int)pos.y, y - (int)pos.x].GetComponent<MeshRenderer>().material.color = Color.white;
                        highlightedGrids.Add(gridArray[x - (int)pos.y, y - (int)pos.x]);

               
                    }

                    break;

            }
        }
    }

    public void resetHighlightedGrids() {

 
        if (MapEditor.instance?.currentMode == MapEditor.Mode.editor)
            foreach (GridStats a in highlightedGrids)
                a.transform.GetComponent<MeshRenderer>().material.color = new Color(0.7f, 0.7f, 0.7f, 0.4f);
        else if(MapEditor.instance?.currentMode == MapEditor.Mode.player || MapEditor.instance == null)
            foreach (GridStats a in highlightedGrids)
                a.transform.GetComponent<MeshRenderer>().material.color = new Color(0.7f, 0.7f, 0.7f, 0f);


        highlightedGrid = null;
        highlightedGrids = new List<GridStats>();

    }

    public void toggleGridVisibility() {

        highlightedGrid = null;
        highlightedGrids = new List<GridStats>();

        if (isGridVisible == false)
        {
            foreach (GridStats gridSlot in gridArray)
            {
                gridSlot.GetComponent<MeshRenderer>().material.color = new Color(0.7f, 0.7f, 0.7f, 0.4f);
            }

            isGridVisible = true;
        }
        else {
            foreach (GridStats gridSlot in gridArray)
            {
                gridSlot.GetComponent<MeshRenderer>().material.color = new Color(0.7f, 0.7f, 0.7f, 0f);
            }

            isGridVisible = false;
        }

    }

    


    private void GenerateGrid()
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {               
                 GridStats obj = Instantiate(gridPrefab, new Vector3(leftBottomLocation.x + scale * x, leftBottomLocation.y, leftBottomLocation.z + scale * y), Quaternion.identity).GetComponent<GridStats>();
                 obj.transform.SetParent(gameObject.transform);
                 obj.scale = scale;
                 obj.x = x;
                 obj.y = y;
                 obj.name = "grid" + x.ToString() + y.ToString();
                 gridArray[x, y] = obj;
            }
        }
    }

    
}

