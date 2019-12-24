using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemPositionType{
	Point,
	Line,
	Block,
	Other
}

public enum ItemWorkType{
	OneUse,//most would be this type
	Permanent//like anti-thunder and magnet
}

/// <summary>
/// The base class of items
/// </summary>
public class ItemBase : MonoBehaviour {

	public ItemPositionType positionType = ItemPositionType.Block;
	public ItemWorkType workType = ItemWorkType.OneUse;

	public int posX = 1, posY = 1;
	/// <summary>
	/// Only valid when this is a item on a line, like elec-current.
	/// </summary>
	public bool isHorizontal = false;

	public virtual void OnCapture(){
		
	}

	public void Start()
	{
		AttachToBlock();
	}

	public void AttachToBlock(){
		GridBlock gb = GridController.instance.GetBlock(posX, posY);
		if (gb)
		{
			gb.items.Add(this);
			ItemManager.SetPosition(transform, posX, posY);
		}
		else
		{
			Debug.LogError("Cannot found the responding grid block to attach.");
		}
	}
}
