using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScorePlus : ItemBase {

	public int scoreDelta = 1;


	public bool used = false;

	GameController gc;

	[SerializeField]UnityEngine.UI.Text scoreText = null;

	public void SetValue(int _value)
	{
		if (_value > 0)
		{
			scoreText.text = "+" + _value.ToString();
		}
		else
		{
			scoreText.text = _value.ToString();
		}

		scoreDelta = _value;
	}

	public override void OnCapture()
	{
		if (!used)
		{
			gc = GameController.instance;

			gc.scoreDelta += scoreDelta;
			//gc.players[GameController.currentPlayer].AddScore(deltaScore);
			used = true;
		}

	}

}
