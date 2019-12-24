using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPasser : MonoBehaviour {

	public string levelCode = "";

	public static LevelPasser instance = null;

	public bool useLandMine = false;

	int range = 10;
	bool vertices;

	void Awake()
	{
		instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public static void UseMine(bool _use)
	{
		instance.useLandMine = _use;
	}

	public static GameObject CreatePasser(string code, bool useMine = false)
	{
		GameObject g = GameObject.CreatePrimitive(PrimitiveType.Quad);
		LevelPasser lp = g.AddComponent<LevelPasser>();
		lp.levelCode = code;
		lp.useLandMine = useMine;

		return g;
	}
		
}
