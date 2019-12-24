using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLine : MonoBehaviour {

	public int posX = 1, posY = 1;
	public bool isHorizontal = true;

	public List<ItemBase> items = new List<ItemBase>();

	public void SetHorizontal(bool _isHorizontal){
		isHorizontal = _isHorizontal;
		if (!isHorizontal)
		{
			transform.GetChild(0).Rotate(new Vector3(0, 0, 90f));
		}
	}
}
