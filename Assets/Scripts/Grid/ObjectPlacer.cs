using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    private DrawGrid grid;

    private void Awake()
    {
        grid = FindObjectOfType<DrawGrid>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hitInfo))
            {
                PlaceCubeNear(hitInfo.point);
            }
        }
    }

    private void PlaceCubeNear(Vector3 clickPoint)
    {
        var finalPosition = grid.GetNearestPointOnGrid(clickPoint);
        GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = finalPosition;

        //GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = nearPoint;
    }
}