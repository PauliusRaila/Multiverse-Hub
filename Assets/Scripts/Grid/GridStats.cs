using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridStats : MonoBehaviour
{
    public int visited = -1;
    public float scale = 1;
    public int x;
    public int y;

    public List<InteriorObject> wallsInGrid;
    public Dictionary<int, bool> blockedDirections; //Walls
    public bool hasBlockedDirections = false;


    public List<InteriorObject> objectsInGrid;
    public bool objectInLowerGrid = false, objectInUpperGrid = false, bothSlotObjectInGrid = false, objectInExtraSlot = false;

    public MeshRenderer tileMesh;

   

    private void Awake()
    {
        objectsInGrid = new List<InteriorObject>();
        blockedDirections = new Dictionary<int, bool>();
        wallsInGrid = new List<InteriorObject>();
    }


    public void addWall(InteriorObject wall) {

        int direction = wall.objectDirection;
        blockedDirections.Add(direction, true);
        hasBlockedDirections = true; // check later ?
        wallsInGrid.Add(wall);

        switch (direction) {

            //up
            case 1:
                if (x < GridManager.instance.gridArray.GetLength(0) && y + 1 < GridManager.instance.gridArray.GetLength(1) && x >= 0 && y + 1 >= 0)
                {

                    GridManager.instance.gridArray[x, y + 1].blockedDirections.Add(3, true);
                    //_gridArray[x, y + 1].wallsInGrid.Add(wall.gameObject);
                    GridManager.instance.gridArray[x, y + 1].hasBlockedDirections = true;
                }

                break;
            //right
            case 2:
                if (x + 1 < GridManager.instance.gridArray.GetLength(0) && y < GridManager.instance.gridArray.GetLength(1) && x + 1 >= 0 && y >= 0)
                {
                    GridManager.instance.gridArray[x + 1, y].blockedDirections.Add(4, true);
                    //_gridArray[x + 1, y].wallsInGrid.Add(wall.gameObject);
                    GridManager.instance.gridArray[x + 1, y].hasBlockedDirections = true;
                }


                break;
            //bottom
            case 3:
                if (x < GridManager.instance.gridArray.GetLength(0) && y - 1 < GridManager.instance.gridArray.GetLength(1) && x >= 0 && y - 1 >= 0)
                {
                    GridManager.instance.gridArray[x, y - 1].blockedDirections.Add(1, true);
                    //_gridArray[x, y - 1].wallsInGrid.Add(wall.gameObject);
                    GridManager.instance.gridArray[x, y - 1].hasBlockedDirections = true;
                }


                break;
            //left
            case 4:
                if (x - 1 < GridManager.instance.gridArray.GetLength(0) && y < GridManager.instance.gridArray.GetLength(1) && x - 1 >= 0 && y >= 0)
                {
                    GridManager.instance.gridArray[x - 1, y].blockedDirections.Add(2, true);
                    //_gridArray[x - 1, y].wallsInGrid.Add(wall.gameObject);
                    GridManager.instance.gridArray[x - 1, y].hasBlockedDirections = true;
                }

                break;


        }

    }

    public void removeWall(InteriorObject wall)
    {
        int direction = wall.objectDirection;
        blockedDirections.Remove(direction);
        hasBlockedDirections = true; // check later ?
        wallsInGrid.Remove(wall);


        switch (direction)
        {

            //up
            case 1:
                if (x < GridManager.instance.gridArray.GetLength(0) && y + 1 < GridManager.instance.gridArray.GetLength(1) && x >= 0 && y + 1 >= 0)
                {

                    GridManager.instance.gridArray[x, y + 1].blockedDirections.Remove(3);
                    if (GridManager.instance.gridArray[x, y + 1].blockedDirections.Count == 0)
                        GridManager.instance.gridArray[x, y + 1].hasBlockedDirections = false;

                }

                break;
            //right
            case 2:
                if (x + 1 < GridManager.instance.gridArray.GetLength(0) && y < GridManager.instance.gridArray.GetLength(1) && x + 1 >= 0 && y >= 0)
                {

                    GridManager.instance.gridArray[x + 1, y].blockedDirections.Remove(4);
                    if (GridManager.instance.gridArray[x + 1, y].blockedDirections.Count == 0)
                        GridManager.instance.gridArray[x + 1, y].hasBlockedDirections = false;

                }


                break;
            //bottom
            case 3:
                if (x < GridManager.instance.gridArray.GetLength(0) && y - 1 < GridManager.instance.gridArray.GetLength(1) && x >= 0 && y - 1 >= 0)
                {

                    GridManager.instance.gridArray[x, y - 1].blockedDirections.Remove(1);
                    if (GridManager.instance.gridArray[x, y - 1].blockedDirections.Count == 0)
                        GridManager.instance.gridArray[x, y - 1].hasBlockedDirections = false;

                }


                break;
            //left
            case 4:
                if (x - 1 < GridManager.instance.gridArray.GetLength(0) && y < GridManager.instance.gridArray.GetLength(1) && x - 1 >= 0 && y >= 0)
                {

                    GridManager.instance.gridArray[x - 1, y].blockedDirections.Remove(2);
                    if (GridManager.instance.gridArray[x - 1, y].blockedDirections.Count == 0)
                        GridManager.instance.gridArray[x - 1, y].hasBlockedDirections = false;

                }

                break;


        }





        if (wallsInGrid.Count > 0)
            hasBlockedDirections = true;
        else
            hasBlockedDirections = false;

        //   blockedDirections

        //  if (blockedGrid != null)
        //    blockedGrid.removeWall();

        //  blockedGrid = null;




    }

    public void addObject(InteriorObject interiorObject) {

        objectsInGrid.Add(interiorObject);

        switch (interiorObject.occupiedSlot) {
            case InteriorObject.occupationSlot.lower:
                objectInLowerGrid = true;
                break;

            case InteriorObject.occupationSlot.upper:
                objectInUpperGrid = true;

                break;

            case InteriorObject.occupationSlot.both:
                bothSlotObjectInGrid = true;

                break;
            case InteriorObject.occupationSlot.extra:

                objectInExtraSlot = true;
                break;

                //extra slot
        }
        
    }

    public void removeObject(InteriorObject interiorObject)
    {
        Debug.Log("removeObject " + interiorObject.name);

        foreach (GridStats occupiedGrid in interiorObject.currentlyOccupiedTiles)
        {
           
            switch (interiorObject.occupiedSlot)
            {
                case InteriorObject.occupationSlot.lower:

                    occupiedGrid.objectInLowerGrid = false;
                      
                    break;

                case InteriorObject.occupationSlot.upper:

                    occupiedGrid.objectInUpperGrid = false;

                    break;

                case InteriorObject.occupationSlot.both:

                    occupiedGrid.bothSlotObjectInGrid = false;

                    break;

                case InteriorObject.occupationSlot.extra:

                    occupiedGrid.objectInExtraSlot = false;

                    break;

            }

            occupiedGrid.objectsInGrid.Remove(interiorObject);

        }

    }

}
