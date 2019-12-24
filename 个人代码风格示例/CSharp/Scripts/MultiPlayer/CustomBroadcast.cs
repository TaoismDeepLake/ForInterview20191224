using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomBroadcast : NetworkDiscovery{

	public static CustomBroadcast instance;


	void Awake()
	{
		instance = this;
		Initialize();

	}


//	public override void OnReceivedBroadcast (string fromAddress, string data)
//	{
//		base.OnReceivedBroadcast (fromAddress, data);
//
//		GameObject g = Instantiate(CustomManager.instance.prefabIP, 
//			CustomManager.instance.gridParent);
//		g.GetComponent<Text>().text = fromAddress;
//	}
}
