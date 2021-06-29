using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class RoomListInfo : MonoBehaviourPun
{
    public RoomInfo _RoomInfo;
    public Text Name;

    public InputField _password;
    public GameObject _passwordScene;
    void Start()
    {
       // _password = GameObject.Find("passwordInputField").GetComponent<InputField>();
       // _passwordScene = GameObject.Find("password");
        
        
    }
    public void SetRoom()
    {
        Name.text = _RoomInfo.Name;
    }

    public void Input() {
        
        _passwordScene.SetActive(true);
    }
    public void OnClickJoinRoom() {
       
        transform.parent.gameObject.GetComponent<LobbyListManager>().host = false;
        

        if (_password.text != _RoomInfo.CustomProperties["password"].ToString()) {
            
            Debug.Log("wrong password");
            return;
        }

        Debug.Log("right password");
        
        PhotonNetwork.JoinRoom(_RoomInfo.Name);
    }
    
}
