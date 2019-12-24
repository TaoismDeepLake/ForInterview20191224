using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageManager : MonoBehaviour {

	[SerializeField]GameObject messagePrefab;
	[SerializeField]RectTransform messageParent;

	public static MessageManager instance;

	void Awake()
	{
		instance = this;

		if (!messageParent)
		{
			messageParent = FindObjectOfType<Canvas>().transform as RectTransform;
		}
	}


	public static MessageBox CreateMessage(string content)
	{
		GameObject g = Instantiate(instance.messagePrefab, instance.messageParent);
		MessageBox msg = g.GetComponent<MessageBox>();

		msg.SetContent(content);

		return msg;
	}

}
