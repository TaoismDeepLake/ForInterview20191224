using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour {

	public static ItemManager instance
	{
		get{
			if (_instance)
			{
				return _instance;
			}
			else
			{
				_instance = FindObjectOfType<ItemManager>();
				return _instance;
			}
		}
		set{
			_instance = value;
		}
	}
	static ItemManager _instance;


	[SerializeField]GameObject scorePlusPrefab = null;
	[SerializeField]GameObject scoreMultiPrefab = null;

	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		//for testing
		//CreatePlusScore(10, 2, 2);
	}

	public static Vector3 offset = new Vector3(0.5f, -0.5f, 0);

	/// <summary>
	/// Creates a plus score item.
	/// </summary>
	/// <param name="value">Value.</param>
	/// <param name="posX">Position x. [1, +INF)</param>
	/// <param name="posY">Position y. [1, +INF)</param>
	public static void CreatePlusScore(int value, int posX, int posY)
	{
		ItemScorePlus item = (ItemScorePlus)CreatePrefabAtBlock(instance.scorePlusPrefab, posX, posY);
		item.SetValue(value);

		Debug.LogFormat("Created a plus-score({0}) @({1},{2}).",value, posX, posY);
	}

	/// <summary>
	/// Creates a score-multiplier item.
	/// </summary>
	/// <param name="value">Value.</param>
	/// <param name="posX">Position x. [1, +INF)</param>
	/// <param name="posY">Position y. [1, +INF)</param>
	public static void CreateMultiScore(int value, int posX, int posY)
	{
		ItemScoreMultiply item = (ItemScoreMultiply)CreatePrefabAtBlock(instance.scoreMultiPrefab, posX, posY);
		item.SetValue(value);

		Debug.LogFormat("Created a multi-score(x{0}) @({1},{2}).",value, posX, posY);
	}

	public static ItemBase CreatePrefabAtBlock(GameObject prefab, int posX, int posY)
	{
		ItemBase item = Instantiate(prefab, instance.transform)
			.GetComponent<ItemBase>();

		item.transform.position = GridController.instance.GetBlock(posX, posY).transform.position 
			+ offset * GridController.instance.unitDist;

		item.posX = posX;
		item.posY = posY;

		return item;
	}

	public static void SetPosition(Transform target, int posX, int posY)
	{
		if (GridController.instance.GetBlock(posX, posY))
		{
			target.position = GridController.instance.GetBlock(posX, posY).transform.position 
				+ offset * GridController.instance.unitDist;
		}
	}

}
