using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class NetworkHUD : MonoBehaviour
{   
    private NetworkManager manager;

    public GameObject mainMenuCamera, mainMenuUI;

    // Start is called before the first frame update
    void Awake()
    {
        manager = GetComponent<NetworkManager>();
    }

    public void HostGame()
    {
        manager.StartHost();
        mainMenuCamera.SetActive(false);
        mainMenuUI.SetActive(false);
    }

    public void JoinGame()
    {
        manager.networkAddress = "localhost";
        manager.StartClient();
        mainMenuCamera.SetActive(false);
        mainMenuUI.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
