using System;
using UnityEngine;

public class Tile {

    public TileType type { get; set; }
    public GameObject tileGO { get; set; }
    public GameObject fogGO { get; }
    public Player player { get; set; }

    public Tile(TileType type, GameObject tileGO, GameObject fogGO){
        this.type = type;
        this.tileGO = tileGO;
        this.fogGO = fogGO;
    }

    internal TileType LookAt()
    {
        return type;
    }
}
