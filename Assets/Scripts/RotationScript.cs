﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationScript : MonoBehaviour
{
    private Camera Camera;
    private Quaternion cameraRotation;

	// Use this for initialization
	void Start ()
    {
        Camera = Camera.main;
        cameraRotation = Camera.transform.rotation;
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.rotation = cameraRotation;
	}
}
