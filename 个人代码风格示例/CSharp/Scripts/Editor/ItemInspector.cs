using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemBase))]
public class ItemInspetor : Editor {

	public override void OnInspectorGUI(){

		DrawDefaultInspector();

		ItemBase myScript = (ItemBase)target;

		if(GUILayout.Button("Attach to Grid"))
		{
			myScript.AttachToBlock();
		}
	}
}
	
[CustomEditor(typeof(ItemScorePlus))]
public class ItemInspetor1 : Editor {

	public override void OnInspectorGUI(){

		DrawDefaultInspector();

		ItemBase myScript = (ItemBase)target;

		if(GUILayout.Button("Attach to Grid"))
		{
			myScript.AttachToBlock();
		}
	}
}
	
[CustomEditor(typeof(ItemScoreMultiply))]
public class ItemInspetor2 : Editor {

	public override void OnInspectorGUI(){

		DrawDefaultInspector();

		ItemBase myScript = (ItemBase)target;

		if(GUILayout.Button("Attach to Grid"))
		{
			myScript.AttachToBlock();
		}
	}
}
