using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public struct WinningStatus
{
    public int Red;
    public int Blue;
    public int Yellow;
    public int Green;

}
public class Dice : MonoBehaviourPun
{
    public enum Tokens
    {
        Blue,
        Red,
        Yellow,
        Green
    }

    public Tokens ActiveToken;
    public Tokens LocalToken;
    public int DiceNumber;
    public Text diceNumberDisplay;
    [HideInInspector] public int PreviousValue;
    public Button DiceButton;
    public bool snapCam;

    private Vector3 _camLerpPos;
    private Vector3 _camLerpRot;

    private Color _turnColor;
    public Camera cam;


    public Material board;
    public static Dice Instance;

    public WinningStatus _winningStatus;
    public int _PlayersFinished = 0;
    public TextMeshProUGUI PlayerWonDisplay;


    //AI
    private Ai_initialize _initialize;
    private Ai_movement _movement;
    private bool _allIn = true;

    private string _debugText;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    
    
    private void Start()
    {
        //Ai
        if (GameObject.Find("Ai_controller(initialize)") != null) {

            _initialize = Ai_initialize.Instance;
            _movement = Ai_movement.Instance;
        }

        _turnColor = new Color(0.6f, 0.6f, 0.6f);
        cam = GameObject.FindObjectOfType<Camera>();
        var diceDisplay = GameObject.FindGameObjectWithTag("DiceDisplay").transform;
        DiceButton = diceDisplay.GetChild(0).GetComponent<Button>();
        //RefreshPlayer();
    }
    public void RefreshPlayer()
    {
        photonView.RPC("RPC_SetBeginGame", RpcTarget.AllBuffered);
        LocalToken = NetworkManager.Instance.localPlayerToken;

        Debug.LogError("Current Active Token is ::::: " + ActiveToken);
        SetCamera();
        //photonView.RPC("UpdateTurn", RpcTarget.AllBuffered, ActiveToken, turncolorl);
        UpdateTurn();
    }
    [PunRPC]
    void RPC_SetBeginGame()
    {
        var customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        
        if (customProperties["MasterPlayerToken"] == null) return;
        var masterToken = (int)customProperties["MasterPlayerToken"];
        ActiveToken = (Tokens)masterToken;
    }
    public void SetCamera()
    {
        switch (LocalToken)
        {
            case Tokens.Green:
                _camLerpPos = new Vector3(-1, 28, 2);
                _camLerpRot = new Vector3(50, 135, 0);
                cam.backgroundColor = Color.green;
                break;

            case Tokens.Yellow:
                _camLerpPos = new Vector3(52, 28, -1);
                _camLerpRot = new Vector3(50, 225, 0);
                cam.backgroundColor = Color.yellow;
                break;

            case Tokens.Blue:
                _camLerpPos = new Vector3(52, 28, -52);
                _camLerpRot = new Vector3(50, 315, 0);
                cam.backgroundColor = Color.blue;
                break;

            case Tokens.Red:
                _camLerpPos = new Vector3(-4, 28, -52);
                _camLerpRot = new Vector3(50, 45, 0);
                cam.backgroundColor = Color.red;
                break;
        }
        switch (ActiveToken)
        {
            case Tokens.Red:
                _turnColor = Color.red;
                break;

            case Tokens.Green:
                _turnColor = Color.green;
                break;

            case Tokens.Yellow:
                _turnColor = Color.yellow;
                break;

            case Tokens.Blue:
                _turnColor = Color.blue;
                break;
        }
        cam.transform.position = _camLerpPos;
        cam.transform.eulerAngles = _camLerpRot;
        Debug.Log("SET CAMERA");
    }
    
    
    private void Update()
    {
        diceNumberDisplay.text = "Dice " + DiceNumber;
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GenerateRandomNumber(1);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            GenerateRandomNumber(2);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            GenerateRandomNumber(3);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateRandomNumber(4);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            GenerateRandomNumber(5);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            GenerateRandomNumber(6);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            StartCoroutine(UpdateCurrentToken());
        }
    }
    public void Replay()
    {
        NetworkManager.Instance.LeaveRoom();
        PhotonNetwork.LoadLevel("Lobby");

    }
    //[PunRPC]
    private void UpdateTurn()
    {
        var diceButtonColor = DiceButton.colors;
        diceButtonColor.normalColor = _turnColor;
        diceButtonColor.highlightedColor = _turnColor;
        diceButtonColor.disabledColor = new Color(_turnColor.r, _turnColor.g, _turnColor.b, .75f);
        DiceButton.colors = diceButtonColor;
        DiceButton.interactable = (LocalToken == ActiveToken);
    }
    public void GenerateRandomNumber(int numb = 0)
    {
        
        Debug.LogError("GenerateRandomNumber");
        
        DiceNumber = numb == 0 ? Random.Range(1, 7) : numb;
        
        photonView.RPC("RPC_DisplayDiceNumber", RpcTarget.AllBuffered, DiceNumber);
        PreviousValue = DiceNumber;
        /*if(GameManager.Instance.LocalPlayer.AllTokensInHouse && DiceNumber < 6)
        {            
            StartCoroutine(UpdateCurrentToken());
        }*/ // replacement below
        if (DiceNumber != 6)
            DiceButton.interactable = false;
                
        Debug.Log("before Check_All_In");
            CheckAllIn();

    }

    public void CheckAllIn() {

        _allIn = true;
        for (var i = 0; i < 4; i++)
            if (GameObject.Find(ActiveToken + "(Clone)").transform.GetChild(i).GetComponent<Follower>().PawnoutOfTheHouse)
                _allIn = false;
        
        Debug.Log("all in "+_allIn + " active "+ActiveToken+ "before switch");
        
        switch (ActiveToken)
        {

            case Tokens.Blue: 
                if (_initialize.tokenBlue && _allIn && DiceNumber < 6) {
                    Debug.Log("in switch active "+ActiveToken);
                    StartCoroutine(UpdateCurrentToken()); 
                }
                break;

            case Tokens.Yellow: 
                if (_initialize.tokenYellow && _allIn && DiceNumber < 6) {
                    Debug.Log("in switch active "+ActiveToken);
                    StartCoroutine(UpdateCurrentToken());
                }
                break;

            case Tokens.Red:
                if (_initialize.tokenRed && _allIn && DiceNumber < 6) {
                    Debug.Log("in switch active "+ActiveToken);
                    StartCoroutine(UpdateCurrentToken());
                }
                break;

            case Tokens.Green:
                if (_initialize.tokenGreen && _allIn && DiceNumber < 6) {
                    Debug.Log("in switch active "+ActiveToken);
                    StartCoroutine(UpdateCurrentToken());
                }
                break;
        }

    }

    public void CheckPlayerCollision()
    {
        photonView.RPC("RPC_CheckPlayerCollision", RpcTarget.OthersBuffered);
    }

    [PunRPC]
    public void RPC_CheckPlayerCollision()
    {
        foreach (var playerPrefab in FindObjectsOfType<Player>())
        {
            Debug.LogError("FindObjectsOfType" + playerPrefab.name);
            for (var i = 0; i < 4; i++)
                playerPrefab.transform.GetChild(i).GetComponent<Follower>().CheckPlayerCollision();
        }
    }
    [PunRPC]
    public void RPC_DisplayDiceNumber(int number)
    {
        DiceNumber = number;
        //diceNumberDisplay.text = number.ToString();
        Debug.LogError("RPC_DisplayDiceNumber" + number);
    }
    public IEnumerator UpdateCurrentToken()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("call from active "+ActiveToken);
        Debug.LogError("RPC_CheckForTurn***********************");
        photonView.RPC("RPC_CheckForTurn", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPC_CheckForTurn()
    {

        switch (ActiveToken)
        {
            case Tokens.Red:
                ActiveToken = Tokens.Green;
                _turnColor = Color.green;
                break;

            case Tokens.Green:
                ActiveToken = Tokens.Yellow;
                _turnColor = Color.yellow;
                break;

            case Tokens.Yellow:
                ActiveToken = Tokens.Blue;
                _turnColor = Color.blue;
                break;

            case Tokens.Blue:
                ActiveToken = Tokens.Red;
                _turnColor = Color.red;
                break;
        }
        Debug.LogError(ActiveToken + "'s Turn");

        UpdateTurn();

        //AI
        //if(PhotonNetwork.IsMasterClient) {
        if(_initialize.roomOwner) {
            Debug.Log("master client");
            Check_Ai_Turn();
        }
        else
            Debug.Log("not master");

    }

    public void Check_Ai_Turn()
    {

        switch (ActiveToken)
        {
            case Tokens.Blue: if (_initialize.tokenBlue == false)
                                    RollAndMoveforAI("Blue");
                break;

            case Tokens.Yellow: if (_initialize.tokenYellow == false)
                                    RollAndMoveforAI("Yellow");
                break;

            case Tokens.Red: if (_initialize.tokenRed == false)
                                RollAndMoveforAI("Red"); 
                break;

            case Tokens.Green: if (_initialize.tokenGreen == false)
                                    RollAndMoveforAI("Green");
                break;
        }

    }

    public void CallWinUpdateRPC(int reachedToken)
    {
        photonView.RPC("RPC_updateWinningStatus", RpcTarget.AllBuffered, reachedToken);
    }

    [PunRPC]
    public void RPC_updateWinningStatus(int reachedToken)
    {
        var _reachedToken = (Tokens)reachedToken;
        switch (_reachedToken)
        {
            case Tokens.Blue: _winningStatus.Blue += 1;
                                if (_winningStatus.Blue >= 4)
                                    StartCoroutine(PlayerWon(Tokens.Blue));
                                break;
            
            case Tokens.Red: _winningStatus.Red += 1;
                                if (_winningStatus.Red >= 4)
                                    StartCoroutine(PlayerWon(Tokens.Red));
                                break;
            
            case Tokens.Yellow: _winningStatus.Yellow += 1;
                                if (_winningStatus.Yellow >= 4)
                                    StartCoroutine(PlayerWon(Tokens.Yellow));
                                break;
            
            case Tokens.Green: _winningStatus.Green += 1;
                                if (_winningStatus.Green >= 4)
                                    StartCoroutine(PlayerWon(Tokens.Green));
                                break;
        }
        Debug.LogError("PLayer Reached :::::" + reachedToken);
    }

    private IEnumerator PlayerWon(Tokens finishedPlayerToken)
    {
        _PlayersFinished += 1;
        PlayerWonDisplay.gameObject.SetActive(true);
        _debugText += "\n" + finishedPlayerToken + " FINISHED " + _PlayersFinished;
        PlayerWonDisplay.text = finishedPlayerToken + " FINISHED " + _PlayersFinished;
        Debug.LogError("FinishedPlayerToken :::::" + finishedPlayerToken);

        yield return new WaitForSecondsRealtime(3f);
        PlayerWonDisplay.gameObject.SetActive(false);
    }

    private void RollAndMoveforAI(string tokenName)
    {
        GenerateRandomNumber(0);

        if (_allIn && DiceNumber < 6)
            StartCoroutine(UpdateCurrentToken());
        else
            _movement.dice_play(DiceNumber, tokenName);
        
        photonView.RPC("RPC_DisplayDiceNumber", RpcTarget.AllBuffered, DiceNumber);

    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), _debugText);
    }
}
