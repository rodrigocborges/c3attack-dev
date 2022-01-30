using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField] private InputField playerNameField;
    [SerializeField] private InputField roomNameField;
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject connectedUIPanel;
    [SerializeField] private TMP_Text playerList;
    [SerializeField] private GameObject btnStartGame;

    private string playerName = "";
    private string roomName = "";

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();

        connectedUIPanel.SetActive(false);
        loadingPanel.SetActive(true);
        roomPanel.SetActive(false);

        playerName = string.Format("Player {0}", Random.Range(0, 9999));
        roomName = "dev";

        SetPlayerName(playerName);
        SetRoomName(roomName);
    }

    private void Update()
    {
        SetPlayerName(playerNameField.text);
        SetRoomName(roomNameField.text);
    }

    public void SetPlayerName(string newPlayerName)
    {
        playerName = newPlayerName;
        PhotonNetwork.NickName = playerName;
    }

    public void SetRoomName(string newRoomName)
    {
        roomName = newRoomName;
    }

    public void HostGame()
    {
        if (string.IsNullOrEmpty(roomName))
            roomName = "dev";
        PhotonNetwork.CreateRoom(roomName, new Photon.Realtime.RoomOptions { IsVisible = true, MaxPlayers = 4, IsOpen = true });

        connectedUIPanel.SetActive(false);
        loadingPanel.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        loadingPanel.SetActive(true);
        roomPanel.SetActive(false);
    }

    public void JoinGame()
    {
        if (string.IsNullOrEmpty(roomName))
            roomName = "dev";
        PhotonNetwork.JoinRoom(roomName);

        connectedUIPanel.SetActive(false);
        loadingPanel.SetActive(true);
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }

    //Callbacks
 
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        connectedUIPanel.SetActive(true);
        loadingPanel.SetActive(false);

        PhotonNetwork.NickName = playerName;
    }

    public override void OnJoinedRoom()
    {
        roomPanel.SetActive(true);
        loadingPanel.SetActive(false);

        playerList.text = string.Empty; //Reseta a lista para quando o jogador tiver entrado novamente não aparecer os antigos nomes de players

        foreach (var player in PhotonNetwork.PlayerList)
        {
            playerList.text += player.NickName + "\n";
        }

        btnStartGame.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        btnStartGame.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(string.Format("Join Room Failed ({0}): {1}", returnCode, message));
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(string.Format("Create Room Failed ({0}): {1}", returnCode, message));
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        playerList.text += newPlayer.NickName + "\n";
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int indexPlayerName = playerList.text.IndexOf(otherPlayer.NickName);
        if(indexPlayerName >= 0)
        {
            playerList.text = playerList.text.Remove(indexPlayerName);
        }
    }

}
