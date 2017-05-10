﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float speed = 10.0f;
    public Vector3 initialPosition;
    public Vector3 initialRotation;

    private MazeBuilder maze;
    private Rigidbody2D rb;
    private Animator animator;
    private Game game;
    private AIController aCat;
    private BoxCollider2D theCollider;
    private SoundPlayer sound = SoundPlayer.instance;


    // Use this for initialization
    void Start() {
        maze = FindObjectOfType<MazeBuilder>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        game = FindObjectOfType<Game>();
        aCat = FindObjectOfType<AIController>();
        theCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update() {
        int dx, dy;
        GetInput(out dx, out dy);
        Move(dx, dy);

        if (game.state == Game.State.DEATH) {
            //TODO: play death sound, do death animation
            Invoke("ResetPlayer", 1.0f);
        }

        if (aCat.flee > 0) {
            transform.localScale = new Vector3(2, 2, 2);
            theCollider.size = new Vector2(1.35f, 1.35f);
        } else {
            transform.localScale = new Vector3(1, 1, 1);
            theCollider.size = new Vector2(2.7f, 2.7f);
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
        if (game.state != Game.State.PLAY) {
            animator.SetBool("IsRunning", false);
            rb.velocity = Vector2.zero;
            return;
        }

        if (dx != 0 || dy != 0) {
            rb.velocity = new Vector2(dx, dy) * speed;
        }

        animator.SetBool("IsRunning", (rb.velocity.x != 0
            || rb.velocity.y != 0));

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

        //correct rotation
        if (dx > 0) {
            transform.rotation =
                Quaternion.Euler(0, 0, -90);
        }
        if (dx < 0) {
            transform.rotation =
                Quaternion.Euler(0, 0, 90);
        }
        if (dy > 0) {
            transform.rotation =
                Quaternion.Euler(0, 0, 0);
        }
        if (dy < 0) {
            transform.rotation =
                Quaternion.Euler(0, 0, 180);
        }
    }

    public void ResetPlayer() {
        game.state = Game.State.RESET_PLAYER;
        transform.position = initialPosition;
        transform.eulerAngles = initialRotation;
        foreach(AIController enemy in FindObjectsOfType<AIController>()) {
            enemy.ResetEnemy();
        }
        Invoke("Play", 1.0f);
    }

    void Play() {
        game.state = Game.State.PLAY;
    }

}
