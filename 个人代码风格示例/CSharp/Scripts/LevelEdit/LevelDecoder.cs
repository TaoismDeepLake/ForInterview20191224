using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class LevelDecoder{

	public static bool dataReady = false;

	/// <summary>
	/// The protocol.
	/// 0 = standard "MP" protocol, uppercase hexadecimal string
	/// </summary>
	public static int protocol = 0;

	public static int range = 10;
	public static bool[] vertices;

	/// <summary>
	/// Decodes the string.
	/// </summary>
	/// <returns>
	/// 	0 if successfully parsed.
	/// 	1 if unknown protocol.
	/// 	2 if map shape error.
	/// </returns>
	/// 
	public static int CheckString(string s)
	{
		dataReady = false;
		if ("" == s)
			s = s.Insert(0, "0");

		s = s.Replace(" ","");
		s = s.Replace("\n","");

		if (s.Length < 5)
			s = s.Insert(0, "MP010");
		//auto parses very short maps



		//check protocol
		switch (s.Substring(0,2))
		{
		case "MP":
			protocol = 0;

			if (int.TryParse(s.Substring(2,3), out range))
			{//loads the size 
				//success!	
			}
			else{
				return 1;//unknown protocal
			}
				
			break;

		//Saving for future update.

		default:
			for (int i = 0; i <= 4; i ++)
			{
				if (!isHexChar(s[i]))
				{
					protocol = -1;
					return 1;//unknown protocol
				}
			}

			//MP010 protocal, header omitted
			protocol = 0;
			range = 10;
			s = s.Insert(0, "MP010");
			//adds header
			break;

		}

		switch(protocol)
		{
		case 0:
			HandleMPProtocol(s.Substring(5,s.Length-5));

			break;

		default:
			Debug.LogError("Unknown protocol");
			break;
		}

		//success
		return 0;
	}

	/// <summary>
	/// Checks wether the string fits the protocol
	/// </summary>
	/// <returns>The MP protocol.</returns>
	/// <param name="s">S.</param>
	static int HandleMPProtocol(string s)
	{
		int vertexCount = range * range + 4;
		//just make sure it doesn't overflow.
		//some will be useless, though.
		vertices = new bool[vertexCount];

		//wether vertices are used
		string basic;
		string toolList = "";
		bool hasToolList = false;
		int cutIndex = s.IndexOf('#');

		if (cutIndex != -1)
		{
			//has tool list
			basic = s.Substring(0, cutIndex);

			int toolListIndex = cutIndex + 5 - cutIndex % 5;//the first char of the tool list
			if (toolListIndex > s.Length - 1)
			{
				//means it's like 5AB##, with no tool list following
				hasToolList = false;
			}
			else{
				hasToolList = true;
				toolList = s.Substring(toolListIndex, s.Length - toolListIndex);

//				Debug.LogWarning("tool list decoding not implemented");
				//tool list decoding not implemented
			}
		}
		else{
			//no tool list
			basic = s;
		}


		foreach(char c in basic)
		{
			if (!isHexChar(c))
				return 2;//backbone error
		}

		int boneArea = range * range;
		int boneStringLen = boneArea / 4 + 
			(0 == boneArea % 4 ? 0 : 1);//number of chars, rounded up;
		
		if (basic.Length < boneStringLen)//make the length fit. 
			basic = basic.PadRight(boneStringLen,'0');

		Debug.LogFormat("basic[{0}] = {1}", basic.Length, basic);
//		Debug.LogFormat("boneStrLen = {0}", boneStringLen);


		for(int i = 0; i < boneStringLen; i++)
		{
			int local = valHexChar(basic[i]);

			vertices[4 * i    ] = (local & 8) != 0;
			vertices[4 * i + 1] = (local & 4) != 0;
			vertices[4 * i + 2] = (local & 2) != 0;
			vertices[4 * i + 3] = (local & 1) != 0;
		}

		//destroy previous
		ItemBase[] items = GameObject.FindObjectsOfType<ItemBase>();
		for(int i = 0; i < items.Length; i++)
		{
			GameObject.DestroyImmediate(items[i].gameObject);
		}

		if (hasToolList)
			DecodeToolList(toolList);

		dataReady = true;//if no tools are needed

		//checking success;
		return 0;

	}

	static void DecodeToolList(string toolList)
	{
		while (true)
		{
			if (toolList.Length < 5 && toolList.Length != 0)
			{
				Debug.LogError("Tool List not finished.");
				break;
			}
			else
			{
				switch (toolList.Substring(0,2))
				{
				case "PL":
					//plus score item
					ItemManager.CreatePlusScore(valHexChar(toolList[2]) + 1,
						valHexChar(toolList[3]) + 1,valHexChar(toolList[4]) + 1);

					break;

				case "MN":
					//plus score item
					ItemManager.CreatePlusScore(-valHexChar(toolList[2]) - 1,
						valHexChar(toolList[3]) + 1,valHexChar(toolList[4]) + 1);

					break;

				case "MT":
					//plus score item
					ItemManager.CreateMultiScore(valHexChar(toolList[2]),
						valHexChar(toolList[3]) + 1,valHexChar(toolList[4]) + 1);

					break;


				default:
					Debug.LogError("Unknown item:" + toolList.Substring(0,2));
					break;

				}

				if (5 == toolList.Length)
				{
					Debug.Log("Item decoding finished successfully.");
					break;
				}	
				toolList = toolList.Substring(5);
			}
		}

	}

	static bool isHexChar(char c)
	{
		return ((c <= 'F') && (c >= 'A'))
			|| ((c <= '9') && (c >= '0'));
	}

	/// <summary>
	/// Values the hex char.
	/// Assumming all input as valid
	/// </summary>
	/// <returns>The hex char.</returns>
	/// <param name="c">C.</param>
	static int valHexChar(char c)
	{
		if ((c <= 'F') && (c >= 'A'))
			return c - 'A' + 10;

		return c - '0';
			 
	}
}
