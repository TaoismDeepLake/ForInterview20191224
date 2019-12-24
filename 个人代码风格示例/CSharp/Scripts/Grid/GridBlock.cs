using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBlock : MonoBehaviour {

	public int posX = 1, posY = 1, size = 1;

	public List<ItemBase> items = new List<ItemBase>();

	public List<MapBlock> breakingList = new List<MapBlock>();

	public void ActivateBlockItems()
	{
		foreach(ItemBase K in items)
		{
			K.OnCapture();
		}

	}

	/// <summary>
	/// Adds related blocks to the breaking list in the controller.
	/// Also disables the four lines around this unit(if existed).
	/// </summary>
	public void BreakingRelatedBlocks()
	{
		foreach(MapBlock K in breakingList)
		{
			if (!MapController.instance.breakingList.Exists((MapBlock obj) => obj == K))
			{
				MapController.instance.breakingList.Add(K);
			}
		}
		//four surrounding lines
		DisableLine(posX, posY, true);
		DisableLine(posX, posY, false);
		DisableLine(posX, posY+1, true);
		DisableLine(posX+1, posY, false);
	}


	/// <summary>
	/// Disables the line if that line exists.
	/// This way nobody captured that line. It is simply disabled. 
	/// </summary>
	/// <param name="x">The x coordinate.[1,+INF)</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="isHori">If set to <c>true</c> is hori.</param>
	public void DisableLine(int x, int y, bool isHori)
	{
		if (null != MapController.instance.lines[x-1 ,y-1,isHori ? 0 : 1])
		{
			MapController.instance.lines[x-1 ,y-1,isHori ? 0 : 1].isVacant = false;
		}
	}


}
