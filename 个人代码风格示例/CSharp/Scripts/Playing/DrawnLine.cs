using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawnLine : MonoBehaviour {

	public bool isHorizontal = true;

	/// <summary>
	/// Makes the line horizontal.
	/// Usually you call with the argument = true;
	/// </summary>
	/// <param name="_isHorizontal">If set to <c>true</c> is horizontal.</param>
	public void MakeHorizontal(bool _isHorizontal = true)
	{
		isHorizontal = _isHorizontal;
		if (!isHorizontal)
		{
			transform.GetChild(0).Rotate(new Vector3(0, 0, 90f));
		}
	}

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
