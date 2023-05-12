using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public Piece nextPiece { get; private set; }
    public Piece savedPiece { get; private set; }
    public Piece WastePiece { get; private set; }
    public GameObject GameoverUI;
    public Text scoretext;
    public Text scoretextover;
    public TetrominoData[] tetrominoes;
    public trashData[] blank;
    public Vector3Int spawnPosition;
    public bool HoldState;
    public static int Score;
    public static int gamestate;
    public static int Trashline;
    public Vector3Int previewPosition = new Vector3Int(8, 6, 0);
    public Vector3Int holdPosition = new Vector3Int(-10, 6, 0);
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int expPosition = new Vector3Int(-10, 5, 0);
    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }
    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();
        nextPiece = gameObject.AddComponent<Piece>();
        nextPiece.enabled = false;
        WastePiece = gameObject.AddComponent<Piece>();
        WastePiece.enabled = false;

        HoldState = false;
        gamestate = 1;
        Score = 0;
        Trashline = 3;
        savedPiece = gameObject.AddComponent<Piece>();
        savedPiece.enabled = false;
        for (int i = 0; i < this.tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
        for (int i = 0; i < this.blank.Length; i++)
        {
            this.blank[i].Initialize();
        }


    }
    private void Start()
    {
        GameoverUI.SetActive(false);
        gamestate = 1;
        SetNextPiece();
        SpawnPiece();
    }
    private void SetNextPiece()
    {
        if (nextPiece.cells != null)
        {
            Clear(nextPiece);
        }
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];
        nextPiece.Initialize(this, previewPosition, data);
        HoldState = false;
        Set(nextPiece);
    }
    private void SetHoldblock()
    {
        if (HoldState == false)
        {
            TetrominoData savedData = savedPiece.data;
            if (savedData.cells == null)
            {

                Clear(activePiece);
                savedPiece.Initialize(this, holdPosition, activePiece.data);
                activePiece.Initialize(this, this.spawnPosition, nextPiece.data);
                if (IsValidPosition(this.activePiece, this.spawnPosition))
                {
                    Set(this.activePiece);
                }
                Set(this.savedPiece);
                SetNextPiece();


            }
            else
            {
                TetrominoData tmp = savedData;
                if (savedData.cells != null)
                {
                    Clear(savedPiece);
                    Clear(activePiece);

                }
                savedPiece.Initialize(this, holdPosition, activePiece.data);
                Set(savedPiece);
                activePiece.Initialize(this, this.spawnPosition, tmp);
                if (IsValidPosition(this.activePiece, this.spawnPosition))
                {
                    Set(this.activePiece);
                }

            }
            HoldState = true;
        }
    }
    private void Update()
    {
        if (gamestate == 0)
        {
            GameoverUI.SetActive(true);

        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            SetHoldblock();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (gamestate == 0)
            {
                tilemap.ClearAllTiles();
                Score = 0;

                gamestate = 1;
                GameoverUI.SetActive(false);
            }

        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (gamestate == 1)
            {
                upperline(Trashline);
            }
        }

        scoretext.text = Score.ToString();
        scoretextover.text = Score.ToString();
    }
    public void SpawnPiece()
    {
        //int random = Random.Range(0, this.tetrominoes.Length);
        // TetrominoData data = this.tetrominoes[random];
        activePiece.Initialize(this, this.spawnPosition, nextPiece.data);
        if (IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(this.activePiece);

        }
        else
        {
            GameOver();
        }
        SetNextPiece();

    }


    private void GameOver()
    {
        //this.tilemap.ClearAllTiles();

        gamestate = 0;
    }
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);

        }
    }
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);

        }
    }
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }
    public void ClearLines() {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;
        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                Score++;
                LineClear(row);
            } else
            {
                row++;
            }
        }


    }
    private bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if (!this.tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
    }
    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);
                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);


            }
            row++;
        }
    }
    private void upperline(int trashline)
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMax + trashline;
        while (row >= bounds.yMin + trashline)
        {
            for (int X = bounds.xMin; X < bounds.xMax; X++)
            {
                Clear(activePiece);
                Vector3Int position = new Vector3Int(X, row - trashline, 0);
                TileBase down = this.tilemap.GetTile(position);
                position = new Vector3Int(X, row, 0);
                this.tilemap.SetTile(position, down);
                Vector3Int position1 = new Vector3Int(X, row - trashline, 0);

                this.tilemap.SetTile(position1, null);

            }
            row--;
        }
        

        trashData data = blank[0];
        WastePiece.Initialize1(this, expPosition, data);
        for (int j = 0; j < trashline; j++)
        {
            int random = Random.Range(bounds.xMin, bounds.xMax);

            for (int i = bounds.xMin; i < bounds.xMax; i++) { 
                
            
                if (i == random)
                    continue;
                Vector3Int tilePosition = new Vector3Int(i, j+bounds.yMin, 0);
                this.tilemap.SetTile(tilePosition, WastePiece.data1.tile);

            }
        }

    } 


}



