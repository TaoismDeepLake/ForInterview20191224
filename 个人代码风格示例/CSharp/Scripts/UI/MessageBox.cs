using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour {

	public Button buttonConfirm;

	public Button buttonB;
	public Image buttonBIcon;
	public Text buttonBLabel;

	public Text content;

	public void SetContent(string str)
	{
		content.text = str;

	}

	public void Cease()
	{
		Destroy(gameObject);
	}

}
