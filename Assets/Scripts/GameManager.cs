using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;


public class GameManager : NetworkBehaviour
{

    public int numPlayersInLobby = 0;
    public int numPlayersReady = 0;
    public int whoIsIt;
    public TMP_Text whoIsItText;
    public TMP_Text boostText;

    public GameObject readyUpUI, readyText, readyButton, tagUI, boostUI;

    public GameObject[] spawnPoints;

    public void ClientJoined()
    {
        //Update number of players in lobby
        CmdClientJoined();
    }

     [Command(requiresAuthority = false)]
    void CmdClientJoined(){
        numPlayersInLobby++;
        RpcPlayerJoined(numPlayersInLobby);
    }

    [Command(requiresAuthority = false)]
    void CmdPlayerReadied(){
        //check is new number of players ready is equal to the number of players in the lobby
        numPlayersReady++;
        if(numPlayersReady == numPlayersInLobby){
            //start the game
            StartGame();
        }
        RpcPlayerReadied(numPlayersReady);
    }

    public void StartGame(){
       //randonmly select a player to be it
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        whoIsIt = Random.Range(1, players.Length);

        //tell all clients to start the game
        RpcStartGame(players, whoIsIt);
    }

    [Command(requiresAuthority = false)]
    void CmdUpdateWhoIsIt(int newWhoIsIt){
        whoIsIt = newWhoIsIt;
        RpcUpdateWhoIsIt(newWhoIsIt);
    }

    [ClientRpc]
    private void RpcPlayerJoined(int newNumPlayers){
        numPlayersInLobby = newNumPlayers;
        //enable the ready up UI
        readyUpUI.SetActive(true);
        //Update ready text
        readyText.GetComponent<TextMeshProUGUI>().text = numPlayersReady + "/" + newNumPlayers + " Ready";
        //update text above all players heads to show their player ID
        GameObject[] localPlayers = GameObject.FindGameObjectsWithTag("Player");
        for(int i = 0; i < localPlayers.Length; i++)
        {
            localPlayers[i].GetComponent<PlayerNetwork>().playerIDText.text = "Player " + (i + 1);
            localPlayers[i].GetComponent<PlayerNetwork>().playerID = (i + 1);
        }
    }

    [ClientRpc]
    private void RpcPlayerReadied(int newNumPlayers){
        numPlayersReady = newNumPlayers;
        //Update ready text
        readyText.GetComponent<TextMeshProUGUI>().text = newNumPlayers + "/" + numPlayersInLobby + " Ready";
        //Make the boost UI visible

    }

    [ClientRpc]
    private void RpcUpdateWhoIsIt(int newWhoIsIt){
        whoIsIt = newWhoIsIt;
        whoIsItText.GetComponent<TextMeshProUGUI>().text = newWhoIsIt + " is it!";
    }

    [ClientRpc]
    private void RpcStartGame(GameObject[] players, int newWhoIsIt){
        //update who is it
        whoIsIt = newWhoIsIt;
        //set who is it text
        whoIsItText.GetComponent<TextMeshProUGUI>().text = newWhoIsIt + " is it!";
        //disable the ready up UI
        readyUpUI.SetActive(false);
        //enable the tag UI
        tagUI.SetActive(true);
        //enable the boost UI
        boostUI.SetActive(true);
        
        for(int i = 0; i < players.Length; i++)
        {
            //move the characters to their starting positions
            players[i].transform.position = spawnPoints[i].transform.position;
        }
    
    }

    public void ReadyUp()
    {
        //disable the ready up button
        readyButton.SetActive(false);
        CmdPlayerReadied();
    }

    public void TryTag(GameObject player)
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position + player.transform.forward.normalized*1.7f, 1.7f);
        int i = 0;
        while (i < hitColliders.Length)
        {
            if (hitColliders[i].gameObject.tag == "Player" && hitColliders[i].gameObject != player)
                CmdUpdateWhoIsIt(hitColliders[i].gameObject.GetComponent<PlayerNetwork>().playerID);

            i++;
        }
    }

}
