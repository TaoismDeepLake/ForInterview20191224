using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPoint : MonoBehaviour {

	/// <summary>
	/// The position y. starts from 1
	/// </summary>
	/// <summary>
	/// The position x. starts from 1
	/// </summary>
	public int posX = 1, posY = 1;

	/// <summary>
	/// The items.
	/// </summary>
	public List<ItemBase> items = new List<ItemBase>();

	Light halo;

	void Start()
	{
		halo = GetComponent<Light>();
	}

	/// <summary>
	/// Inherits the value of the grid point.
	/// </summary>
	/// <param name="gp">Gp.</param>
	public void UseValueOf(GridPoint gp)
	{
		posX = gp.posX;
		posY = gp.posY;
		items = gp.items;
	}

	void OnMouseUpAsButton()
	{
		if (GameController.instance.gamePhase != GamePhase.DrawLine)
			return;


		//Debug.Log(gameObject.name + " was clicked.");

		if (this == GameController.instance.selected)
		{
			return;
		}

		//Debug.Log(GameController.instance.adjacent.Exists((MapPoint p) => this == p));

		//When clicked on an adjacent point
		if (GameController.instance.adjacent.Exists((MapPoint p) => this == p))
		{
			Debug.Log("[Drawing a line]");


			MapPoint last = GameController.instance.selected;
			//finding the coordinate of the line
			int x = last.posX < posX ? last.posX : posX;
			int y = last.posY < posY ? last.posY : posY;
			bool isHori = last.posY == posY;

			//Draw a line here. if at the turn.
			if (GameController.gameMode == GameMode.LocalPVP)//in local PvP, always true.
			{
				//draw
				GameController.instance.DrawLine(x, y, isHori);
				return;
			}
			else if (GameController.gameMode == GameMode.NetPVP)
			{
				if (GameController.instance.turnOwnerID == OnlinePlayer.myID)
				{
					OnlinePlayer.localPlayer.CmdDrawline(x, y, isHori);
				}

				//GameController.instance.SelectPoint(this);
				//return;
			}
		}

		GameController.instance.SelectPoint(this);
	}

	/// <summary>
	/// Undo any highlighting.
	/// </summary>
	public void UnHighlight()
	{
		halo.enabled = false;
	}

	/// <summary>
	/// Highlights as selected.
	/// </summary>
	public void HighlightAsSelected()
	{
		halo.range = 1.5f;
		halo.enabled = true;
	}

	/// <summary>
	/// Highlights as selectable.
	/// </summary>
	public void HighlightAsSelectable()
	{
		halo.range = 1f;
		halo.enabled = true;
	}

	public static bool operator == (MapPoint a, MapPoint b)
	{
		bool isNullA, isNullB;
		isNullA = object.Equals(a, null);
		isNullB = object.Equals(b, null);
		if (isNullA != isNullB)
			return false;

		if (isNullA && isNullB)
			return true;
		
		return a.posX == b.posX && a.posY == b.posY;
	}

	public static bool operator != (MapPoint a, MapPoint b)
	{
		

		return !(a == b);
	}

}
