using Mirror;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PlayerNetwork : NetworkBehaviour {
    public static GameManager gameManager;

    public int playerID;
    public TMP_Text playerIDText;

    #region Start
    
    public override void OnStartClient()
    {
        //turn off camera's that are not the local player's
        if(!isOwned){
            transform.Find("CameraController").gameObject.SetActive(false);
            return;
        }       

        //get the game manager
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        //let game manager know that a new player has joined
        gameManager.ClientJoined(); 

        //set the player's ID
        playerID = gameManager.numPlayersInLobby;

        //set the player's ID text
        playerIDText.text = "Player " + playerID;

    }
    
    #endregion

    public void ReadyUp(){
        //increase the number of players ready
        gameManager.ReadyUp();
    }

    
}