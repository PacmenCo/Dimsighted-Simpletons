using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerImpl : Player {

    public override string GetPlayerName() {
        return "Dimsighted simplebot";
    }
    MoveDirection moveDirection;
    public override void ActionPhase() {
        //List<Vector2Int> wallPositions = SenseNearbyWalls();
        if (UnityEngine.Random.Range(0, 2) == 1)
        {
            TradeActionsForExtraMovement();
        }
        /*


         Debug.Log(wallPositions.Count);
*/
        //List<PlayerInfo> nearbyPlayers = SenseNearbyPlayers();

        if (!isTrapActive())
        {
            PlaceTrap();
        }

        //Debug.Log(nearbyPlayers.Count);

        if (isTileFruit(LookAtTile(GetXPos(), GetYPos() + 1))){
            moveDirection = MoveDirection.UP;
        } else if (isTileFruit(LookAtTile(GetXPos(), GetYPos() - 1))) {
            moveDirection = MoveDirection.DOWN;
        } else if (isTileFruit(LookAtTile(GetXPos() - 1, GetYPos()))){
            moveDirection = MoveDirection.LEFT;
        } else if (isTileFruit(LookAtTile(GetXPos() + 1, GetYPos()))){
            moveDirection = MoveDirection.RIGHT;
        } else {
            moveDirection = (MoveDirection)UnityEngine.Random.Range(0, 4);   
        }

        //Debug.Log(GetActionsLeft());
        LookAtTile(GetXPos(), GetYPos()+5);
    }

    private bool isTileFruit(TileType tileType)
    {
        return tileType == TileType.FruitTile;
    }

    public override void MovePhase()
    {
        MoveInDirection(moveDirection);
        if (GetMovePointsLeft() > 0) {
            MoveInDirection((MoveDirection)UnityEngine.Random.Range(0, 4));
        }
    }

    public override void NewRound()
    {
        Debug.Log("New round started");
    }

  
}
