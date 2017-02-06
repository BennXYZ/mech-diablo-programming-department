﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayer : MonoBehaviour {

    public MovieTexture movie;
    public bool loop;

	// Use this for initialization
	void Start () {
        GetComponent<RawImage>().texture = movie as MovieTexture;
        movie.Play();
        if(loop == false)
        movie.loop = false;

        if (loop == true)
            movie.loop = true;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
