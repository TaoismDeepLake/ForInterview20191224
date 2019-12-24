using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class GridController : MonoBehaviour {

	[Range(1,30)]
	public int sizeX = 10, sizeY = 10;
	/// <summary>
	/// The unit distance between two dots.
	/// </summary>
	[Range(0.1f,10f)]
	public float unitDist = 1f;

	[SerializeField]GameObject pointPrefab = null, linePrefab = null, blockPrefab = null;
	[SerializeField]Transform pointParent = null, lineParent = null, blockParent = null; 

	public List<GridPoint> points = null;
	public List<GridLine> lines = null;
	public List<GridBlock> blocks = null;

	public static GridController instance
	{
		get {
			if (_instance)
			{
				return _instance;
			}
			else
			{
				_instance = FindObjectOfType<GridController>();
				return _instance;
			}
		}
		set
		{
			_instance = value;
		}

	}

	static GridController _instance;

	void Awake()
	{
		_instance = this;
	}

	void Start()
	{
		ApplyLevelPasser();
	}

	public void ApplyLevelPasser()
	{
		LevelPasser lp = FindObjectOfType<LevelPasser>();
		if (lp != null)
		{
			levelCode = lp.levelCode;
			ApplyCode();
			if (lp.useLandMine)
			{
				LandMineDeployment.instance.BeginSetting();
			}
			else
			{
				LandMineDeployment.instance.HidePanel();
				GameController.instance.gamePhase = GamePhase.DrawLine;
			}

			Destroy(lp.gameObject);
		}
	}

	void Reset()
	{
		sizeX = 10;
		sizeY = 10;
		unitDist = 1.5f;
	}

	#region CODE
	public string levelCode = "";
	public void CalculateCode()
	{
		StringBuilder s = new StringBuilder();
		s.Append("MP");//protocol name

		if (sizeX > 999 || sizeY > 999)
			throw new UnityException("Too big. Cannot encode");

		//size denote. usually as "010"
		s.Append((sizeX > sizeY ? sizeX.ToString() : sizeY.ToString())
			.PadLeft(3,'0'));

		inStack = 0;
		curVal = 0;

		for(int y = 0; y < sizeY; y++)
			for(int x = 0; x < sizeX; x++)
			{
				CodeToAChar(points[y * sizeX + x].isUsed, s);
			}

		if (inStack != 0)
		{
			s.Append(GetCurrentChar());
		}

		//Now handles the items.
		//first we parse it into 5n length string with '#'
		int appendLen = 5 - s.Length % 5;

		s.Append('#',appendLen);

		//now adds the items.
		ItemBase[] items = FindObjectsOfType<ItemBase>();

		foreach(ItemBase K in items)
		{
			if (K is ItemScorePlus)
			{
				ItemScorePlus item = K as ItemScorePlus;
				if (item.posX > 16 || item.posY > 16)
				{
					Debug.LogWarning("An item out of coding range.");
					continue;
				}

				if (item.scoreDelta > 0)
				{
					s.Append("PL");
					
					if (item.scoreDelta > 16)
						item.SetValue(16);

					//+1 is denoted as '0', and +16 as 'F'
					s.Append(FindHexChar(item.scoreDelta - 1));
					s.Append(FindHexChar(item.posX - 1));
					s.Append(FindHexChar(item.posY - 1));
				}
				else
				{
					s.Append("MN");

					if (item.scoreDelta < -16)
						item.SetValue(-16);

					//-1 is denoted as '0', and -16 as 'F'
					s.Append(FindHexChar(- 1 - item.scoreDelta));
					s.Append(FindHexChar(item.posX - 1));
					s.Append(FindHexChar(item.posY - 1));
				}

				//if an item is a plus-score, it cannot be other.
				continue;
			}

			if (K is ItemScoreMultiply)
			{
				ItemScoreMultiply item = K as ItemScoreMultiply;
				if (item.scoreFactor >= 0 && item.scoreFactor <= 15)
				{
					if (1 == item.scoreFactor)
					{
						Debug.LogWarning("A x1 item has no meaning. It will be ignored.");
						continue;
					}

					if (item.posX > 16 || item.posY > 16)
					{
						Debug.LogWarning("An item out of coding range.");
						continue;
					}

					s.Append("MT");
				
					//x0 is denoted as '0', and x15 as 'F'
					s.Append(FindHexChar(item.scoreFactor));
					s.Append(FindHexChar(item.posX - 1));
					s.Append(FindHexChar(item.posY - 1));
				}
				else
				{
					Debug.LogWarning("This protocol cannot support this multiply item.");
				}

				//if an item is a multi-score, it cannot be other.
				continue;
			}
				
			Debug.LogWarningFormat("An unknown item detected:{0}", K.gameObject.name);
		}




		levelCode = s.ToString();
		Debug.Log(levelCode);
	}
	int inStack = 0;
	int curVal = 0;

	public void CodeToAChar(bool _val, StringBuilder s)
	{
		if (_val)
		{
			curVal += 1 << (3 - inStack);
		}

		inStack++;

		//four vertices, should be a char now.
		if (4 == inStack)
		{
			if (curVal > 9)
			{
				s.Append((char)(curVal - 10 + 'A'));
			}
			else
			{
				s.Append((curVal));
			}
			//clear cache
			inStack = 0;
			curVal = 0;
		}

	}

	char FindHexChar(int val)
	{
		if (val >= 16)
		{
			Debug.LogWarning("Coding overflow detected");
			return 'F';
		}

		if (val <= -1)
		{
			Debug.LogWarning("Coding overflow detected");
				return '0';
		}

		if (val > 9)
		{
			return (char)(val - 10 + 'A');
		}
		else
		{
			return (char)(val + '0');
		}
	}

	/// <summary>
	/// Gets the current char.
	/// Usually used when the remaining dots is not enough 
	/// to form a complete char
	/// </summary>
	/// <returns>The current char.</returns>
	public char GetCurrentChar()
	{
		if (curVal > 9)
		{
			return (char)(curVal - 10 + 'A');
		}
		else
		{
			return (char)(curVal + '0');
		}
	}

	public void ApplyCode()
	{
		Reset();
		//CreateMapBySize();

		if (0 == LevelDecoder.CheckString(levelCode))
		{
			sizeX = LevelDecoder.range;
			sizeY = LevelDecoder.range;

			CreateMapBySize();
			int area = sizeX * sizeY;

			for(int i = 0; i < area; i++)
			{
				points[i].isUsed = LevelDecoder.vertices[i];
			}

			FindObjectOfType<MapController>().GenerateMap();


		}
		else
		{
			Debug.LogError("Cannot decode.");
		}
	}
	#endregion

	/// <summary>
	/// Gets the block. Use Logical coordination.
	/// </summary>
	/// <returns>The block.</returns>
	/// <param name="x">The x coordinate.[1,INF)</param>
	/// <param name="y">The y coordinate.[1,INF)</param>
	public GridBlock GetBlock(int x, int y)
	{
		if (x > sizeX - 1 || y > sizeY - 1 || x <= 0 || y <= 0)
			return null;

		//Debug.Log(string.Format("({0},{1}) = {2}", x, y ,(x-1) * sizeY  + (y-1)));
		return blocks[(y-1) * (sizeX-1)  + (x-1) ];
	}

	public void CreateMapBySize()
	{
		ClearMap();

		if (null == pointParent)
		{
			pointParent = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
			pointParent.SetParent(transform);
			pointParent.gameObject.name = "PointParent";
		}

		if (null == lineParent)
		{
			lineParent = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
			lineParent.SetParent(transform);
			lineParent.gameObject.name = "LineParent";
		}

		if (null == blockParent)
		{
			blockParent = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
			blockParent.SetParent(transform);
			blockParent.gameObject.name = "BlockParent";
		}

		points = new List<GridPoint>();
		lines = new List<GridLine>();
		blocks = new List<GridBlock>();

		for (int y = 1; y <= sizeX; y++)
		{
			for (int x = 1; x <= sizeY; x++)
			{
				GridPoint p = CreatePoint(x, y);

				if (x != sizeX)
				{
					//line
					GridLine lineH = CreateLine(x, y, true);
				}

				if (y != sizeY)
				{
					GridLine lineV = CreateLine(x, y, false);
				}
			
				//Block
				if (x != sizeX && y != sizeY)
				{
					GridBlock b = CreateBlock(x, y, 1);

				}
			}
		}
	}

	void ClearMap()
	{
		if (points != null)
		{
			foreach(GridPoint K in points)
			{
				if (K != null)

				DestroyImmediate(K.gameObject);
			}
			points = null;
		}

		if (lines != null)
		{
			foreach(GridLine K in lines)
			{
				if (K != null)
				DestroyImmediate(K.gameObject);
			}
			lines = null;
		}

		if (blocks != null)
		{
			foreach(GridBlock K in blocks)
			{
				if (K != null)
				DestroyImmediate(K.gameObject);
			}
			blocks = null;
		}
	}

	/// <summary>
	/// Creates the point. Using playing coord [1, +INF)
	/// </summary>
	/// <returns>The point.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	GridPoint CreatePoint(int x, int y)
	{
		GridPoint p =
			Instantiate(pointPrefab, 
				transform.position + new Vector3(x * unitDist, - y * unitDist, 0), 
				Quaternion.identity)
				.GetComponentInChildren<GridPoint>();
		//initialize its coordination
		p.posX = x;
		p.posY = y;

		p.transform.SetParent(pointParent);

		points.Add(p);

		return p;
	}

	GridLine CreateLine(int x, int y, bool isHorizontal)
	{//need simpifying
		if (isHorizontal)
		{
			GridLine lineH = //Horizontal
				Instantiate(linePrefab, 
					transform.position + new Vector3((float)((x + 0.5f) * unitDist), - y * unitDist, 0), 
					Quaternion.identity)
				.GetComponentInChildren<GridLine>();

			lineH.posX = x;
			lineH.posY = y;
			lineH.SetHorizontal(true);

			lines.Add(lineH);

			lineH.transform.SetParent(lineParent);
			lineH.transform.localScale = new Vector3(unitDist, unitDist, unitDist);

			return lineH;
		}
		else
		{
			//Vertical
			GridLine lineV = Instantiate(linePrefab, 
				transform.position + new Vector3(x * unitDist, - (float)((y + 0.5f) * unitDist), 0), 
				Quaternion.identity)
				.GetComponentInChildren<GridLine>();

			lineV.posX = x;
			lineV.posY = y;
			lineV.SetHorizontal(false);

			lines.Add(lineV);

			lineV.transform.SetParent(lineParent);
			lineV.transform.localScale = new Vector3(unitDist, unitDist, unitDist);

			return lineV;
		}
	}

	GridBlock CreateBlock(int x, int y, int size)
	{
		GridBlock b = //Vertical
			Instantiate(blockPrefab, 
				transform.position + new Vector3(x * unitDist, - y * unitDist, 0), 
				Quaternion.identity)
			.GetComponentInChildren<GridBlock>();

		b.posX = x;
		b.posY = y;
		b.size = size;

		blocks.Add(b);

		b.transform.SetParent(blockParent);

		b.transform.localScale = new Vector3(unitDist, unitDist, unitDist);

		return b;
	}
}
