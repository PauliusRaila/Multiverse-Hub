using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralCube : MonoBehaviour
{

    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }


    // Start is called before the first frame update
    void Start()
    {
        MakeCube();
        UpdateMesh();
    }
    

    void MakeCube() {

        vertices = new List<Vector3>();
        triangles = new List<int>();

      //  for (int i = 0; i < 6; i++)
      //  {
      //       MakeFace(i);
      //  }

         // MakeFace(0);
          MakeFace(1); //side face
         // MakeFace(2);
          MakeFace(3); //side face
          MakeFace(4); // top
          MakeFace(5);

    }

    void MakeFace(int dir) {
        vertices.AddRange(CubeMeshData.faceVertices(dir));
        int vCount = vertices.Count;

        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 1);
        triangles.Add(vCount - 4 + 2);

        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 2 );
        triangles.Add(vCount - 4 + 3);
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}