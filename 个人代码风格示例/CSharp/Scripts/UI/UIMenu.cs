using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMenu : MonoBehaviour {

	public void Restart()
	{
		if (GridController.instance != null)
		{
			GameObject g =GameObject.CreatePrimitive(PrimitiveType.Quad);
			g.AddComponent<LevelPasser>().levelCode =
				GridController.instance.levelCode;

			LevelPasser lp = g.GetComponent<LevelPasser>();
			lp.useLandMine = GameController.instance.useLandMine;

			DontDestroyOnLoad(g);
		}


		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void Quit()
	{
		Application.Quit();
	}

	public void MainMenu()
	{
		SceneManager.LoadScene(0);
	}

	public void Credits()
	{
		SceneManager.LoadScene(1);
	}

	public void Settings()
	{
		SceneManager.LoadScene(2);
	}

	public void PlayLocal()
	{
		SceneManager.LoadScene(3);
	}

	public void Tutorial()
	{
		SceneManager.LoadScene(4);
	}

	public void LevelEditor()
	{
		SceneManager.LoadScene(6);
	}

	public void NetworkPlay()
	{
		SceneManager.LoadScene(5);
	}
}
