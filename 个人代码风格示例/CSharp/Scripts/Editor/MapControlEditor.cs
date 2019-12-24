using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapController))]
public class MapControlEditor : Editor {

	public override void OnInspectorGUI(){
	
		DrawDefaultInspector();

		MapController myScript = (MapController)target;

		if(GUILayout.Button("Create Map by Grid"))
		{
			myScript.GenerateMap();
		}

		if(GUILayout.Button("Clear Map"))
		{
			myScript.ClearMap();
		}
	}

}
