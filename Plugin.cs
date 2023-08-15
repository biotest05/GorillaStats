using System;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using Utilla;
using Bepinject;
using Photon.Pun;
using UnityEngine.Timeline;
using GorillaNetworking;
using System.Collections.Specialized;
using System.Net;
using HarmonyLib;
using HoneyLib;
using HoneyLib.Events;
using System.Timers;

namespace GorillaServerStats
{
    /// <summary>
    /// This is your mod's main class.
    /// </summary>

    /* This attribute tells Utilla to look for [ModdedGameJoin] and [ModdedGameLeave] */
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin("com.thaterror404.gorillatag.SererStats", "ServerStats", "1.0.2")]
    public class Plugin : BaseUnityPlugin
    {
        bool inRoom;
        private Timer timer;
        public GameObject forestSign;
        public Text signText;
        public bool init;
        public int roomTags = 0;


        void Awake()
        {

        }

        void Start()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
            HoneyLib.Events.Events.InfectionTagEvent += InfectionTagEvent;

        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            init = true;
            forestSign = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/Tree Room Texts/WallScreenForest");
        }

        void OnEnable()
        {
            if (init)
            {
                int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
                var master = PhotonNetwork.MasterClient;
                var lobbyCode = PhotonNetwork.CurrentRoom.Name;
                int totalPlayerCount = PhotonNetwork.CountOfPlayersInRooms;

                // Change the text on the sign
                if (forestSign != null)
                {
                    signText = forestSign.GetComponent<Text>();
                    signText.text = "LOBBY CODE: " + lobbyCode + "\r\nPLAYERS: " + playerCount + "\r\nMASTER: " + master.NickName + "\r\nTOTAL PLAYERS: " + totalPlayerCount + "\r\nMY TAGS: " + roomTags;
                }
                else
                {
                    Debug.Log("forestSign doesn't exist in OnJoin");
                }
            }
        }

        void OnDisable()
        {
            if (init)
            {
                if (forestSign != null)
                {
                    signText = forestSign.GetComponent<Text>();
                    signText.text = "WELCOME TO GORILLA TAG!\r\n\r\nPLEASE JOIN A ROOM FOR STATS TO APPEAR!";
                }
                else
                {
                    Debug.Log("forestSign doesn't exist in OnDisable");
                }
            }
        }
        void Update()
        {
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            var master = PhotonNetwork.MasterClient;
            var lobbyCode = PhotonNetwork.CurrentRoom.Name;
            int totalPlayerCount = PhotonNetwork.CountOfPlayersInRooms;

            // Change the text on the sign
            if (forestSign != null)
            {
                signText = forestSign.GetComponent<Text>();
                signText.text = "LOBBY CODE: " + lobbyCode + "\r\nPLAYERS: " + playerCount + "\r\nMASTER: " + master.NickName + "\r\nTOTAL PLAYERS: " + totalPlayerCount + "\r\nMY TAGS: " + roomTags;
            }
            else
            {
                Debug.Log("forestSign doesn't exist in OnJoin");
            }
        }
        public void OnJoin(string gamemode)
        {
            inRoom = true;

            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            var master = PhotonNetwork.MasterClient;
            var lobbyCode = PhotonNetwork.CurrentRoom.Name;
            int totalPlayerCount = PhotonNetwork.CountOfPlayersInRooms;

            // Change the text on the sign
            if (forestSign != null)
            {
                signText = forestSign.GetComponent<Text>();
                signText.text = "LOBBY CODE: " + lobbyCode + "\r\nPLAYERS: " + playerCount + "\r\nMASTER: " + master.NickName + "\r\nTOTAL PLAYERS: " + "\r\nMY TAGS: " + roomTags;
            }
            else
            {
                Debug.Log("forestSign doesn't exist in OnJoin");
            }
        }

        public void OnLeave(string gamemode)
        {
            inRoom = false;
            roomTags = 0;

            // Change the text on the sign
            signText = forestSign.GetComponent<Text>();
            signText.text = "WELCOME TO GORILLA TAG!\r\n\r\nPLEASE JOIN A ROOM FOR STATS TO APPEAR!";
        }

        void InfectionTagEvent(object sender, InfectionTagEventArgs e)
        {
            if (inRoom)
            {
                roomTags++;
                Update();
            }
        }
    }   
}
