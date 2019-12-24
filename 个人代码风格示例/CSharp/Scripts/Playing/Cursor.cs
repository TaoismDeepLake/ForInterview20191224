using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour {

	[SerializeField]GameObject highlightPrefab = null;

	GameObject[] highlightEffects = new GameObject[5]; 

	// Use this for initialization
	void Start () {

		for(int i = 0; i < 5; i++)
		{
			highlightEffects[i] = Instantiate(highlightPrefab);
		}

	}

	// Update is called once per frame
	void Update () {

		Vector3 temp = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);

		Vector3 pos = Camera.main.ScreenToWorldPoint(temp);

		//Debug.Log(pos);
	}
}
