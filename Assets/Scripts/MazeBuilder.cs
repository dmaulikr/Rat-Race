﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeBuilder : MonoBehaviour {
    public GameObject exterior;
    public GameObject[] solidRegions;
    public GameObject[] wallTiles;

    public int xSize = 48;
    public int ySize = 64;

    private Recorder recorder = Recorder.instance;
    private GameStateManager gsm = GameStateManager.instance;

    private bool[,] grid;

    // Use this for initialization
    void Start() {
        Initialize();

        //upper maze
        CarveMazeSection(8, 4, 1, xSize - 2, 1, ySize / 2 - 4);

        //lower maze, leaving several rows for a cat box
        CarveMazeSection(8, 4, 1, xSize - 2, ySize / 2 + 4, ySize / 2 - 5);

        //join the two halves together
        CarveColumn(2, 1, ySize - 2);
        CarveColumn(xSize - 3, 1, ySize - 2);

        //build tunnels
        CarveRow(ySize / 2, 1, 15);
        CarveRow(ySize / 2, xSize - 17, 15);

        Draw();
    }

    // Update is called once per frame
    void Update() {

    }

    public void Initialize() {
        grid = new bool[xSize, ySize];
        for (int x = 0; x < xSize; x++) {
            for (int y = 0; y < ySize; y++) {
                grid[x, y] = true;
            }
        }
    }

    /*
     * Assumptions: numColumns and numRows both 
     * at least 2; xSize and ySize both at least 7; 
     * startX and startX+xSize-1 in range 0 through global xSize - 1
     * startY, startY+ySize-1 in range 0 through global ySize - 1
     */
    public void CarveMazeSection(int numCols, int numRows, int startX, int xSize, int startY, int ySize) {

        CarveRow(startY + 1, startX, xSize);
        CarveRow(startY + ySize - 2, startX, xSize);
        CarveColumn(startX + 1, startY, ySize);
        CarveColumn(startX + xSize - 2, startY, ySize);

        int[] RowWallWidth = new int[numRows - 1];
        for (int i = 0; i < numRows - 1; i++) {
            RowWallWidth[i] = 1;
        }
        for (int i = 0; i < ySize - 2 - 4 * numRows; i++) {
            int j = Random.Range(0, numRows - 1);
            RowWallWidth[j]++;
        }
        int r = 5;
        for (int i = 0; i < numRows - 2; i++) {
            r += RowWallWidth[i];
            CarveRow(r + startY - 1, startX, xSize);
            r += 3;
        }

        int[] ColWallWidth = new int[numCols - 1];
        for (int i = 0; i < numCols - 1; i++) {
            ColWallWidth[i] = 1;
        }
        for (int i = 0; i < xSize - 2 - 4 * numCols; i++) {
            int j = Random.Range(0, numCols - 1);
            ColWallWidth[j]++;
        }
        int c = 5;
        for (int i = 0; i < numCols - 2; i++) {
            c += ColWallWidth[i];
            CarveColumn(c + startX - 1, startY, ySize);
            c += 3;
        }


        //now connect some "wall islands" together, first beteen rows
        r = 4 + startY;
        for (int i = 0; i < numRows - 2; i++) {
            r += RowWallWidth[i];

            // r-1, r, r+1 are y values of a player path

            if (Random.value > 0.5) {

                c = 4 + startX;
                for (int j = 0; j < numCols - 2; j++) {
                    if (Random.value > 0.5) {
                        for (int k = c; k < c + ColWallWidth[j]; k++) {
                            grid[k - 1, r - 1] = true;
                            grid[k - 1, r] = true;
                            grid[k - 1, r + 1] = true;
                        }
                    }
                    c += ColWallWidth[j];
                    c += 3;
                }
            }

            r += 3;
        }

        //... and then between columns
        c = 4 + startX;
        for (int i = 0; i < numCols - 2; i++) {
            c += ColWallWidth[i];

            // c-1, c, c+1 are x values of a player path

            if (Random.value > 0.5) {

                r = 4 + startY;
                for (int j = 0; j < numRows - 2; j++) {
                    if (Random.value > 0.5) {
                        for (int k = r; k < r + RowWallWidth[j]; k++) {
                            grid[c - 1, k - 1] = true;
                            grid[c, k - 1] = true;
                            grid[c + 1, k - 1] = true;
                        }
                    }
                    r += RowWallWidth[j];
                    r += 3;
                }
            }

            c += 3;
        }


        //eliminate dead ends
        bool found;
        do {//loop in case removing a dead end adds a new one
            found = false;
            for (int x = startX + 2; x < startX + xSize - 2; x++) {
                for (int y = startY + 2; y < startY + ySize - 2; y++) {
                    int xx = x;
                    while (DeadEndToLeft(xx, y)) {
                        grid[xx, y] = true;
                        grid[xx, y - 1] = true;
                        grid[xx, y + 1] = true;
                        xx++;
                        found = true;
                    }
                    xx = x;
                    while (DeadEndToRight(xx, y)) {
                        grid[xx, y] = true;
                        grid[xx, y - 1] = true;
                        grid[xx, y + 1] = true;
                        xx--;
                        found = true;
                    }
                    int yy = y;
                    while (DeadEndUp(x, yy)) {
                        grid[x, yy] = true;
                        grid[x - 1, yy] = true;
                        grid[x + 1, yy] = true;
                        yy++;
                        found = true;
                    }
                    yy = y;
                    while (DeadEndDown(x, yy)) {
                        grid[x, yy] = true;
                        grid[x - 1, yy] = true;
                        grid[x + 1, yy] = true;
                        yy--;
                        found = true;
                    }
                }
            }
        } while (found);
    }

    public void CarveRow(int y, int startX, int xSize) {
        for (int x = startX; x <= startX + xSize - 1; x++) {
            for (int j = y - 1; j <= y + 1; j++) {
                grid[x, j] = false;
            }
        }
    }

    public void CarveColumn(int x, int startY, int ySize) {
        for (int y = startY; y <= startY + ySize - 1; y++) {
            for (int j = x - 1; j <= x + 1; j++) {
                grid[j, y] = false;
            }
        }
    }

    public void Draw() {
        for (int x = 0; x < xSize; x++) {
            for (int y = 0; y < ySize; y++) {
                GameObject g = Instantiate(exterior, new Vector3(x, y, 10),
                    Quaternion.identity, transform);
                if (gsm.recordThisGame) {
                    recorder.RecordIfRecording(recorder.CreateCommand(g));
                }
                if (grid[x, y]) {

                    //instantiate solid regions
                    int directions = 0;

                    if (WallContinuesUp(x, y)) {//up
                        directions += 1;
                    }

                    if (WallContinuesDown(x, y)) {//down
                        directions += 4;
                    }
                    if (WallContinuesLeft(x, y)) {//left
                        directions += 8;
                    }
                    if (WallContinuesRight(x, y)) {//right
                        directions += 2;
                    }

                    if ((directions & 9) == 9 && !WallUpLeftConcave(x, y)) {//up left
                        g = Instantiate(solidRegions[0], new Vector3(x, y, 9),
                      Quaternion.identity, transform);
                        if (gsm.recordThisGame) {
                            recorder.RecordIfRecording(recorder.CreateCommand(g));
                        }

                        directions += 16;
                    }

                    if ((directions & 3) == 3 && !WallUpRightConcave(x, y)) {//up right
                        g = Instantiate(solidRegions[1], new Vector3(x, y, 9),
                        Quaternion.identity, transform);
                        if (gsm.recordThisGame) {
                            recorder.RecordIfRecording(recorder.CreateCommand(g));
                        }
                        directions += 32;
                    }
                    if ((directions & 6) == 6 && !WallDownRightConcave(x, y)) {//down right
                        g = Instantiate(solidRegions[2], new Vector3(x, y, 9),
                        Quaternion.identity, transform);
                        if (gsm.recordThisGame) {
                            recorder.RecordIfRecording(recorder.CreateCommand(g));
                        }
                        directions += 64;
                    }
                    if ((directions & 12) == 12 && !WallDownLeftConcave(x, y)) {//down left
                        g = Instantiate(solidRegions[3], new Vector3(x, y, 9),
                        Quaternion.identity, transform);
                        if (gsm.recordThisGame) {
                            recorder.RecordIfRecording(recorder.CreateCommand(g));
                        }
                        directions += 128;
                    }


                    //Find the correct wall tile
                    int wallTile = 5;//let default be vertical
                    int count = (directions & 16) / 16 + (directions & 32) / 32
                    + (directions & 64) / 64 + (directions & 129) / 128;
                    if (count == 1) {
                        if ((directions & 16) != 0) {
                            wallTile = 9;//up and left
                        } else if ((directions & 32) != 0) {
                            wallTile = 3;//up and right
                        } else if ((directions & 64) != 0) {
                            wallTile = 6;//down and right
                        } else if ((directions & 128) != 0) {
                            wallTile = 12;//down and left
                        }
                    } else if ((directions & 15) == 10) {//horizontal
                        wallTile = 10;
                    } else if ((directions & 48) == 48) {//horizontal
                        wallTile = 10;
                    } else if ((directions & 192) == 192) {//horizontal
                        wallTile = 10;
                    }

                    if (WallUpLeftConcave(x, y)) {
                        wallTile = 9;
                    }

                    if (WallUpRightConcave(x, y)) {
                        wallTile = 3;
                    }

                    if (WallDownRightConcave(x, y)) {
                        wallTile = 6;
                    }

                    if (WallDownLeftConcave(x, y)) {
                        wallTile = 12;
                    }

                    if (WallEndsUp(x, y)) {
                        wallTile = 4;
                    }

                    if (WallEndsDown(x, y)) {
                        wallTile = 1;
                    }

                    if (WallEndsLeft(x, y)) {
                        wallTile = 2;
                    }

                    if (WallEndsRight(x, y)) {
                        wallTile = 8;
                    }

                    if (WallLeftTee(x, y)) {
                        wallTile = 13;
                    }

                    if (WallRightTee(x, y)) {
                        wallTile = 7;
                    }

                    if (WallUpTee(x, y)) {
                        wallTile = 11;
                    }

                    if (WallDownTee(x, y)) {
                        wallTile = 14;
                    }


                    if (count != 4) {//count==4 means solid region, no wall
                        g = Instantiate(wallTiles[wallTile], new Vector3(x, y, 0),
                        Quaternion.identity, transform);
                        if (gsm.recordThisGame) {
                            recorder.RecordIfRecording(recorder.CreateCommand(g));
                        }

                    }
                }

            }
        }
    }

    public bool IsPlayerSpace(int x, int y) {
        if (AtOrBeyondEdge(x, y)) {
            return false;
        }
        for (int i = x - 1; i <= x + 1; i++) {
            for (int j = y - 1; j <= y + 1; j++) {
                if (grid[i, j]) {
                    return false;
                }
            }
        }
        return true;
    }

    public bool AtOrBeyondEdge(int x, int y) {
        return (y <= 0 || y >= ySize - 1 || x <= 0 || x >= xSize - 1);
    }

    public bool OutOfRange(int x, int y) {
        return (y < 0 || y > ySize - 1 || x < 0 || x > xSize - 1);
    }

    public bool WallContinuesUp(int x, int y) {
        if (OutOfRange(x, y + 1)) {
            return true;
        }
        return grid[x, y + 1];
    }

    public bool WallContinuesDown(int x, int y) {
        if (OutOfRange(x, y - 1)) {
            return true;
        }
        return grid[x, y - 1];
    }

    public bool WallContinuesRight(int x, int y) {
        if (OutOfRange(x + 1, y)) {
            return true;
        }
        return grid[x + 1, y];
    }

    public bool WallContinuesLeft(int x, int y) {
        if (OutOfRange(x - 1, y)) {
            return true;
        }
        return grid[x - 1, y];
    }

    public bool WallUpLeft(int x, int y) {
        if (OutOfRange(x - 1, y + 1)) {
            return true;
        }
        return grid[x - 1, y + 1];
    }

    public bool WallDownLeft(int x, int y) {
        if (OutOfRange(x - 1, y - 1)) {
            return true;
        }
        return grid[x - 1, y - 1];
    }

    public bool WallUpRight(int x, int y) {
        if (OutOfRange(x + 1, y + 1)) {
            return true;
        }
        return grid[x + 1, y + 1];
    }

    public bool WallDownRight(int x, int y) {
        if (OutOfRange(x + 1, y - 1)) {
            return true;
        }
        return grid[x + 1, y - 1];
    }

    public bool WallUpLeftConcave(int x, int y) {
        return WallContinuesLeft(x, y)
            && WallContinuesUp(x, y)
            && !WallUpLeft(x, y)
            && WallDownRight(x, y);
    }

    public bool WallDownLeftConcave(int x, int y) {

        return WallContinuesLeft(x, y)
            && WallContinuesDown(x, y)
            && !WallDownLeft(x, y)
            && WallUpRight(x, y);
    }

    public bool WallUpRightConcave(int x, int y) {

        return WallContinuesRight(x, y)
            && WallContinuesUp(x, y)
            && !WallUpRight(x, y)
            && WallDownLeft(x, y);
    }

    public bool WallDownRightConcave(int x, int y) {

        return WallContinuesRight(x, y)
            && WallContinuesDown(x, y)
            && !WallDownRight(x, y)
            && WallUpLeft(x, y);
    }

    public bool WallEndsUp(int x, int y) {
        return WallContinuesDown(x, y)
            && !WallContinuesUp(x, y)
             && !WallContinuesRight(x, y)
              && !WallContinuesLeft(x, y);
    }

    public bool WallEndsDown(int x, int y) {
        return !WallContinuesDown(x, y)
            && WallContinuesUp(x, y)
             && !WallContinuesRight(x, y)
              && !WallContinuesLeft(x, y);
    }

    public bool WallEndsLeft(int x, int y) {
        return !WallContinuesDown(x, y)
            && !WallContinuesUp(x, y)
             && WallContinuesRight(x, y)
              && !WallContinuesLeft(x, y);
    }

    public bool WallEndsRight(int x, int y) {
        return !WallContinuesDown(x, y)
            && !WallContinuesUp(x, y)
             && !WallContinuesRight(x, y)
              && WallContinuesLeft(x, y);
    }

    public bool WallLeftTee(int x, int y) {
        return WallContinuesDown(x, y)
           && WallContinuesUp(x, y)
            && !WallContinuesRight(x, y)
             && WallContinuesLeft(x, y)
              && (!WallUpLeft(x, y)
             || !WallDownLeft(x, y));
    }

    public bool WallRightTee(int x, int y) {
        return WallContinuesDown(x, y)
           && WallContinuesUp(x, y)
            && WallContinuesRight(x, y)
             && !WallContinuesLeft(x, y)
               && (!WallUpRight(x, y)
             || !WallDownRight(x, y));
    }

    public bool WallUpTee(int x, int y) {
        return !WallContinuesDown(x, y)
           && WallContinuesUp(x, y)
            && WallContinuesRight(x, y)
             && WallContinuesLeft(x, y)
             && (!WallUpLeft(x, y)
             || !WallUpRight(x, y));
    }

    public bool WallDownTee(int x, int y) {
        return WallContinuesDown(x, y)
           && !WallContinuesUp(x, y)
            && WallContinuesRight(x, y)
             && WallContinuesLeft(x, y)
             && (!WallDownLeft(x, y)
             || !WallDownRight(x, y));
    }

    public bool DeadEndToRight(int x, int y) {
        for (int dy = -2; dy <= 2; dy++) {
            if (dy < -1 || dy > 1) {
                if (!grid[x, y + dy]) {
                    return false;
                }
            } else if (grid[x, y + dy]) {
                return false;
            }
            if (!grid[x + 1, y + dy]) {
                return false;
            }
        }

        return true;
    }

    public bool DeadEndToLeft(int x, int y) {
        for (int dy = -2; dy <= 2; dy++) {
            if (dy < -1 || dy > 1) {
                if (!grid[x, y + dy]) {
                    return false;
                }
            } else if (grid[x, y + dy]) {
                return false;
            }
            if (!grid[x - 1, y + dy]) {
                return false;
            }
        }

        return true;
    }

    public bool DeadEndUp(int x, int y) {
        for (int dx = -2; dx <= 2; dx++) {
            if (dx < -1 || dx > 1) {
                if (!grid[x + dx, y]) {
                    return false;
                }
            } else if (grid[x + dx, y]) {
                return false;
            }
            if (!grid[x + dx, y - 1]) {
                return false;
            }
        }

        return true;
    }

    public bool DeadEndDown(int x, int y) {
        for (int dx = -2; dx <= 2; dx++) {
            if (dx < -1 || dx > 1) {
                if (!grid[x + dx, y]) {
                    return false;
                }
            } else if (grid[x + dx, y]) {
                return false;
            }
            if (!grid[x + dx, y + 1]) {
                return false;
            }
        }

        return true;
    }

}