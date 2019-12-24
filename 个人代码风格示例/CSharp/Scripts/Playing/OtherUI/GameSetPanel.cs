using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSetPanel : MonoBehaviour {

	public static GameSetPanel instance;

	[SerializeField]GameObject winnerPanel, drawPanel;
	[SerializeField]Image winnerIcon;
	[SerializeField]Image[] drawIcons;

	void Awake()
	{
		instance = this;
	}

	public void Win(Color winnerColor)
	{
		winnerPanel.SetActive(true);
		winnerIcon.color = winnerColor;
	}

	public void Draw()
	{
		drawPanel.SetActive(true);
		drawIcons[0].color = GameController.instance.players[0].playerColor.color;
		drawIcons[1].color = GameController.instance.players[1].playerColor.color;
	}
}
