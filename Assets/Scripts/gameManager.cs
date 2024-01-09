using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;
public class gameManager : MonoBehaviour
{

    public static gameManager instance { get; protected set; }
   
    // Start is called before the first frame update
    void Start()
    {

        if (instance == null)
            instance = this;

    }

    public void LoadPlayerRoom() {

        if (SceneManager.GetSceneByName("playerRoom").isLoaded == false) {
            SceneManager.LoadSceneAsync("playerRoom", LoadSceneMode.Additive).completed += HandlePlayerRoomLoadCompleted;


        }
       

    }

    private void HandlePlayerRoomLoadCompleted(AsyncOperation obj)
    {

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("playerRoom"));

        Networking.instance.setAvatarPositionVariables(0 , 0, 0);


    }

    // Update is called once per frame
    void Update()
    {

       

    }


    
}
    