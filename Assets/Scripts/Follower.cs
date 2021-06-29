using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using Photon.Pun;

  public class Follower : MonoBehaviourPun
  {
     public PathCreator pathCreator;
     private Dice _dice; 
     EndOfPathInstruction endOfPathInstruction;
     private float speed = 2.98f;
     public float distanceTravelled;
     public bool canplay;
     [HideInInspector] public Vector3 InHousePos;
     public bool PawnoutOfTheHouse;
     public bool canCollide;
     
     
     public bool checkOpponentCollide = false;
     Collider col;
     
     //AI
     private Ai_initialize _initialize;

    private void Awake()
    {
        InHousePos = transform.position;
    }

    private void Start()
    {
        _dice = FindObjectOfType<Dice>();
        col = GetComponent<Collider>();
        canplay = true;

        //AI
        _initialize = Ai_initialize.Instance;

        //dice.CheckForTurn();
    }

    private void OnMouseDown()
    {
       
        var currentToken = _dice.ActiveToken;
        switch(currentToken)
        {
        case Dice.Tokens.Red: if(gameObject.CompareTag("RedPawn") && photonView.IsMine)
                                CheckForTheStart();
            break;
        
        case Dice.Tokens.Yellow: if (gameObject.CompareTag("YellowPawn") && photonView.IsMine)
                                    CheckForTheStart();
            break;
        
        case Dice.Tokens.Green: if (gameObject.CompareTag("GreenPawn") && photonView.IsMine)
                                    CheckForTheStart();
            
            break;
        
        case Dice.Tokens.Blue: if(gameObject.CompareTag("BluePawn") && photonView.IsMine)
                                    CheckForTheStart();
            break;
        }
        
    }
    
    
    public void CheckForTheStart()
    {

        if (_dice.DiceNumber == 6)
        {       
            if (!PawnoutOfTheHouse)
            {
                canplay = false;
                MovePlayerToStartPos();
                GameManager.Instance.LocalPlayer.AllTokensInHouse = false;               
            }
            else
            {
                if (canplay)
                {
                    StartCoroutine(Hold());
                }
            }
        }
        else
        {
            if(PawnoutOfTheHouse)
            {
                StartCoroutine(Hold());        
            }
            /*else
            {
                dice.DiceNumber = 0;
                dice.DiceButton.interactable = true;
                StartCoroutine(Dice.Instance.UpdateCurrentToken());
            }*/ // player can mistakely click on in house token
            
        }

    }

    private void MovePlayerToStartPos()
    {
        canCollide = true;
        StartCoroutine(PauseNturnover());
        print("Player will go to start Position");
        //transform.position = manager.StartPos.position;
        transform.position = pathCreator.path.GetPointAtDistance(0);
        transform.rotation = pathCreator.path.GetRotationAtDistance(0);
        PawnoutOfTheHouse = true;
        canplay = true;
        _dice.DiceNumber = 0;
        _dice.diceNumberDisplay.text = "Dice " + _dice.DiceNumber;
        _dice.DiceButton.interactable = true;

        //AI
        StartCoroutine(check_for_ai_2_turn());

    }

    public IEnumerator check_for_ai_2_turn() {

        yield return new WaitForSeconds(1f);

        switch (_dice.ActiveToken.ToString()) {

            case "Blue" :   if(_initialize.tokenBlue == false)
                                _dice.Check_Ai_Turn(); break;

            case "Yellow" :   if(_initialize.tokenYellow == false)
                                _dice.Check_Ai_Turn(); break;

            case "Red" :   if(_initialize.tokenRed == false)
                                _dice.Check_Ai_Turn(); break;

            case "Green" :   if(_initialize.tokenGreen == false)
                                _dice.Check_Ai_Turn(); break;
        }

    }

    private IEnumerator Hold()
    {
        _dice.DiceButton.interactable = false;
        var remainingDiceNumber = _dice.DiceNumber * 2.98f + distanceTravelled;
        
        if(remainingDiceNumber>=182f)
            StartCoroutine(TurnOver());
        else
        {
            photonView.RPC("RPC_ToggleTokenCollision", RpcTarget.AllBuffered, false);
            for (var i = 0; i < _dice.DiceNumber; i++)
            {

                yield return new WaitForSeconds(.15f);
                distanceTravelled += speed;
                //print(distanceTravelled);
                transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled);
                transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled);
               // transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled);   

                if (distanceTravelled > 181f)
                {
                    StartCoroutine(TurnOver());
                    //print("plus 1");
                    Destroy(gameObject);
                    Dice.Instance.CallWinUpdateRPC((int)_dice.ActiveToken);
                }
            }
            //dice.CheckForTurn();
           
        }
        //canCollide = true;
        StartCoroutine(PauseNturnover());
        StartCoroutine(TurnOver());
        StartCoroutine(_dice.DiceNumber != 6 ? Dice.Instance.UpdateCurrentToken() : check_for_ai_2_turn());
    }

    private IEnumerator TurnOver()
    {
        //canplay = false;
        // dice.CheckForTurn();
        if(_dice.DiceNumber == 6)
         yield return null;

        _dice.DiceNumber = 0;

        _dice.diceNumberDisplay.text = "Dice " + _dice.DiceNumber;

        //_dice.DiceButton.interactable = true;   
        yield return new WaitForSeconds(.5f);

        photonView.RPC("RPC_ToggleTokenCollision", RpcTarget.AllBuffered, true);
        //print("TurnOver");
    }

    private IEnumerator PauseNturnover()
    {
        yield return new WaitForSeconds(0.5f);
        //TurnOver();
        canCollide = true;
    }

    [PunRPC]
    public void RPC_ToggleTokenCollision(bool value)
    {
        GetComponent<BoxCollider>().enabled = value;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("SafeZone"))//&&(other.gameObject.T))
        {

            canCollide = false;   
            //GetComponent<BoxCollider>().enabled = false;
            //transform.position += new Vector3(0,1f, 0);
            //transform.localScale = new Vector3(1, 1.25f, 1);
        }

        if (canCollide)
        {
            if (!other.gameObject.CompareTag(gameObject.tag) && !gameObject.CompareTag(Dice.Instance.ActiveToken + "Pawn"))
            {
                if(!other.CompareTag("SafeZone"))
                {
                    Debug.LogError("reset the " + other.gameObject.tag + " position");
                    //photonView.RPC("RPC_CheckPlayerCollision",RpcTarget.All); 
                    ResetPlayer(this);
                }       
            }
            else 
            {
                    var folsc = other.gameObject.GetComponent<Follower>();
                    
                    var gameObjectTag = gameObject.tag;

                    var localTransform = transform;
                    localTransform.localScale = new Vector3(1.4f, 2, 1.4f);

                    switch (gameObjectTag)
                    {
                        case "BluePawn" : localTransform.localPosition += new Vector3(0.74f, 0, 0.74f);
                            break;
                        
                        case "RedPawn" : localTransform.localPosition += new Vector3(-0.74f, 0, 0.74f);
                            break;
                        
                        case "GreenPawn" : localTransform.localPosition += new Vector3(0.74f, 0, -0.74f);
                            break;
                        
                        case "YellowPawn" : localTransform.localPosition += new Vector3(-0.74f, 0, -0.74f);
                            break;
                        
                        default: Debug.Log("Not Colliding");
                            break;
                    }

            }
        }
    }
    public void CheckPlayerCollision()
    {
        checkOpponentCollide = true;
    }

    private void ResetPlayer(Follower folsc)
    {
                Debug.LogError("RPC_ResetPlayer");
                folsc.transform.position = folsc.InHousePos;
                folsc.PawnoutOfTheHouse = false;
                folsc.distanceTravelled = 0f;
                folsc.transform.eulerAngles = Vector3.zero;
                _dice.PreviousValue = 6;
    }
    private void OnTriggerStay(Collider other)
    {

        if (other.gameObject.CompareTag("SafeZone"))
        {
           // GetComponent<BoxCollider>().enabled = true;
        }
        // if(checkOpponentCollide)
        // {
        //     Debug.LogError("checkOpponentCollide IS TRUE");
        //     if (other.tag != gameObject.tag)
        //     {                
        //         Debug.LogError("other.tag != gameObject.tag IS TRUE");
        //         if(other.tag != "SafeZone")
        //         {                       
        //             Debug.LogError("SafeZone IS TRUE");
        //             ResetPlayer(this);
        //             checkOpponentCollide = false;
        //         }
        //     }
        // }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("SafeZone"))
        {
            canCollide = false;
            //transform.localScale = new Vector3(1.5f, 2f, 1.5f);
        }
        
    }

  }


