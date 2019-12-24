using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLine : MonoBehaviour {

	public int posX = 1, posY = 1;
	public bool isHorizontal = true;

	public bool isVacant = true;
	/// <summary>
	/// The owner ID. -1 when unowned.
	/// </summary>
	public int ownerID = -1;

	public List<ItemBase> items = new List<ItemBase>();

	public List<MapBlock> blocksRelated = new List<MapBlock>();

	public void SetHorizontal(bool _isHorizontal){
		isHorizontal = _isHorizontal;
		if (!isHorizontal)
		{
			transform.GetChild(0).Rotate(new Vector3(0, 0, 90f));
		}
	}

	public void BeDrawn()
	{
		isVacant = false;
		foreach(MapBlock K in blocksRelated)
		{
			K.OccupyLine();
		}
	}
}
