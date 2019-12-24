using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public enum GamePhase
{
	DrawLine,
	ItemA,
	BlockForming,
	Resolving,
	ItemB,
	EndGame,
	Other,
	Presetting
}

public enum GameMode{
	PvAI,
	LocalPVP,
	NetPVP
}

public class GameController : MonoBehaviour {

	public GamePhase gamePhase = GamePhase.Other;
	public static GameMode gameMode = GameMode.LocalPVP;

	public int playerCount = 2;
	public PlayerInstance[] players = new PlayerInstance[2];
	public int turnOwnerID = -1;//-1 means nobody

	#region ITEMS
	public bool useLandMine = false;
	public int scoreMultiplier = 1;
	public int scoreDelta = 0;

	public void ResetScoreModifiers()
	{
		scoreMultiplier = 1;
		scoreDelta = 0;
	}

	public int CalculateScoreByItems(int _score)
	{
		Debug.LogFormat("Score+={0} * {1} + {2}",
			_score,scoreMultiplier,scoreDelta);
		return _score * scoreMultiplier + scoreDelta;
	}

	#endregion


	public static int currentPlayer{
		get{
			return instance.turnOwnerID;
		}
	}

	int[] score = new int[2]{0,0};
	bool goOn = true;

	public MapPoint selected = null;
	/// <summary>
	/// The adjacent points to the selected. Only the connectables are included.
	/// </summary>
	public List<MapPoint> adjacent = new List<MapPoint>();
	public Transform drawnLineParent = null;
	public Transform drawnBlockParent = null;

	public GameObject drawnLinePrefab = null;
	public GameObject drawnBlockPrefab = null;

	#region singleton
	public static GameController instance;

	void Awake()
	{
		instance = this;
	}
	#endregion

	void Start()
	{
		if (FindObjectOfType<NetworkManager>())
		{
			gameMode = GameMode.NetPVP;
		}

		if (gameMode == GameMode.LocalPVP)
			gamePhase = GamePhase.DrawLine;

		if (drawnLineParent == null)
		{
			drawnLineParent = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
			drawnLineParent.SetParent(transform);
			drawnLineParent.gameObject.name = "drawnLineParent";
		}

		if (drawnBlockParent == null)
		{   
			drawnBlockParent = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
			drawnBlockParent.SetParent(transform);
			drawnBlockParent.gameObject.name = "drawnBlockParent";
		}

		if (null == drawnLinePrefab)
		{
			Debug.LogError("The drawnLinePrefab is not set");
		}

		for(int i = 0; i < players.Length; i++)
		{
			players[i].SetHighlight(i == turnOwnerID);
			players[i].playerIndex = i;
		}
	}


	int clickCountOnNothing = 0;
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (gamePhase != GamePhase.DrawLine)
				return;

			if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition)))
			{
				Debug.Log("Clicked on nothing");
				clickCountOnNothing++;

				if (clickCountOnNothing >= 2)
				{
					SelectPoint(null);
				}
			}
			else
			{
				Debug.Log("Clicked on sth");
				clickCountOnNothing = 0;
			}
		}
	}

	/// <summary>
	/// Selecting a point.
	/// </summary>
	/// <param name="_p">the point</param>
	public void SelectPoint(MapPoint _p)
	{
		if (selected != null)
		{
			selected.UnHighlight();
			selected = null;
			//only when selected is not null
			foreach (MapPoint K in adjacent)
			{
				K.UnHighlight();
			}
			adjacent.Clear();
		}

		if (_p != null)
		{
			selected = _p;
			_p.HighlightAsSelected();

			//the four direction. Generalized writing
			float quarter = Mathf.PI / 2;//90 degrees
		

			for (float theta = 0; theta < 3 * quarter + 0.01f; theta += quarter)
			{
				int x = _p.posX + (int)Mathf.Round(Mathf.Cos(theta));
				int y = _p.posY + (int)Mathf.Round(Mathf.Sin(theta));
//				Debug.Log("Checking for point(" + x.ToString() + "," + y.ToString() + ")");

				if (MapController.instance.CheckExistPoint(x, y))
				{
					MapLine line = MapController.instance.GetLineFromPoint(_p.posX, _p.posY, theta);
					if (null == line || false == line.isVacant)
					{
						//	only the points that are able to connect can be 
						//second-highligt as selectable.

						//Debug.Log(string.Format("The line for ({0}, {1}, {2}) is ivalid", x, y, theta));
						continue;
					}

					//Debug.Log(string.Format("The point({0}, {1}) is recognized as adjacent.", x, y, theta));
					adjacent.Add(MapController.instance.points[x-1, y-1]);
					MapController.instance.points[x-1, y-1].HighlightAsSelectable();
				}
				else
				{
					Debug.Log(string.Format("No point({0}, {1})", x, y, theta));
				}
			}
		}
	}
		


	public void NextPhase()
	{
		if (GamePhase.EndGame == gamePhase)
		{
			gamePhase = GamePhase.DrawLine;
		}
		else
		{
			gamePhase++;
		}

		switch(gamePhase)
		{
		case GamePhase.DrawLine:
			goOn = false;
			//players[turnOwnerID];

			break;
		}
	}

	/// <summary>
	/// Draws the line.
	/// </summary>
	/// <param name="posX">Position x. Ranges from 1</param>
	/// <param name="posY">Position y. Ranges from 1</param>
	/// <param name="isHori">If set to <c>true</c> is horizontal.</param>
	public void DrawLine(int posX, int posY, bool isHori)
	{
		DrawLine(MapController.instance.lines[posX - 1, posY - 1, isHori ? 0 : 1]);
	}

	/// <summary>
	///   Core function. Also creates blocks, add score 
	/// and check winning condition.
	/// </summary>
	/// <param name="_line">Line.</param>
	public void DrawLine(MapLine _line)
	{
		GameObject g = Instantiate(drawnLinePrefab, drawnLineParent);
		g.transform.position = _line.transform.position;
		g.GetComponent<DrawnLine>().MakeHorizontal(_line.isHorizontal);
		g.GetComponent<DrawnLine>().SetColor(players[turnOwnerID].playerColor);
	
		_line.ownerID = turnOwnerID;
		_line.BeDrawn();

		//add score
		MapController.instance.Breaking();

		int units = MapController.instance.capturedUnits.Count;
		players[turnOwnerID].AddScore(CalculateScoreByItems(units));//should be modified by items
		//if (NetPvP)
		//if isHost(ID = 0)
		//	calc all
		//if is non-Host
		//	only listen to effects.
		//  and commands


		ResetScoreModifiers();
		MapController.instance.capturedUnits.Clear();

		if (CheckGameEnd())
		{
			//GameOver
			return;
		}

		//NextTurn
		turnOwnerID++;
		turnOwnerID %= players.Length;

		for(int i = 0; i < players.Length; i++)
		{
			players[i].SetHighlight(i == turnOwnerID);
		}

		SelectPoint(null);
	}

	/// <summary>
	/// Handles winning. Also handles draw condition.
	/// </summary>
	/// <returns><c>true</c>, if game ends, <c>false</c> otherwise.</returns>
	public bool CheckGameEnd()
	{
		if (0 == MapController.instance.vacantBlockCount)
		{
			gamePhase = GamePhase.EndGame;
			turnOwnerID = -1;

			if (players[0].score == players[1].score)
			{
				GameSetPanel.instance.Draw();
			}
			else if (players[0].score > players[1].score)
			{
				GameSetPanel.instance.Win(players[0].playerColor.color);
			}
			else
			{
				GameSetPanel.instance.Win(players[1].playerColor.color);
			}

			return true;
		}
		else 
		{
			return false;
		}

	}

	public void CreateDrawnBlock(GridBlock gb)
	{
		DrawnBlock drawn = Instantiate(drawnBlockPrefab, gb.transform).GetComponent<DrawnBlock>();
		drawn.SetColor(players[turnOwnerID].playerColor);
	}
}
