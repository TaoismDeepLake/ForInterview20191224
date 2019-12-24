using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScoreMultiply : ItemBase {

	public int scoreFactor = 2;

	public bool used = false;

	GameController gc;

	[SerializeField]UnityEngine.UI.Text scoreText;

	public void SetValue(int score)
	{
		if (score > 0)
		{
			scoreText.text = "x" + score.ToString();
		}
		else
		{
			scoreText.text = string.Format("x({0})", score);
		}

		scoreFactor = score;
	}

	public override void OnCapture()
	{
		if (!used)
		{
			gc = GameController.instance;

			gc.scoreMultiplier *= scoreFactor;

			used = true;
		}
	}

}
