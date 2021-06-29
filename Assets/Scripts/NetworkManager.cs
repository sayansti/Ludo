
using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NetworkManager :MonoBehaviourPunCallbacks,IPunObservable
{
    private string _debugText;
    public InputField roomIDField;
    public Button joinRoom;
    public Button createRoom;  

    //AI
    private Ai_initialize _initialize;

    public byte maxPlayer = 1;
    private byte _playersReady = 0;

    public Dice.Tokens localPlayerToken;
    public static NetworkManager Instance;

    public GameObject pickToken, noOfPlayer;
    public Button startGameButton;

    public Text debug;

    private void Awake()
    {
        if(Instance==null)
            Instance = this;
    }

    private void Start()
    {
        SetupLobbyMenu();

        ConnectToGameServer();
        Debug.LogError("ROOMID&&&&&&&&&& " + roomIDField.text);
    }
    private void Update()
    {
    }
    public void SetupLobbyMenu()
    {
        pickToken.SetActive(false);
        DontDestroyOnLoad(gameObject);
        joinRoom.interactable = false;
        createRoom.interactable = false;
        noOfPlayer.SetActive(false);
        startGameButton.gameObject.SetActive(false);

    }
    public void ConnectToGameServer()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        _debugText += "\n ConnectedToMaster";
        debug.text += _debugText;
        joinRoom.interactable = true;
        createRoom.interactable = true;
        PhotonNetwork.JoinLobby();
    }
    public void OnClickCreateRoom()
    {
        noOfPlayer.SetActive(true);
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        Destroy(this.gameObject);
    }
    public void OnclickNoOfPlayers(int NO_ofplayer)
    {
        var roomId = Random.Range(100000, 999999).ToString();
        var newRoomOptions = new RoomOptions();
        newRoomOptions.MaxPlayers = (byte)NO_ofplayer;
        newRoomOptions.IsVisible = true;
        newRoomOptions.IsOpen = true;

        //TODO PASSWORD
        //var password = new Hashtable {{"password", RoomIDField.text}};
        newRoomOptions.CustomRoomProperties = new Hashtable {{"password", roomIDField.text}};
        newRoomOptions.CustomRoomPropertiesForLobby = new[] {"password"}; 
        
        PhotonNetwork.CreateRoom(roomId, newRoomOptions);
        
        _debugText += "\n RoomID: " + roomId;
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        noOfPlayer.SetActive(false);
        _playersReady = 0;
        _debugText += "\n Room Created :" + PhotonNetwork.CurrentRoom.Name;
        Debug.LogError("RoomCreated");
        Debug.LogError("MaxNumberOfPlayer" + PhotonNetwork.CurrentRoom.MaxPlayers);
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        _debugText += "\n Joined Room";
        _debugText +="\n Number of Players: " + PhotonNetwork.PlayerList.Length.ToString();

        PhotonNetwork.LoadLevel("MainLudo");   
        pickToken.SetActive(true);
        startGameButton.gameObject.SetActive(true);
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.interactable = true;
            startGameButton.GetComponentInChildren<Text>().text = "Start Game";
            Debug.LogError("MasterClient");
        }
        else
        {
            startGameButton.GetComponentInChildren<Text>().text = "Waiting for Host";
            startGameButton.interactable = false;
            Debug.LogError("You Ain't No Master");
        }
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        _debugText += "\n CreateRoomFailed: " + message;
    }
    public void JoinRandomRoom()
    {
        debug.text += "\n JoinRandomRoom";
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        _debugText = "\n Join Room Failed: " + message;
        debug.text += _debugText;
    }
    public void OnTokenPicked(int PlayerToken)
    {
        localPlayerToken = (Dice.Tokens)PlayerToken;
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
            {
                ["MasterPlayerToken"] = PlayerToken
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
        Dice.Instance.RefreshPlayer();
        GameManager.Instance.RefreshPlayer();
        for (var i = 0; i < pickToken.transform.childCount; i++)
            pickToken.transform.GetChild(i).GetComponent<Button>().interactable = false;
        
        photonView.RPC("RPC_ExcludeToken", RpcTarget.AllBuffered, PlayerToken);
    }

    [PunRPC]
    public void RPC_ExcludeToken(int token)
    {
        pickToken.transform.GetChild(token).GetComponent<Button>().interactable = false;
        _playersReady += 1;
        _debugText += "\n Color Excluded";
    }
    public void OnclickStartGame()
    {
        Debug.LogError("playersReady :" + _playersReady);
        Debug.LogError("MaxPlayers :" + PhotonNetwork.CurrentRoom.MaxPlayers);

        if (_playersReady != PhotonNetwork.CurrentRoom.MaxPlayers) return;
        
        photonView.RPC("RPC_StartGame", RpcTarget.AllBuffered);
        _initialize = Ai_initialize.Instance;
        _initialize.room_owner_controls_ai();
    }
    [PunRPC]
    public void RPC_StartGame()
    {
        pickToken.SetActive(false);
        startGameButton.gameObject.SetActive(false);
    }
    private void OnGUI()
    {
        GUI.Label(new UnityEngine.Rect(10, 10, 3000, 500), _debugText);
    }
    public void OnPhotonSerializeView(PhotonStream stream,PhotonMessageInfo info)	
    {

    }
    
    public void ChangeStatus(string input) {

        if (!PhotonNetwork.IsMasterClient) return;

        
        switch (input) {
            
            case "open": PhotonNetwork.CurrentRoom.IsOpen = true; break;
            
            case "close": PhotonNetwork.CurrentRoom.IsOpen = false; break;
            
            case "visible":PhotonNetwork.CurrentRoom.IsVisible = true; break;
            
            case "invisible": PhotonNetwork.CurrentRoom.IsVisible = false; break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
       
    }

    public void RefreshRoomList() {
        
        PhotonNetwork.JoinLobby();
    }


}

