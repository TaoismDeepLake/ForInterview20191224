using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandMineDeployment : MonoBehaviour {

	[SerializeField]GameObject landMineParent = null;
	[SerializeField]GameObject landMinePrefab = null;

	[SerializeField]UnityEngine.UI.Image buttonImage = null;
	[SerializeField]GameObject minePanel = null;
	[SerializeField]GameObject mineWaitingPanel = null;

	ItemBase posMarker;

	bool otherPlayerReady = false;
	bool thisPlayerReady = false;

	public static LandMineDeployment instance;

	void Awake()
	{
		instance = this;
	}

	public void BeginSetting()
	{
		GameController.instance.useLandMine = true;

		minePanel.SetActive(true);
		posMarker = Instantiate(landMineParent).GetComponent<ItemBase>();
		posMarker.AttachToBlock();
		GameController.instance.gamePhase = GamePhase.Presetting;

		if (GameController.gameMode == GameMode.LocalPVP)
		{
			buttonImage.color = GameController.instance.players[GameController.currentPlayer].playerColor.color;
		}
		else if (GameController.gameMode == GameMode.NetPVP)
		{
			buttonImage.color = GameController.instance.players[OnlinePlayer.myID].playerColor.color;
		}


		MessageManager.CreateMessage("地雷玩法启用，请布雷。");
	}

	public void MoveX(int dx)
	{
		int newX = posMarker.posX + dx;
		if (GridController.instance.GetBlock(newX, posMarker.posY))
		{
			posMarker.posX = newX;
			posMarker.AttachToBlock();
		}
	}

	public void MoveY(int dy)
	{
		int newY = posMarker.posY + dy;
		if (GridController.instance.GetBlock(posMarker.posX, newY))
		{
			posMarker.posY = newY;
			posMarker.AttachToBlock();
		}
	}

	public void Deploy(int damage = 3)
	{
		Deploy(GameController.currentPlayer, damage);
	}

	public void Deploy(int playerIndex, int damage = 3)
	{
		int posX = posMarker.posX;
		int posY = posMarker.posY;

		ItemLandMine item = (ItemLandMine)ItemManager.CreatePrefabAtBlock(
			landMinePrefab,
			posX,
			posY);

		item.damage = damage;
		item.ownerID = playerIndex;

		Destroy(posMarker.gameObject);

		if (GameController.gameMode == GameMode.LocalPVP)
		{
			GameController.instance.turnOwnerID ++;

			if (GameController.currentPlayer >= GameController.instance.playerCount)
			{
				GameController.instance.turnOwnerID = 0;
				GameController.instance.gamePhase = GamePhase.DrawLine;
				HidePanel();

				MessageManager.CreateMessage("所有玩家布雷完毕，\n游戏开始！");
				foreach(PlayerInstance K in GameController.instance.players)
				{
					K.ShowExamineMineButton();
				}
			}
			else
			{
				BeginSetting();
				MessageManager.CreateMessage("地雷已设置，\n请下一位玩家布雷");
			}
		}
		else if (GameController.gameMode == GameMode.NetPVP)
		{
			//Since this is the place that a player decides where
			//his own mine is depolyed, we only handle this part here.
			OnlinePlayer.localPlayer.CmdCreateMine(posX,posY,OnlinePlayer.myID,damage);
			thisPlayerReady = true;
			//Note that the mine is already settled before the mode has be decided.

			//  In online mode, a player can always see his own mine.
			//No need to use any show mine buttons.
			item.Show();

			if (otherPlayerReady)
			{
				GameController.instance.turnOwnerID = 0;
				GameController.instance.gamePhase = GamePhase.DrawLine;
				HidePanel();

				MessageManager.CreateMessage("所有玩家布雷完毕，\n游戏开始！");
			}
			else
			{
				ShowWaitingPanel(true);
			}
		}
	}

	/// <summary>
	/// Called by the server, when the other player has deployed his mine.
	/// </summary>
	public void DeployHidden(int x, int y, int owner, int damage = 3)
	{
		ItemLandMine item = (ItemLandMine)ItemManager.CreatePrefabAtBlock(
			landMinePrefab,
			x,
			y);

		item.damage = damage;
		item.ownerID = owner;

		MessageManager.CreateMessage("对方已布雷");


		otherPlayerReady = true;
		if (thisPlayerReady)
		{
			ShowWaitingPanel(false);
			GameController.instance.turnOwnerID = 0;
			GameController.instance.gamePhase = GamePhase.DrawLine;
			HidePanel();

			MessageManager.CreateMessage("所有玩家布雷完毕，\n游戏开始！");
		}
	}

	public void HidePanel()
	{
		minePanel.SetActive(false);
	}

	/// <summary>
	/// Turns on or off the waiting panel.
	/// Only in online multiplayer.
	/// </summary>
	/// <param name="showing">If set to <c>true</c> showing.</param>
	public void ShowWaitingPanel(bool showing)
	{
		mineWaitingPanel.SetActive(showing);
	}
}
