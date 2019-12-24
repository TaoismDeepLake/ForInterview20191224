using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	Transform camTrans = null;
	Vector3 startingPoint = Vector3.left;

	#region zooming
	Vector2 lastDelta = new Vector2(0,0);
	Vector2 delta = new Vector2(0,0);

	[Range(0.1f, 5f)]
	public float zoomSensitivity = 1;

	bool firstFrame2Finger = true;
	#endregion

	#region moving
	[Range(-2f, 2f)]
	public float moveSensitivity = 1;

	Vector2 lastCenter = Vector2.up;
	Vector2 center = Vector2.up;

	bool firstFrame3Finger = true;
	#endregion

	#region PC_CONTROL

	public float scrollZoomSensitivity = 100;
	public float keyboardMoveSensitivity = 1 ;
	public float scrollMoveSensitivity = 1;

	#endregion

	public void ResetPosition()
	{
		camTrans.position = startingPoint;
	}

	void Start()
	{
		camTrans = Camera.main.transform;
		if (null == camTrans)
		{
			Debug.LogError("Main camera is missing. Please setup one.");
			return;
		}

		startingPoint = camTrans.position;
	}

	// Update is called once per frame
	void Update () {
		//zooming
		//Debug.Log(Input.touchCount);

		if (2 == Input.touchCount )
		{
			delta = Input.GetTouch(1).position - Input.GetTouch(0).position;
			if (firstFrame2Finger)
			{
				firstFrame2Finger = false;
				lastDelta = delta;
				return;
			}

			if (Camera.main.orthographic)
			{
				Camera.main.orthographicSize += (delta.magnitude - lastDelta.magnitude) * zoomSensitivity;
			}
			else
			{
				camTrans.Translate(0, 0, (delta.magnitude - lastDelta.magnitude) * zoomSensitivity);
			}
			lastDelta = delta;
		}
		else
		{
			firstFrame2Finger = true;
		}

		//moving camera
		if (3 == Input.touchCount )
		{
			center = Input.GetTouch(2).position +
				Input.GetTouch(1).position +
				Input.GetTouch(0).position;
			center /= 3;//average

			if (firstFrame3Finger)
			{
				firstFrame3Finger = false;
				lastCenter = center;
				return;
			}

			camTrans.Translate((center - lastCenter) * moveSensitivity);
			lastCenter = center;
		}
		else
		{
			firstFrame3Finger = true;
		}

		#if UNITY_STANDALONE || UNITY_EDITOR
		camTrans.Translate(Input.GetAxis("Horizontal") * keyboardMoveSensitivity,
			Input.GetAxis("Vertical") * keyboardMoveSensitivity,
			Input.GetAxis("Mouse ScrollWheel") * scrollZoomSensitivity);

		//othro
		camTrans.Translate(Input.GetAxis("Horizontal") * keyboardMoveSensitivity,
			Input.GetAxis("Vertical") * keyboardMoveSensitivity, 
			0);
		Camera.main.orthographicSize +=	Input.GetAxis("Mouse ScrollWheel") * scrollZoomSensitivity;
		


		if (Input.GetMouseButton(2))
		{
			camTrans.Translate(Input.GetAxis("Mouse X") * scrollMoveSensitivity,
				Input.GetAxis("Mouse Y") * scrollMoveSensitivity,
				0f);
		}
		//Next may consider holding mid button to move

		if (Camera.main.orthographicSize < 0)
			Camera.main.orthographicSize = 0.01f;
		#endif
	}
}
