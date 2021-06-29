using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;

public class LobbyListManager : MonoBehaviourPunCallbacks , ILobbyCallbacks

{
    Transform RoomListPanel;
    public GameObject RoomInfoPrefab;

    private List<GameObject> _list = new List<GameObject>();

    [HideInInspector] public bool host;

    public Text debug;

    private void Start() {

        host = true;
        RoomListPanel = this.transform;
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> roomList) {

        if (host == false) return;
        
        base.OnRoomListUpdate(roomList);
        Debug.LogError("OnRoomListUpdate");
        
        RefreshList(roomList);
    }

    private void RefreshList(List<RoomInfo> roomInfos) {
        
        debug.text += "\n Refreshing";

        /*while(RoomListPanel.childCount != 0)
        {
            Destroy(RoomListPanel.GetChild(0));    
        }
        foreach(RoomInfo _roominfo in roomInfos)
        {
            CreateRoomList(_roominfo);
        }*/
        foreach(var roomInfo in roomInfos) {

            if (roomInfo.RemovedFromList || !roomInfo.IsVisible) {
               
                var i = _list.FindIndex(x => x.gameObject.GetComponent<RoomListInfo>()._RoomInfo.Name == roomInfo.Name);
                
                if (i == -1) continue;
                
                Destroy(_list[i]);
                _list.RemoveAt(i);
            }
            else {
                
                if (transform.childCount > 0) {
                    
                    var id = transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().text;
                    for (var i = 0; i < transform.childCount; i++)
                        if (id == roomInfo.Name)
                            return;
                }

                CreateRoomList(roomInfo);
            }
        }
        
    }

    
    private void CreateRoomList(RoomInfo room) {
        
        debug.text += "\n Showing List";

        var cloneRoom = Instantiate(RoomInfoPrefab,RoomListPanel);

       if (cloneRoom == null) return;

       cloneRoom.GetComponent<RoomListInfo>()._RoomInfo = room;
       cloneRoom.GetComponent<RoomListInfo>().SetRoom();
       _list.Add(cloneRoom);
    }
}
