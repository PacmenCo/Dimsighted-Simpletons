
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player : MonoBehaviour
{

    private Color playerColor; public Color GetColor() { return playerColor; }
    private bool wasHitByTrap = false; public void WasHitByTrap() { if (!gameManager.IsCorrectPhase(GamePhase.EndOfTurnPhase)) { return; } wasHitByTrap = true; }

    private GameManager gameManager;
    private GameObject player;

    private int actionsLeft = 0;
    private int moveActions = 0;

    private int score = 0;
    private Vector2Int position;
    private GameObject trap;
    private Vector2Int trapPosition;

    public int Score
    {
        get
        {
            return score;
        }
    }

    //Legit to use
    public Vector2Int GetPosition(){
        return position;
    }

    //Legit to use
    public int GetXPos()
    {
        return position.x;
    }

    //Legit to use
    public int GetYPos()
    {
        return position.y;
    }

    //Legit to use
    public int GetActionsLeft()
    {
        return actionsLeft;
    }

    //Legit to use
    public int GetMovePointsLeft()
    {
        return moveActions;
    }

    //Byt 3 actions på 1 extra move
    //MAN SPISER DER HVOR MAN STÅR VED RUNDENS SLUTNING; EFTER MOVE PHASE ER DONE
    public void TradeActionsForExtraMovement()
    {
        if (!PayActionPoints(3))
        {
            return;
        }
        moveActions++;
    }


    //Placér en fælde hvor du selv står. (Sørg for at bevæg dig væk fra den med det samme, ellers klapper den om dig selv og udløser en kedelig præmie)
    //Fælder kan ikke placeres på tiles med bær eller lort.
    //Du kan maximalt have 1 fælde ude af gangen - Sættes en ny fælde fjernes den gamle
    //Fælder forsvinder når en spiller rammes af dem.
    //Fælder er 100% usynlige for andre spillere.
    public void PlaceTrap()
    {
        if (!PayActionPoints(2))
        {
            return;
        }

        if (LookAtTile(position.x, position.y) != TileType.NormalTile) {
            return;
        }
        Destroy(trap);

        trap = Instantiate(Resources.Load<GameObject>("Prefabs/Trap"), new Vector3((float)position.x, (float)position.y, 0.1f), Quaternion.identity);
        trap.GetComponent<SpriteRenderer>().color = playerColor - new Color(0, 0, 0, 0.35f);
        trapPosition = new Vector2Int(position.x, position.y);
    }

    //Returnerer true hvis du har en trap på map
    public bool isTrapActive() {
        return trap != null;
    }

    //byt moveaction for 2 actions.
    public void TradeMovementForActions()
    {
        if (moveActions > 0)
        {
            moveActions--;
            actionsLeft += 2;
        }
    }

    //Se på et tile - cost 1 action
    public TileType LookAtTile(int x, int y)
    {
        if (!PayActionPoints(1))
        {
            return TileType.NormalTile;  //ahaha
        }

        return gameManager.LookAtMapPos(x, y);
    }


    //finder players rundt om playeren (i et 7x7 område, centrum i spiller position) - Cost 1 action
    public List<PlayerInfo> SenseNearbyPlayers()
    {
        if (!PayActionPoints(1))
        {
            return new List<PlayerInfo>();
        }
        return gameManager.GetNearbyPlayers(position.x, position.y);
    }


    //finder walls rundt om playeren (i et 5x5 område, centrum i spiller position) - Cost 2 actions
    public List<Vector2Int> SenseNearbyWalls()
    {
        if (!PayActionPoints(2))
        {
            return new List<Vector2Int>();
        }
        return gameManager.GetNearbyWalls(position.x, position.y);
    }

    //Flytter spilleren i en given retning
    public void MoveInDirection(MoveDirection moveDirection)
    {
        if (!gameManager.IsCorrectPhase(GamePhase.MovePhase) || !payMovePoint(1))
        {
            return;
        }

        int newXPos = position.x;
        int newYPos = position.y;

        switch (moveDirection)
        {
            case MoveDirection.DOWN:
                newYPos = HelperFunctions.mod(position.y - 1, MapGenerator.rows);
                break;
            case MoveDirection.UP:
                newYPos = HelperFunctions.mod(position.y + 1, MapGenerator.rows);
                break;
            case MoveDirection.LEFT:
                newXPos = HelperFunctions.mod(position.x - 1, MapGenerator.columns);
                break;
            case MoveDirection.RIGHT:
                newXPos = HelperFunctions.mod(position.x + 1, MapGenerator.columns);
                break;
        }

        if (gameManager.LookAtMapPos(newXPos, newYPos) == TileType.WallTile)
        {
            return;
        }

        position = new Vector2Int(newXPos, newYPos);

        player.transform.position = new Vector2(position.x, position.y);
    }



    /**
     * Metoder nedenfor er not for use
     * Metoder nedenfor er not for use
     * Metoder nedenfor er not for use
     * Metoder nedenfor er not for use!
    **/



    public abstract string GetPlayerName();

    public abstract void ActionPhase();

    public abstract void MovePhase();

    public abstract void NewRound();

    internal void AwardPoints(int i)
    {
        if (!gameManager.IsCorrectPhase(GamePhase.EndOfTurnPhase))
        {
            return;
        }
        score += i;
    }

    internal void SetGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    private bool payMovePoint(int cost)
    {
        if (moveActions >= cost)
        {
            moveActions -= cost;
            return true;
        }
        return false;
    }

    private bool PayActionPoints(int cost)
    {
        if (!gameManager.IsCorrectPhase(GamePhase.ActionPhase))
        {
            return false;
        }
        if (actionsLeft < cost)
        {
            return false;
        }
        actionsLeft -= cost;
        return true;
    }

    public void CheckIfTrapTriggers(List<Player> players){
        if (!gameManager.IsCorrectPhase(GamePhase.EndOfTurnPhase))
        {
            return;
        }
        if (trap == null){
            return;
        }

        bool trapWasTriggered = false;
        foreach(Player p in players) {
            if (p.position == trapPosition){
                trapWasTriggered = true;
                if (p == this) {
                    p.AwardPoints(-100);
                    continue;
                }
                p.WasHitByTrap();
                p.AwardPoints(-25);
                AwardPoints(115);

            }
        }

        if (trapWasTriggered)
        {
            Destroy(trap);
        }
    }

    public void StartNewRound()
    {
        if (!gameManager.IsCorrectPhase(GamePhase.NewTurn))
        {
            return;
        }

        Destroy(trap);
        position = new Vector2Int(MapGenerator.columns / 2, MapGenerator.rows / 2);

        if (player != null)
        {
            Destroy(player);
        }

        player = Instantiate(Resources.Load<GameObject>("Prefabs/PlayerSprite"), new Vector2(position.x, position.y), Quaternion.identity);
        player.GetComponent<SpriteRenderer>().color = playerColor;

        NewRound();
    }

    public void StartNewTurn()
    {
        if (!gameManager.IsCorrectPhase(GamePhase.NewTurn))
        {
            return;
        }

        //punchLocation = Vector2Int();
        if (wasHitByTrap) {
            actionsLeft = 3;
            moveActions = 0;
        }
        else {
            actionsLeft = 7;
            moveActions = 1;
        }
        wasHitByTrap = false;

    }

    public Color[] colors = { Color.red, Color.blue, Color.cyan, Color.green, Color.magenta, Color.yellow, Color.cyan, Color.gray };
    static int i = 0;
    private void Start()
    {
        if (i > colors.Length)
        {
            playerColor = new Color(UnityEngine.Random.Range(0,1f), UnityEngine.Random.Range(0, 1f),UnityEngine.Random.Range(0, 1f), 1);
        }
        else
        {
            playerColor = colors[i];
            i++;
        }
    }
}

public enum MoveDirection
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}