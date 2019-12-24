using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridController))]
public class GridEditor : Editor {

	public override void OnInspectorGUI(){
	
		DrawDefaultInspector();

		GridController myScript = (GridController)target;

		if(GUILayout.Button("Create Grid"))
		{
			myScript.CreateMapBySize();
		}

		if(GUILayout.Button("CalculateCode"))
		{
			myScript.CalculateCode();
		}

		if(GUILayout.Button("Apply Code"))
		{
			myScript.ApplyCode();
		}

	}

}
