﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float speed = 10.0f;
    public Vector3 InitialPosition;
    public Vector3 InitialRotation;


    private MazeBuilder maze = null;
    private Animator animator;
    private Rigidbody2D rb;
    private AIController aCat;
    private BoxCollider2D theCollider;
    private SoundPlayer sound = SoundPlayer.instance;
    private GameStateManager gsm = GameStateManager.instance;
    private Recorder recorder = Recorder.instance;

    private int catValue = 200;

    // Use this for initialization
    void Start() {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        aCat = FindObjectOfType<AIController>();
        theCollider = GetComponent<BoxCollider2D>();
        if (gsm.recordThisGame) {
            recorder.RecordIfRecording(recorder.CreateCommand(gameObject));
        }
    }

    // Update is called once per frame
    void Update() {
        if (maze) {
            int dx, dy;
            GetInput(out dx, out dy);
            Move(dx, dy);
        } else {
            maze = FindObjectOfType<MazeBuilder>();
        }        
        if (aCat.flee > 0) {
            transform.localScale = new Vector3(2, 2, 2);
            theCollider.size = new Vector2(1.35f, 1.35f);
        } else {
            catValue = 200;
            transform.localScale = new Vector3(1, 1, 1);
            theCollider.size = new Vector2(2.7f, 2.7f);
        }
    }

    private void LateUpdate() {
        //update recording
        if (gsm.framesInState > 7 && gsm.recordThisGame && gsm.state == GameStateManager.State.GAME_MODE_PLAY) {
            recorder.RecordIfRecording(recorder.MoveCommand(gameObject));
        }
    }

    void GetInput(out int dx, out int dy) {
        dx = 0;
        dy = 0;

        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.y);

        if (Input.GetAxis("Horizontal") < 0) {
            dx = -1;
        } else if (Input.GetAxis("Horizontal") > 0) {
            dx = 1;
        } else if (Input.GetAxis("Vertical") < 0) {
            dy = -1;
        } else if (Input.GetAxis("Vertical") > 0) {
            dy = 1;
        }

        if (!maze.IsPlayerSpace(x + dx, y + dy)) {
            dx = 0;
            dy = 0;
        }
    }

    void Move(int dx, int dy) {
        if (gsm.state != GameStateManager.State.GAME_MODE_PLAY) {
            if (animator) {
                animator.SetBool("IsRunning", false);
                rb.velocity = Vector2.zero;
            }
            return;
        }

        if (dx != 0 || dy != 0) {
            rb.velocity = new Vector2(dx, dy) * speed;
        }

        if (animator) {
            animator.SetBool("IsRunning", (rb.velocity.x != 0 ||
            rb.velocity.y != 0));
        } else {
            Debug.LogWarning("Missing Mouse Animator");
        }

        if (dx > 0) {
            transform.rotation = Quaternion.Euler(0, 0, -90);
        } else if (dx < 0) {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        } else if (dy > 0) {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        } else if (dy < 0) {
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }

        if (Random.value < Time.deltaTime) {
            if (aCat.flee > 0) {
                sound.BigSqueak();
            } else {
                sound.Squeak();
            }
        }

        //Teleport
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.y);

        if (y == 32) {
            if (x == 14 && rb.velocity.x > 0) {
                Debug.Log("dx=" + dx);
                x = 32;
                transform.position = new Vector3(x, y, transform.position.z);
            } else if (x == 32 && rb.velocity.x < 0) {
                x = 14;
                transform.position = new Vector3(x, y, transform.position.z);
            }
        }


        //correct position
        if (dx != 0) {
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }
        if (dy != 0) {
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }
    }
   

    public void DoubleCatValue() {
        catValue *= 2;
    }

    public int GetCatValue() {
        return catValue;
    }
}