using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawnBlock : MonoBehaviour {

	//slower. not recommended.
	public void SetColor(Color c)
	{
		transform.GetChild(0).GetComponent<MeshRenderer>().material
		=new Material(transform.GetChild(0).GetComponent<MeshRenderer>().material);

		transform.GetChild(0).GetComponent<MeshRenderer>().material.color = c;


	}

	public void SetColor(Material mat)
	{
		transform.GetChild(0).GetComponent<MeshRenderer>().material = mat;
	}
}
