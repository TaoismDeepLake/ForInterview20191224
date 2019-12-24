using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMineParent : ItemBase {

	[SerializeField]MeshRenderer mr = null;

	void Awake()
	{
		Debug.Log(GameController.gameMode);

		if (GameController.gameMode == GameMode.LocalPVP)
		{
			mr.sharedMaterial = GameController.instance.players[GameController.currentPlayer].playerColor;	
		}
		else if (GameController.gameMode == GameMode.NetPVP)
		{
			mr.sharedMaterial = GameController.instance.players[OnlinePlayer.myID].playerColor;	
		}
	}

}
