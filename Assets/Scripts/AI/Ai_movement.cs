using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai_movement : MonoBehaviour {
    
    private Ai_initialize _Initialize;
    private Dice _dice;
    private Follower[] _follower = new Follower[4];
    public static Ai_movement Instance;

    private void Awake() {
        
        if(Instance == null)
            Instance = this;
    }

    private void Start() {

        _Initialize = Ai_initialize.Instance;
        _dice = Dice.Instance;
    }
    
    public void dice_play(int diceNumber, string tokenName) {

    var allPawned = true;

        if(diceNumber == 6) {

            for (var i = 0; i < 4; i++) {
                
                _follower[i] = GameObject.Find(tokenName +"(Clone)").transform.GetChild(i).GetComponent<Follower>();
                    
                    if(_follower[i].PawnoutOfTheHouse == false) {
                    
                        allPawned = false;
                        _follower[i].CheckForTheStart();
                        break;
                    
                    }
            }

            if(allPawned) {

                    var i = Random.Range(0,4);

                    while (GameObject.Find(tokenName+"(Clone)").transform.GetChild(i).gameObject.activeInHierarchy == false)
                        i = Random.Range(0,4);
                    
                    _follower[i] = GameObject.Find(tokenName+"(Clone)").transform.GetChild(i).GetComponent<Follower>();
                    _follower[i].CheckForTheStart();

            }      

        }
        else {

            for (var i = 0; i < 4; i++) {
                
                _follower[i] = GameObject.Find(tokenName+"(Clone)").transform.GetChild(i).GetComponent<Follower>();
                    
                    if(_follower[i].PawnoutOfTheHouse) {
                    
                        _follower[i].CheckForTheStart();
                        break;
                    }
            }
        }

    }

    
}
