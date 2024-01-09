using UnityEngine;
using UnityEngine.SceneManagement;

/* An Item that can be equipped to increase armor/damage. */


[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Equipment")]
public class Equipment : Item {
	
    public GameObject itemPrefab;
	public Mesh mesh;
    public Material mat;

    // Called when pressed in the inventory
    public override void Use ()
	{

    }






}

