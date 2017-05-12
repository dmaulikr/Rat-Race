﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour {

    private GameStateManager gsm = GameStateManager.instance;
    private MusicPlayer music = MusicPlayer.instance;


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Options() {
        gsm.state = GameStateManager.State.GAME_MODE_OPTIONS;
        music.paused = true;
    }
}
