using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBlock : MonoBehaviour {

	public int posX = 1, posY = 1, size = 1;

	public List<ItemBase> items = new List<ItemBase>();

	//not used
	public List<MapLine> outsideLines = new List<MapLine>();

	public int outsideVacantLineCount = 0 ;
	public bool isVacant = true;

	public void SetSize(int _size)
	{
		if (_size <= 0)//could do more check
		{
			Debug.LogError("Trying to set a wrong size");
			return;
		}

		size = _size;

		transform.GetChild(0).localScale = new Vector3(size, size, size);
		float halfSize = (float)size/2f;
		transform.GetChild(0).localPosition = new Vector3(halfSize, -halfSize, halfSize);

		outsideVacantLineCount = 4 * size;

		//set breaking
		for(int x = posX; x <= posX + size - 1; x++)
			for(int y = posY; y <= posY + size - 1; y++)
			{
				GridBlock targetBlock =GridController.instance.GetBlock(x, y);

				if (null != targetBlock)
				{
					Debug.LogFormat("Added {0} to {1}",this,targetBlock);
					targetBlock.breakingList.Add(this);
				}
			}
	}

	public void OccupyLine()
	{
		if (isVacant)
		{
			outsideVacantLineCount--;
			if (0 == outsideVacantLineCount)
			{
				//TODO
				MapController.instance.AddCapturingUnitsFor(this);
				//isVacant = false;
				//MapController.instance.vacantBlockCount--;
				Debug.Log(name + "is captured.");
			}
		}
	}
}
