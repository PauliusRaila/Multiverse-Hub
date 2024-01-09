using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerController : MonoBehaviour
{
    public static playerController instance { get; set; }

    public List<GridStats> path = new List<GridStats>();

    [SerializeField]
    int startX = 0;
    [SerializeField]
    int startY = 0;
   
    public int endX = 0;   
    public int endY = 0;
    public bool findDistance = true;

    public float speed = 5;
    public bool move = false;
    public int moveStep = 0; 

    // Start is called before the first frame update
    void Awake()
    {

        if (instance == null)
            instance = this;
 
    }

     
    void Update()
    {

        if (MapEditor.instance?.currentMode == MapEditor.Mode.player || MapEditor.instance == null)
        {

            //Move player to grid x,y;
            if (Input.GetMouseButtonDown(0))
            {

                if (GridManager.IsPointerOverUIObject()) return; 
               
                RaycastHit hitInfo;
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, 1 << LayerMask.NameToLayer("Grid"));
                if (hit && !move)
                {

                    endX = hitInfo.collider.GetComponent<GridStats>().x;
                    endY = hitInfo.collider.GetComponent<GridStats>().y;
                    findDistance = true;
                   

                }

            }


            //Draw player path.
            if (path.Count != 0)
            {

                for (var i = 1; i < path.Count; i++)
                {
                    Debug.DrawLine(path[i - 1].transform.position, path[i].transform.position);
                }

                Debug.DrawLine(path[0].transform.position, path[1].transform.position, Color.green);
            }


            if (findDistance)
            {

                SetDistance();
                SetPath();

            }

            MoveIt(transform);

        }
    }      
        void SetDistance()
        {
            //in open world instead of going through all grids, take x,y size grid around the player and go from there

            InitialSetUp();

            int x = Mathf.RoundToInt(startX);
            int y = Mathf.RoundToInt(startY);
           // int[] testArray = new int[GridManager.instance.rows * GridManager.instance.columns];

            for (int step = 1; step < GridManager.instance.rows * GridManager.instance.columns; step++)
            {
                foreach (GridStats gridElement in GridManager.instance.gridArray)
                {
                    if (gridElement)
                    {
                        if (gridElement.visited == step - 1)
                        {
                            TestFourDirections(gridElement.x, gridElement.y, step);
                        }
                    }
                }
            }

            moveStep = path.Count - 1;
            findDistance = false;

        }

        void InitialSetUp()
        {
            foreach (GridStats gridElement in GridManager.instance.gridArray)
            {
                if (gridElement)
                    gridElement.visited = -1;
            }
            GridManager.instance.gridArray[Mathf.RoundToInt(startX), Mathf.RoundToInt(startY)].visited = 0;
        }

        void SetPath()
        {
            int step;
            int x = endX;
            int y = endY;
            List<GridStats> tempList = new List<GridStats>();
            path.Clear();
            if (GridManager.instance.gridArray[endX, endY] && GridManager.instance.gridArray[endX, endY].visited > 0)
            {
                path.Add(GridManager.instance.gridArray[x, y]);
                step = GridManager.instance.gridArray[x, y].visited - 1;
            }
            else
            {
                print("Can't reach the desired location");
                return;
            }

            for (int i = step; step > -1; step--)
            {

                if (TestDirection(x, y, step, 1))
                {
                    tempList.Add(GridManager.instance.gridArray[x, y + 1]);
                    // y = y + 1;
                }
                if (TestDirection(x, y, step, 2))
                {
                    tempList.Add(GridManager.instance.gridArray[x + 1, y]);
                    // x = x + 1;
                }
                if (TestDirection(x, y, step, 3))
                {
                    tempList.Add(GridManager.instance.gridArray[x, y - 1]);
                    // y = y - 1;

                }
                if (TestDirection(x, y, step, 4))
                {
                    tempList.Add(GridManager.instance.gridArray[x - 1, y]);
                    //  x = x - 1;
                }


                GridStats tempObj = FindClosest(GridManager.instance.gridArray[endX, endY].transform, tempList);
                


                path.Add(tempObj);


                x = tempObj.x;
                y = tempObj.y;
                tempList.Clear();

            }

           

            if (path.Count > 0)
            {

                path.Reverse();

                for (var i = 0; i < path.Count; i++)
                {

                    
                    if (path[i].hasBlockedDirections)
                    {
                    //do nothing
    
                    }
                    else if (i + 2 < path.Count)
                    {

                    if (Vector3.Distance(path[i].transform.position, path[i + 2].transform.position) < 2)

                        if (path[i].hasBlockedDirections || path[i + 2].hasBlockedDirections)
                        {

                        }
                        else {  
                            path.Remove(path[i + 1]);
                        }
                                

                    }
                }

                path.Reverse();
                moveStep = path.Count - 1;

            }




        }

        void TestFourDirections(int x, int y, int step)
        {

            if (TestDirection(x, y, -1, 1))
                SetVisited(x, y + 1, step);
            if (TestDirection(x, y, -1, 2))
                SetVisited(x + 1, y, step);
            if (TestDirection(x, y, -1, 3))
                SetVisited(x, y - 1, step);
            if (TestDirection(x, y, -1, 4))
                SetVisited(x - 1, y, step);

        }


        void MoveIt(Transform obj)
        {
            int step = moveStep;
            if (step > -1 && path.Count > 0)
            {
                //print("step: "+step);
                move = true;
                obj.position = Vector3.MoveTowards(obj.position, path[step].transform.position, speed * Time.deltaTime);
                Vector3 targetPostition = new Vector3(path[step].transform.position.x,
                                            obj.transform.position.y,
                                            path[step].transform.position.z);
                obj.transform.LookAt(targetPostition);

                float dist = Vector3.Distance(obj.transform.position, path[step].transform.localPosition);


                //print(dist);
                if (dist < .1f)
                {
                    startX = endX;
                    startY = endY;
                    moveStep = moveStep - 1;
                    move = false;
                }
            }
        }


        bool TestDirection(int x, int y, int step, int direction)
        {
            //int direction tells which case to use. 1 is up, 2 is to the right, 3 is bottom, 4 is to the left, 5 is check self. 6 is up right , 7 is up left , 8 is bottom right , 9 is bottom left;
            switch (direction)
            {

                case 4:
                    if (x - 1 > -1 && GridManager.instance.gridArray[x - 1, y] && GridManager.instance.gridArray[x - 1, y].visited == step && !GridManager.instance.gridArray[x - 1, y].blockedDirections.ContainsKey(2) && !GridManager.instance.gridArray[x, y].blockedDirections.ContainsKey(4))
                        return true;
                    else
                        return false;


                case 3:
                    if (y - 1 > -1 && GridManager.instance.gridArray[x, y - 1] && GridManager.instance.gridArray[x, y - 1].visited == step && !GridManager.instance.gridArray[x, y - 1].blockedDirections.ContainsKey(1) && !GridManager.instance.gridArray[x, y].blockedDirections.ContainsKey(3))
                        return true;
                    else
                        return false;
                case 2:



                    if (x + 1 < GridManager.instance.columns && GridManager.instance.gridArray[x + 1, y] && GridManager.instance.gridArray[x + 1, y].visited == step && !GridManager.instance.gridArray[x + 1, y].blockedDirections.ContainsKey(4) && !GridManager.instance.gridArray[x, y].blockedDirections.ContainsKey(2))
                        return true;
                    else
                        return false;



                case 1:
                    if (y + 1 < GridManager.instance.rows && GridManager.instance.gridArray[x, y + 1] && GridManager.instance.gridArray[x, y + 1].visited == step && !GridManager.instance.gridArray[x, y + 1].blockedDirections.ContainsKey(3) && !GridManager.instance.gridArray[x, y].blockedDirections.ContainsKey(1))
                        return true;
                    else
                        return false;

            }
            return false;
        }

        void SetVisited(int x, int y, int step)
        {
            if (GridManager.instance.gridArray[x, y] != null && GridManager.instance.gridArray[x, y].objectInLowerGrid == false && GridManager.instance.gridArray[x, y].bothSlotObjectInGrid == false) 
                GridManager.instance.gridArray[x, y].visited = step;
        }



        GridStats FindClosest(Transform targetLocation, List<GridStats> list)
        {
            float currentDistance = GridManager.instance.scale * GridManager.instance.rows + GridManager.instance.scale * 2 * GridManager.instance.columns;
            int indexNumber = 0;
            //print("test " + list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                //print("index" + indexNumber + "    " + Vector3.Distance(targetLocation.position, list[i].transform.position));
                if (Vector3.Distance(targetLocation.position, list[i].transform.position) < currentDistance)
                {
                    currentDistance = Vector3.Distance(targetLocation.position, list[i].transform.position);

                    indexNumber = i;
                }
            }
            return list[indexNumber];
        }



    }


