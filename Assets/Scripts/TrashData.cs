using UnityEngine;
using UnityEngine.Tilemaps;


public enum one
{
    white

}

[System.Serializable]
public struct trashData
{
    public one trash;
    public Tile tile;
    public Vector2Int[] cells { get; private set; }
    public void Initialize()
    {
        this.cells = Data.waste[this.trash];
        
    }


}
