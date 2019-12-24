using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPoint : MonoBehaviour {

	public int posX = 1, posY = 1;

	public bool isUsed = false;

	public List<ItemBase> items = new List<ItemBase>();

	void OnDrawGizmos()
	{
		if (!isUsed)
		{
			Gizmos.DrawIcon(transform.position, "Disabled.png");
		}
	}

}
