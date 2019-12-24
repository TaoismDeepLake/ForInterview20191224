using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SettingsMobile : MonoBehaviour {

	void OnDisable()
	{
		Debug.Log("PrintOnDisable: script was disabled");
	}

	void OnEnable()
	{
		Debug.Log("PrintOnEnable: script was enabled");
	}


}
