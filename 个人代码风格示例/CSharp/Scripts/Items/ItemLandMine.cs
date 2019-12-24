using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLandMine : ItemBase {

	public int damage = 3;

	public bool used = false;

	GameController gc;

	[SerializeField]UnityEngine.UI.Text scoreText = null;
	[SerializeField]GameObject displayParent = null;
	[SerializeField]GameObject explosion = null;

	public int ownerID = 0;

	public void SetValue(int score)
	{
		if (score > 0)
		{
			scoreText.text = "+" + score.ToString();
		}
		else
		{
			scoreText.text = score.ToString();
		}

		damage = score;
	}

	public override void OnCapture()
	{
		if (!used)
		{
			gc = GameController.instance;

			if (GameController.gameMode == GameMode.LocalPVP)
			{
				if (gc.turnOwnerID != ownerID)
				{
					Explode();
					used = true;
				}
			}
			else if (GameController.gameMode == GameMode.NetPVP)
			{
				if (gc.turnOwnerID != ownerID)
				{
					Explode();
					used = true;
				}
			}

			//gc.players[GameController.currentPlayer].AddScore(deltaScore);
		}
	}


	public void Explode()
	{
		if (GameController.gameMode == GameMode.LocalPVP)
		{
			gc.players[gc.turnOwnerID].AddScore(-damage);	
		}
		else if (GameController.gameMode == GameMode.NetPVP)
		{
			//whoever owns mine handles this call
			if (OnlinePlayer.myID == ownerID)
				OnlinePlayer.localPlayer.CmdScore(gc.turnOwnerID, -damage);
		}


		//create effect
		explosion.SetActive(true);
		Show();
	}

	public void Hide()
	{
		displayParent.SetActive(false);
	}

	public void Show()
	{
		displayParent.SetActive(true);
	}

	public void ShowTemp()
	{
		StartCoroutine(ShowCoro());
	}

	IEnumerator ShowCoro()
	{
		Show();

		yield return new WaitForSeconds(5);

		Hide();
	}
}
