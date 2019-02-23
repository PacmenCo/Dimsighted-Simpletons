using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour {

    private Player player;

    public PlayerInfo(Player p){
        player = p;       
    }

    public Vector2Int GetPlayerPosition(){
        return new Vector2Int(player.GetXPos(), player.GetYPos());
    }

    public int GetScore() {
        return player.Score;
    }
}
