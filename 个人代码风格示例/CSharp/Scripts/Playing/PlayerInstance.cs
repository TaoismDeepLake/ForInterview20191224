using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInstance : MonoBehaviour {

	//[SerializeField]GameObject cursorPrefab = null;
	//GameObject cursor = null;
	public Material playerColor = null;
	public int score = 0;
	public int playerIndex;


	public Image icon;
	public Text scoreText;
	public Image indicator;

	[SerializeField]GameObject showMineButton = null;

	void Start()
	{
		if (null == playerColor)
			Debug.LogError("A player has color un-assigned.");

		if (null == icon)
			Debug.LogError("A player has icon image un-assigned.");

		if (null == scoreText)
			Debug.LogError("A player has scoreText un-assigned.");
	
		if (null == indicator)
			Debug.LogError("A player has indicator un-assigned.");

		Init();
	}

	void Init()
	{
		//cursor = Instantiate(cursorPrefab);
		score = 0;
		scoreText.text = score.ToString();
		icon.color = playerColor.color;
	}

	public void PleaseDrawLine()
	{
		//cursor.SetActive(true);
	}

	public void AddScore(int delta)
	{
		//if (GameController.gameMode == GameMode.NetPVP)

		
		score += delta;
		scoreText.text = score.ToString();
	}

	public void LocalAddScore(int delta)
	{
		
	}

	public void SetHighlight(bool x = true)
	{
		indicator.enabled = x;
	}

	[SerializeField]float omega = 15f;
	void Update()
	{
		indicator.transform.Rotate(0,0,omega * Time.deltaTime);
	}

	public void ShowExamineMineButton()
	{
		showMineButton.SetActive(true);
	}

	public void ShowMine()
	{
		if (GameController.instance.gamePhase != GamePhase.DrawLine)
			return;
		//cannot click this while presetting.
		//only useful at local multiplayer

		MessageBox msg = MessageManager.CreateMessage("要查看你的地雷位置3秒吗？");

		msg.buttonB.gameObject.SetActive(true);

		msg.buttonConfirm.onClick.AddListener(ShowMineConfirm);

	}

	public void ShowMineConfirm()
	{
		ItemLandMine[] mines = FindObjectsOfType<ItemLandMine>();

		foreach(ItemLandMine K in mines)
		{
			if (K.ownerID == playerIndex)
			{
				K.ShowTemp();
			}
		}

	}
}
