using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public float gameSpeed = 0.1f;
    public MapGenerator mapGenerator;

    private GamePhase gamePhase;
    private List<Player> players = new List<Player>();
    public Text highscoreText;


    // Use this for initialization
    void Start()
    {
        GameObject[] playerGameObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in playerGameObjects)
        {
            players.Add(p.GetComponent<Player>());
            p.GetComponent<Player>().SetGameManager(this);
        }

        StartCoroutine("StartGame");
    }

    public void RegisterPlayer(Player player)
    {
        players.Add(player);
    }

    public TileType LookAtMapPos(int x, int y)
    {
        return mapGenerator.LookTileTypeAt(HelperFunctions.mod(x, MapGenerator.columns), HelperFunctions.mod(y, MapGenerator.rows));
    }

    public bool IsCorrectPhase(GamePhase gamePhase)
    {
        return this.gamePhase == gamePhase;
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(1);
        mapGenerator.DrawMap();
        int gameRound = 0;
        while (gameRound < 50)
        {
            mapGenerator.GenerateNewMap();

            this.gamePhase = GamePhase.NewTurn;
            foreach (Player p in players)
            {
                p.StartNewRound();
            }

            int turn = 0;
            while (turn < 60)
            {
                yield return new WaitForSeconds(gameSpeed);

                this.gamePhase = GamePhase.NewTurn;
                foreach (Player p in players)
                {
                    p.StartNewTurn();
                }

                this.gamePhase = GamePhase.ActionPhase;
                foreach (Player p in players)
                {
                    p.ActionPhase();
                   
                }
                yield return new WaitForSeconds(gameSpeed);

                this.gamePhase = GamePhase.MovePhase;
                foreach (Player p in players)
                {
                    p.MovePhase();
                }

                yield return new WaitForSeconds(gameSpeed);

                this.gamePhase = GamePhase.EndOfTurnPhase;
                foreach (Player p in players)
                {
                    mapGenerator.TakeItem(p, players);
                    p.CheckIfTrapTriggers(players);

                }
                UpdateHighscoreText(players);
                turn++;
            }
        }
    }

    internal List<PlayerInfo> GetNearbyPlayers(int x, int y)
    {
        List<PlayerInfo> playerInfos = new List<PlayerInfo>();
        int sightDist = 3;

        foreach (Player p in players)
        {
            int dx = Math.Abs(x - p.GetXPos());
            int dy = Math.Abs(y - p.GetYPos());
            if (!(dx >= MapGenerator.columns - sightDist || dx <= sightDist))
            {
                continue;
            }
            if (!(dy >= MapGenerator.rows - sightDist || dy <= sightDist))
            {
                continue;
            }
            playerInfos.Add(new PlayerInfo(p));
        }

        return playerInfos;
    }

    internal List<Vector2Int> GetNearbyWalls(int posX, int posY)
    {
        return mapGenerator.GetNearbyWalls(posX, posY);
    }

    private void UpdateHighscoreText(List<Player> playerList)
    {
        playerList.Sort((p1, p2) => p2.Score.CompareTo(p1.Score));

        string scoreText = "";
     
        foreach (Player p in playerList)
        {
            scoreText += "<color=#" + ColorUtility.ToHtmlStringRGBA(p.GetColor()) + ">" +  p.Score + " : " + p.GetPlayerName() + "</color>" + System.Environment.NewLine;
        }

        highscoreText.text = scoreText;
    }


    public void SetGameSpeed(float f){
        gameSpeed = f;
    }
}


public enum GamePhase
{
    ActionPhase,
    MovePhase,
    EndOfTurnPhase,
    NewTurn
}
