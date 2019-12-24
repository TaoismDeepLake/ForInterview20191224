using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class NetworkMenuUI : MonoBehaviour {

	[SerializeField]InputField ipText ;
	public GameObject panel;
	public static NetworkMenuUI instance;

	bool useBroadcast = true;

	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		OnConnected += Connected;

		if (CustomBroadcast.instance == null)
		{
			useBroadcast = false;
			return;
		}

		CustomBroadcast.instance.Initialize();
		CustomBroadcast.instance.StartAsClient();
	}

	public void HostLAN()
	{
		NetworkManager.singleton.StartHost();
		GameController.gameMode = GameMode.NetPVP;
	}

	public void JoinLAN()
	{
		if (ipText.text != "")
		{
			NetworkManager.singleton.networkAddress = ipText.text;
		}
		NetworkManager.singleton.StartClient();
		GameController.gameMode = GameMode.NetPVP;
	}

	//----
	NetworkClient myClient;

	public void SetupClient()
	{
		myClient = new NetworkClient();
		myClient.RegisterHandler(MsgType.Connect, OnConnected);     
		myClient.Connect("127.0.0.1", 4444);
		isAtStartup = false;
	}

	// Create a local client and connect to the local server
	public void SetupLocalClient()
	{
		myClient = ClientScene.ConnectLocalServer();
		myClient.RegisterHandler(MsgType.Connect, OnConnected);     
		isAtStartup = false;
	}

	bool isAtStartup = true;

	NetworkMessageDelegate OnConnected;



	void Connected(NetworkMessage message)
	{
		Debug.Log("A server has connected");
	}




}
