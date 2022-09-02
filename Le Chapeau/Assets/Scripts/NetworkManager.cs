using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    // instance
    public static NetworkManager instance;

    void Awake()
    {
        // if an instance already exists and its not this one, destroy us
        if (instance != null && instance != this)
            gameObject.SetActive(false);
        else
        {
            //set instance
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        // connect to master server
        PhotonNetwork.ConnectUsingSettings();
    }


    // attemts to create a room
    public void CreateRoom (string roomName)
    {
        PhotonNetwork.CreateRoom(roomName);
    }

    // attempts to join a room
    public void JoinRoom (string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    //changesd the scene using Photon's system
    [PunRPC]
    public void ChangeScene (string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
}
