
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public GameObject tilePrefab;
    public GameObject tileFogPrefab;
    public GameObject fruitPrefab;
    public GameObject wallPrefab;
    public GameObject excrementPrefab;
    public Dictionary<TileType, GameObject> tilePrefabs = new Dictionary<TileType, GameObject>();

    public static int columns = 16;
    public static int rows = 14;

    private Tile[,] tileMap;


    private void Start()
    {
        tilePrefabs.Add(TileType.FruitTile, fruitPrefab);
        tilePrefabs.Add(TileType.WallTile, wallPrefab);
        tilePrefabs.Add(TileType.NormalTile, tileFogPrefab);
        tilePrefabs.Add(TileType.ExcrementTile, excrementPrefab);
    }

    // Use this for initialization
    public void GenerateNewMap()
    {
        DestroyOldMap();

        CreateNewTilemap();

        SetupTileTypeOccurences(TileType.FruitTile, Random.Range(13, 23));
        SetupTileTypeOccurences(TileType.WallTile, Random.Range(37, 60));
        SetupNormalTiles();

        ClearStartZone();
        //DrawMap();
    }

    private void ClearStartZone(){
        for (int x = columns / 2 - 1; x <= columns / 2 + 1; x++)
        {
            for (int y = rows / 2 - 1; y <= rows / 2 + 1; y++)
            {
                if (tileMap[x, y].type == TileType.WallTile)
                {
                    tileMap[x, y].type = TileType.NormalTile;
                    Destroy(tileMap[x, y].tileGO);
                }
            }
        }
    }

    private void DestroyOldMap()
    {
        if (tileMap == null) {
            return;
        }
        foreach (Tile tile in tileMap) {
            Destroy(tile.tileGO);
            Destroy(tile.fogGO);
            //tile.
        }
        tileMap = new Tile[columns, rows];
    }

    private void SetupNormalTiles()
    {
        for (int x = 0; x < columns; x++) {
            for (int y = 0; y < rows; y++) {
                if (tileMap[x, y] == null) {
                    tileMap[x, y] = new Tile(TileType.NormalTile, null, SpawnTile(new Vector2Int(x, y), tileFogPrefab));
                }
            }
        }
    }

    void CreateNewTilemap(){
        tileMap = new Tile[columns, rows];
    }

    void SetupTileTypeOccurences(TileType tileType, int occurences){
        while (occurences > 0)
        {
            int xPos = Random.Range(0, columns);
            int yPos = Random.Range(0, rows);

            if (tileMap[xPos, yPos] == null) {
                tileMap[xPos, yPos] = new Tile(tileType, SpawnTile(new Vector2Int(xPos, yPos), tilePrefabs[tileType]), SpawnTile(new Vector2Int(xPos, yPos), tileFogPrefab));
                occurences--;
            }
        }
    }

    public void DrawMap(){
        for (int y = 0; y < rows; y++) {
            for (int x = 0; x < columns; x++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(x, y, 1), Quaternion.identity);
                tile.transform.parent = this.transform;
            }
        }
    }

    GameObject SpawnTile(Vector2Int spawnPos, GameObject prefab){
        GameObject tile = Instantiate(prefab, new Vector3(spawnPos.x, spawnPos.y, prefab.transform.position.z), Quaternion.identity);
        tile.transform.parent = this.transform;
        return tile;
    }

    internal List<Vector2Int> GetNearbyWalls(int posX, int posY)
    {
        List<Vector2Int> walls = new List<Vector2Int>();

        int x = posX - 2; //HelperFunctions.mod(posX - 2, columns);
        int xMax = posX + 3; //HelperFunctions.mod(posX + 2, columns);
        int yMin = posY - 2; //HelperFunctions.mod(posY - 2, rows);
        int yMax = posY + 3; //HelperFunctions.mod(posY + 2, rows);

        while (HelperFunctions.mod(x, MapGenerator.columns) != HelperFunctions.mod(xMax, MapGenerator.columns)){
            int y = yMin;
            while (HelperFunctions.mod(y, MapGenerator.rows) != HelperFunctions.mod(yMax, MapGenerator.rows)){
                if (tileMap[HelperFunctions.mod(x, MapGenerator.columns), HelperFunctions.mod(y, MapGenerator.rows)].type == TileType.WallTile){
                    walls.Add(new Vector2Int(HelperFunctions.mod(x, MapGenerator.columns), HelperFunctions.mod(y, MapGenerator.rows)));
                    LookTileTypeAt(HelperFunctions.mod(x, MapGenerator.columns), HelperFunctions.mod(y, MapGenerator.rows));
                }
                y++;
            }
            x++;
        }

        return walls;
    }

    public TileType LookTileTypeAt(int x, int y) {
        TileType ret = tileMap[x, y].LookAt();

        Destroy(tileMap[x, y].fogGO);
        //tileMap[x, y].type = TileType.NormalTile;

        return ret;
    }

    internal void TakeItem(Player p, List<Player> otherPlayers){
        int x = p.GetXPos();
        int y = p.GetYPos();
        TileType ret = tileMap[p.GetXPos(), p.GetYPos()].LookAt();
        Destroy(tileMap[p.GetXPos(), p.GetYPos()].fogGO);
        if (ret == TileType.NormalTile) {
            return;
        }

        if (tileMap[p.GetXPos(), p.GetYPos()].player == p) {
            return;
        }

        List<Player> playersAtSameSpot = new List<Player>();
        foreach (Player otherPlayer in otherPlayers){
            if (otherPlayer == p) {
                continue;
            }
            if (otherPlayer == tileMap[p.GetXPos(), p.GetYPos()].player) {
                continue;
            }
            if (otherPlayer.GetXPos() == x && otherPlayer.GetYPos() == y) {
                playersAtSameSpot.Add(otherPlayer);
            }
        }

        switch (ret)
        {
            case TileType.FruitTile:
                Destroy(tileMap[x, y].tileGO);
                if (playersAtSameSpot.Count == 0)
                {

                    tileMap[x, y] = new Tile(TileType.ExcrementTile, SpawnTile(new Vector2Int(x, y), excrementPrefab), null);
                    tileMap[x, y].player = p;
                    p.AwardPoints(50);
                }
                else {
                    tileMap[x, y].type = TileType.NormalTile;
                    p.AwardPoints(5);
                    foreach(Player op in playersAtSameSpot){
                        op.AwardPoints(5);
                    }
                }
                break;
            case TileType.ExcrementTile:
                Destroy(tileMap[x, y].tileGO);
                tileMap[x, y].type = TileType.NormalTile;

                if (playersAtSameSpot.Count == 0)
                {
                    p.AwardPoints(20);
                } else {
                    foreach (Player op in playersAtSameSpot)
                    {
                        op.AwardPoints(5);
                    }
                }
                break;
        }

    }
}
