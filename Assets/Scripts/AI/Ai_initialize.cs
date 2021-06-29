using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Serialization;


public class Ai_initialize : MonoBehaviourPun {
    
    private NetworkManager _manager;
    public Player[] playerPrefabsAI;

    //[HideInInspector]
    public bool roomOwner, tokenGreen, tokenBlue, tokenYellow, tokenRed;

    public  GameObject aiTokenBlue, aiTokenRed, aiTokenGreen, aiTokenYellow;

    public static Ai_initialize Instance;
    
    //if tokens are true then player else ai
    //public Follower[] ai_player = new Follower[4];


    private void Awake() {
        
        if(Instance == null)
            Instance = this;
        
        roomOwner = false;
    }

    private void Start() {

        DontDestroyOnLoad(gameObject);
        _manager = GameObject.Find("NetWorkManager").GetComponent<NetworkManager>();

    }

    public void room_owner_controls_ai() {

        if (!PhotonNetwork.IsMasterClient) return;
       
        roomOwner = true;
        StartCoroutine(WaitAndSpawnAi());

    }

    private IEnumerator WaitAndSpawnAi() {
        
            //TIME TO WAIT BEFORE SPAWNING AI
            yield return new WaitForSeconds(1f);
            
            photonView.RPC("SetAI", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void SetAI() {
        
            //CHECK FOR PLAYERS
            if(GameObject.Find("Green(Clone)") != null)
                tokenGreen = true;
            if(GameObject.Find("Yellow(Clone)") != null)
                tokenYellow = true;
            if(GameObject.Find("Red(Clone)") != null)
                tokenRed = true;
            if(GameObject.Find("Blue(Clone)") != null)
                tokenBlue = true;

            if (roomOwner != true) return;
            //SPAWN AI
            if(tokenBlue == false)
                aiTokenBlue = PhotonNetwork.Instantiate(playerPrefabsAI[0].gameObject.name, playerPrefabsAI[0].transform.position,playerPrefabsAI[0].transform.rotation);
            if(tokenRed == false)
                aiTokenRed = PhotonNetwork.Instantiate(playerPrefabsAI[1].gameObject.name, playerPrefabsAI[1].transform.position,playerPrefabsAI[1].transform.rotation);
            if(tokenYellow == false)
                aiTokenYellow = PhotonNetwork.Instantiate(playerPrefabsAI[2].gameObject.name, playerPrefabsAI[2].transform.position,playerPrefabsAI[2].transform.rotation);
            if(tokenGreen == false)
                aiTokenGreen = PhotonNetwork.Instantiate(playerPrefabsAI[3].gameObject.name, playerPrefabsAI[3].transform.position,playerPrefabsAI[3].transform.rotation);
    }


}
