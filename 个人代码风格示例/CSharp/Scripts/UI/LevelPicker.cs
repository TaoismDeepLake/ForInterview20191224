using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LevelInfo{
	public string levelName = "Unnamed";
	public string levelCode = "0";
	public string author = "Secret Person";
}

public class LevelPicker : MonoBehaviour {

	public List<LevelInfo> levelList = new List<LevelInfo>();
	public int curIndex;
	[SerializeField]GameObject prefab = null;
	[SerializeField]Scene scenePlay;

	[SerializeField]Text levelNameText = null;
	[SerializeField]InputField levelCodeText = null;
	[SerializeField]Text authorText = null;

	[SerializeField]GameObject subpanel = null;
	[SerializeField]InputField customlevelCodeText = null;

	[SerializeField]Toggle toggleMine = null;

	public void Awake()
	{
		RefreshDisplay();
	}

	public void FlipPage(int delta)
	{
		if (levelList.Count != 0)
		{
			curIndex += delta;
			curIndex %= levelList.Count;
			if (curIndex < 0)
				curIndex += levelList.Count;

			RefreshDisplay();
		}

	}

	public void EnterLevel()
	{
		LevelPasser lp = Instantiate(prefab).GetComponent<LevelPasser>();
		lp.levelCode = levelList[curIndex].levelCode;
		lp.useLandMine = toggleMine.isOn;
		Debug.LogFormat("Use mine = {0}", lp.useLandMine);

		SceneManager.LoadScene(3);

	}
	public void EnterLevelThisScene()
	{
		LevelPasser lp = Instantiate(prefab).GetComponent<LevelPasser>();
		lp.levelCode = levelList[curIndex].levelCode;
		lp.useLandMine = toggleMine.isOn;
		Debug.LogFormat("Use mine = {0}", lp.useLandMine);

		GameController.instance.gamePhase = GamePhase.Presetting;
		GridController.instance.ApplyLevelPasser();
	}


	public void ShutThisPanel()
	{
		gameObject.SetActive(false);
	}

	public void SwitchSubPanel()
	{
		subpanel.SetActive(!subpanel.activeSelf);
	}

	public void EnterCustomLevel()
	{
		LevelPasser lp = Instantiate(prefab).GetComponent<LevelPasser>();
		lp.levelCode = customlevelCodeText.text;
		lp.useLandMine = toggleMine.isOn;
		Debug.LogFormat("Use mine = {0}", lp.useLandMine);

		SceneManager.LoadScene(3);
	}
	public void EnterCustomLevelThisScene()
	{
		LevelPasser lp = Instantiate(prefab).GetComponent<LevelPasser>();
		lp.levelCode = customlevelCodeText.text;
		lp.useLandMine = toggleMine.isOn;
		Debug.LogFormat("Use mine = {0}", lp.useLandMine);

		GridController.instance.ApplyLevelPasser();
	}


	void RefreshDisplay()
	{
		levelNameText.text = levelList[curIndex].levelName;
     	levelCodeText.text = levelList[curIndex].levelCode;
     	authorText   .text = levelList[curIndex].author;

	}
}
