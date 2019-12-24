using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour {

	public static MapController instance;

	[SerializeField]GameObject pointPrefab = null, linePrefab = null, blockPrefab = null;
	[SerializeField]Transform pointParent = null, lineParent = null, blockParent = null; 

	/// <summary>
	/// The points. Starts from (0,0)
	/// </summary>
	[SerializeField]public MapPoint[,] points= null;
	/// <summary>
	/// The 3rd dimension indicates whether it's horizontal.
	/// 0 = horizontal, 1 = vertical.
	/// </summary>
	[SerializeField]public MapLine[,,] lines= null;
	public List<MapBlock> blocks = null;

	public int vacantBlockCount = 0;

	public List<GridBlock> capturedUnits = new List<GridBlock>();
	public List<MapBlock> breakingList = new List<MapBlock>();

	/// <summary>
	/// Set this to false to use custom levels.
	/// </summary>
	public bool createMapOnLoad = true;

	int sizeX = 1;
	int sizeY = 1;
	float unitDist = 1f;

	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		//note that [,] can't be serialized. 
		//once you enter play mode, the points and lines will lose track.
		if (createMapOnLoad)
		{
			GenerateMap();

		}
	}

	void Reset(){
		instance = this;
		if (null == GridController.instance)
			GridController.instance = FindObjectOfType<GridController>();

		transform.position = GridController.instance.transform.position;

		sizeX = GridController.instance.sizeX;
		sizeY = GridController.instance.sizeY;
		unitDist = GridController.instance.unitDist;

		ClearMap();

		points = new MapPoint[sizeX, sizeY];
		lines = new MapLine[sizeX, sizeY, 2];
		blocks = new List<MapBlock>();

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
	}

	public void ClearMap()
	{
		if (points != null)
		{
			foreach(MapPoint K in points)
			{
				if (K != null)// && K.gameObject!=null)
					DestroyImmediate(K.gameObject);
			}
			points = null;
		}

		if (lines != null)
		{
			foreach(MapLine K in lines)
			{
				if (K != null)
					DestroyImmediate(K.gameObject);
			}
			lines = null;
		}

		if (blocks != null)
		{
			foreach(MapBlock K in blocks)
			{
				if (K != null)
					DestroyImmediate(K.gameObject);
			}
			blocks = new List<MapBlock>();
		}

		while(pointParent.childCount != 0)
		{
			DestroyImmediate(pointParent.GetChild(0).gameObject);
		}

		while(lineParent.childCount != 0)
		{
			DestroyImmediate(lineParent.GetChild(0).gameObject);
		}
	}

	public void GenerateMap(){
		Reset();

		if (null == GridController.instance)
		{
			Debug.LogError("Please Add a Grid Controller first.");
			return;
		}

		foreach(GridPoint K in GridController.instance.points)
		{
			if (K.isUsed)
				CreatePoint(K);
		}

		//create horizontal lines
		for (int x = 0; x <= sizeX - 2; x++)
		{
			for (int y = 0; y <= sizeY - 1; y++)
			{
				if (points[x,y] != null &&
					points[x+1,y] != null)
				{
					CreateLine (x + 1, y + 1, true);
				}
			}
		}

		//create vetical lines
		for (int x = 0; x <= sizeX - 1; x++)
		{
			for (int y = 0; y <= sizeY - 2; y++)
			{
				if (points[x,y] != null &&
					points[x,y+1] != null)
				{
					CreateLine (x + 1, y + 1, false);
				}
			}
		}

		//Create blocks:
		//foreach non-rightmost, non-undermost point, check each possible size.
		for (int x = 0; x < sizeX - 2; x++)
		{
			for (int y = 0; y < sizeY - 2; y++)
			{
				bool goOn = true;

				//checking each valid size that won't cause
				//index out of range
				for (int size = 1; size <= sizeX - 1 - x
					&& size <= sizeY - 1 - y
					&& goOn; size++)
				{
					List<MapLine> relatedLines = new List<MapLine>();
					bool isOk = true;//this block is complete so far.

					//check wether the 4 sides is complete
					//by lines instead of points, though without items they are equal
					for (int i = 0; i <= size - 1; i++)//checking each line of the sides
					{
						//tries to check the four sides
						//simultaneously within one loop

						//if these lines are broken,
						//there won't be large blocks.
						if (lines[x + i, y, 0] == null ||//up side
							lines[x, y + i, 1] == null)//left side
						{
//							Debug.Log("Breaking because (" + x.ToString() + ", "
//								+ y.ToString() + ", " + size + ", " + i + ") has bad side");

							goOn = false;
							isOk = false;
							break;
						}
							
						//if these are broken, there may still be larger blocks.
						if (lines[x + i, y + size, 0] == null ||//down side
							lines[x + size, y + i, 1] == null)//right side
						{
							isOk = false;
							break;
						}

						//add them
						relatedLines.Add(lines[x + i, y, 0]);
						relatedLines.Add(lines[x, y + i, 1]);
						relatedLines.Add(lines[x + i, y + size, 0]);
						relatedLines.Add(lines[x + size, y + i, 1]);
					}

					if (isOk)
					{
						//all four sides passed ,the blcok is complete.
						//now create it.
						MapBlock block = CreateBlock(x + 1, y + 1, size);

						block.outsideLines = relatedLines;

						foreach(MapLine K in relatedLines)
						{
							K.blocksRelated.Add(block);
						}
					}

				}
			}
		}

		vacantBlockCount = blocks.Count;
	}

	public void AddCapturingUnitsFor(MapBlock _mb)
	{
		for(int x = _mb.posX; x <= _mb.posX + _mb.size - 1; x++)
			for(int y = _mb.posY; y <= _mb.posY + _mb.size - 1; y++)
			{
				if (!capturedUnits.Exists((GridBlock _b) => _b == GridController.instance.GetBlock(x,y)))
				{
					capturedUnits.Add(GridController.instance.GetBlock(x,y));
				}
			}
	}

	/// <summary>
	/// Breaks every block related to the units in capturedUnits.
	/// Also disables the lines inside the big blocks.
	/// </summary>
	public void Breaking()
	{
		foreach(GridBlock K in capturedUnits)
		{
			K.ActivateBlockItems();
			K.BreakingRelatedBlocks();
			GameController.instance.CreateDrawnBlock(K);
		}

		foreach(MapBlock K in breakingList)
		{
			if (K.isVacant)
			{
				if (K == null)
					Debug.LogError("Check it!");

				Debug.LogFormat("Breaking {0}", K);
				vacantBlockCount--;
				K.isVacant = false;
			}
		}


		breakingList.Clear();
	}

	/// <summary>
	/// Checks whether the point with the given coordination exists.
	/// The range is [1, +INF)
	/// </summary>
	/// <returns><c>true</c>, if point exists, <c>false</c> otherwise.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public bool CheckExistPoint(int x, int y)
	{
		if (null == points)
			Debug.LogError("The Map controller has lost track of points!");

		//out of bounds
		if (x <= 0 || x > points.GetLength(0) ||
			y <= 0 || y > points.GetLength(1))
		{
			Debug.Log("Point check out of bounds");
			return false;
		}

//		Debug.Log(string.Format("Check point({0},{1} = {2}", x , y, null == points[x - 1, y - 1]));
		return null != points[x - 1, y - 1];
	}

	/// <summary>
	/// Gets the adjacent line from point by given angle.
	/// </summary>
	/// <returns>The line.</returns>
	/// <param name="x">The x coordinate of the point. [1,INF)</param>
	/// <param name="y">The y coordinate of the point. [1,INF).</param>
	/// <param name="theta">Theta. To the right is 0, cw.</param>
	public MapLine GetLineFromPoint(int x, int y, float theta)
	{
		if (CheckExistPoint(x, y))
		{
			int dx = (int)Mathf.Round(Mathf.Cos(theta));
			int dy = (int)Mathf.Round(Mathf.Sin(theta));

			int lineX = x + (dx < 0 ? dx : 0) ; 
			int lineY = y + (dy < 0 ? dy : 0) ;

			return lines[lineX - 1, lineY - 1, (dx != 0 ? 0 : 1) ];
		}

		return null;
	}

	MapPoint CreatePoint(GridPoint gp)
	{
		GameObject g = Instantiate(pointPrefab, gp.transform.position, Quaternion.identity);
		g.name = System.String.Format("MapPoint({0}, {1})", gp.posX, gp.posY);

		MapPoint mp = g.GetComponentInChildren<MapPoint>();
		mp.UseValueOf(gp);

		points[mp.posX - 1, mp.posY - 1] = mp;
//		Debug.Log(string.Format("points[{0}, {1}] is filled.", mp.posX - 1, mp.posY - 1));

		mp.transform.SetParent(pointParent);

		return mp;
	}

	/// <summary>
	/// Creates the line. Using playing coord [1, +INF)
	/// </summary>
	/// <returns>The line.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="isHorizontal">If set to <c>true</c> is horizontal.</param>
	MapLine CreateLine(int x, int y, bool isHorizontal)
	{
		GameObject g = null;

		if (isHorizontal)
		{
			g = Instantiate(linePrefab, 
				transform.position + new Vector3((x + 0.5f) * unitDist, - y * unitDist, 0),
				Quaternion.identity);

			g.name = System.String.Format("MapLine ({0}, {1}, H)", x, y);
		}
		else
		{
			g = Instantiate(linePrefab, 
				transform.position + new Vector3(x * unitDist, - (y + 0.5f) * unitDist, 0),
				Quaternion.identity);

			g.name = System.String.Format("MapLine ({0}, {1}, V)", x, y);
		}


		MapLine line = g.GetComponentInChildren<MapLine>();

		line.posX = x;
		line.posY = y;
		line.SetHorizontal(isHorizontal);

		if (isHorizontal)
		{
			lines[x - 1, y - 1, 0] = line;
			Debug.Log(System.String.Format("Creating Line[{0}, {1}, {2}]", x, y, 0));
		}
		else
		{
			lines[x - 1, y - 1, 1] = line;
			Debug.Log(System.String.Format("Creating Line[{0}, {1}, {2}]", x, y, 1));
		}

		line.transform.localScale = new Vector3(unitDist, unitDist, unitDist);
		line.transform.SetParent(lineParent);

		return line;
	}

	/// <summary>
	/// Creates the block. Using playing coord [1, +INF)
	/// </summary>
	/// <returns>The block.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="size">Size.</param>
	MapBlock CreateBlock(int x, int y, int size)
	{
		GameObject g = Instantiate(blockPrefab, 
			transform.position + new Vector3(x * unitDist, - y * unitDist, 0),
			Quaternion.identity);
		g.name = System.String.Format("MapBlock({0}, {1}, {2}x{2})", x, y, size);

		MapBlock block = g.GetComponentInChildren<MapBlock>();
		block.posX = x;
		block.posY = y;
		block.SetSize(size);
		blocks.Add(block);

		block.transform.localScale = new Vector3(unitDist, unitDist, unitDist);
		block.transform.SetParent(blockParent);

		return block;
	}
}
