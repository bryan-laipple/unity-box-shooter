﻿using UnityEngine;
using System.Collections;

public class BasicMover : MonoBehaviour {

	public float spinSpeed = 180.0f;
	public float motionMagnitude = 0.1f;

	public bool doSpin = true;
	public bool doMotion = false;

	// Use this for initialization
	void Update () {
		if (doSpin) {
			gameObject.transform.Rotate (Vector3.up * spinSpeed * Time.deltaTime);
		}

		if (doMotion) {
			gameObject.transform.Translate (Vector3.up * Mathf.Cos (Time.timeSinceLevelLoad) * motionMagnitude);
			gameObject.transform.Translate (Vector3.right * Mathf.Sin (Time.timeSinceLevelLoad) * motionMagnitude);
		}
	}
}