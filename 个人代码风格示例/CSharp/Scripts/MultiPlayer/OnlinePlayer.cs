using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OnlinePlayer : NetworkBehaviour {

	public static OnlinePlayer localPlayer;
	//public bool isServer = false;
	public static int myID = 1;


	void Start()
	{
		//NetworkManager.singleton.OnClientDisconnect

	}

	public override void OnStartLocalPlayer()
	{
		NetworkMenuUI.instance.panel.SetActive(false);
		base.OnStartLocalPlayer();
		if (isLocalPlayer)
		{
			localPlayer = this;
			myID = isServer ? 0 : 1;

			Debug.LogFormat("MyID = {0}", myID);
		}
		//isHost = isServer;
		//Debug.Log("isServer = " + isHost.ToString());
	}

	void ClientDisconnect()
	{
		localPlayer = null;
		Debug.LogError("DC");
	}

	[Command]
	public void CmdSelectPoint(int x, int y)
	{
		
	}

	[ClientRpc]
	public void RpcDrawLine(int x, int y, bool isHori)
	{
		GameController.instance.DrawLine(x, y, isHori);
	}
	[Command]
	public void CmdDrawline(int x, int y, bool isHori)
	{
		RpcDrawLine(x, y, isHori);
	}

	[ClientRpc]
	public void RpcCreateMap(string str, bool useMine)
	{
		LevelPasser.CreatePasser(str, useMine);
		GridController.instance.ApplyLevelPasser();
		//GameController.instance.gamePhase = GamePhase.DrawLine;
	}

	[Command]
	public void CmdCreateMine(int x, int y, int owner, int damage)
	{
		RpcCreateMine(x,y,owner,damage);
	}
	[ClientRpc]
	public void RpcCreateMine(int x, int y, int owner, int damage)
	{
		if (owner != myID)
		{
			LandMineDeployment.instance.DeployHidden(x,y,owner,damage);
		}
		//if owner == myID, the mine will already be deployed locally.
	}


	[Command]
	public void CmdOccupy(int player, int x, int y)
	{
		
	}
	[ClientRpc]
	public void RpcOccupy(int player, int x, int y)
	{
		
	}

	[Command]
	public void CmdScore(int player, int score)
	{
		RpcScore(player,score);
	}
	[ClientRpc]
	public void RpcScore(int player, int score)
	{
		GameController.instance.players[player].AddScore(score);
	}
}
