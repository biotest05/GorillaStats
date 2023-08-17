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
using System.Collections;

namespace GorillaServerStats
{
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin("com.thaterror404.gorillatag.SererStats", "ServerStats", "1.0.2")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance; // Singleton instance

        bool inRoom;
        public GameObject forestSign;
        public Text signText;
        public bool init;

        Coroutine timerCoroutine;
        System.TimeSpan time = System.TimeSpan.FromSeconds(0);
        string playTime = "00:00:00";

        private void Awake()
        {
            // Singleton pattern to ensure only one instance of this class
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[ServerStats] Plugin Awake and Singleton Instance set.");
                
                // Start the timer as soon as the plugin is loaded
                timerCoroutine = StartCoroutine(Timer());
            }
            else
            {
                Debug.LogWarning("[ServerStats] Multiple instances detected. Destroying...");
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
        }

        public string boardStatsUpdate()
        {
            if (PhotonNetwork.CurrentRoom == null)
            {
                Debug.LogError("[ServerStats] Current room is null");
                return "Hello! Thank you for using ServerStats!\r\n\r\nPlease join a room for stats to appear!";
            }
            
            var lobbyCode = PhotonNetwork.CurrentRoom.Name;
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            var master = PhotonNetwork.MasterClient;
            int totalPlayerCount = PhotonNetwork.CountOfPlayersInRooms;

            return "LOBBY CODE: " + lobbyCode + 
                "\r\nPLAYERS: " + playerCount + 
                "\r\nMASTER: " + master.NickName + 
                "\r\nTOTAL PLAYERS: " + totalPlayerCount +
                "\r\n\r\nPLAY TIME: " + playTime;

        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            Debug.Log("[ServerStats] Game Initialized.");
            init = true;
            forestSign = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/Tree Room Texts/WallScreenForest");
            if (forestSign == null)
            {
                Debug.LogError("[ServerStats] Could not find ForestSign");
                return;
            }
            signText = forestSign.GetComponent<Text>();
            if (signText == null)
            {
                Debug.LogError("[ServerStats] Could not find Text component for ForestSign");
                return;
            }
            if (PhotonNetwork.CurrentRoom == null)
            {
                Debug.LogError("[ServerStats] Current room is null");
                return;
            }
            else
            {
                signText.text = boardStatsUpdate();
            }
        }

        void OnEnable()
        {
            if (init)
            {
                if (forestSign != null)
                {
                    signText = forestSign.GetComponent<Text>();
                    signText.text = boardStatsUpdate();
                }
                else
                {
                    Debug.Log("[ServerStats] forestSign doesn't exist in OnJoin");
                }
            }
            else
            {
                Debug.Log("[ServerStats] Not initialized in OnEnable");
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
                    Debug.Log("[ServerStats] forestSign doesn't exist in OnDisable");
                }
            }
            else
            {
                Debug.Log("[ServerStats] Not initialized in OnDisable");
            }
        }

        void Update()
        {
            if (forestSign != null)
            {
                signText = forestSign.GetComponent<Text>();
                signText.text = boardStatsUpdate();
            }
            else
            {
                Debug.Log("[ServerStats] forestSign doesn't exist in Update");
            }
        }

        public void OnJoin(string gamemode)
        {
            Debug.Log("[ServerStats] Joined a room.");
            inRoom = true;

            // Ensure the Timer coroutine is correctly controlled
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
            }
            time = System.TimeSpan.FromSeconds(0); // Reset the time
            timerCoroutine = StartCoroutine(Timer());

            if (forestSign != null)
            {
                signText = forestSign.GetComponent<Text>();
                signText.text = boardStatsUpdate();
            }
            else
            {
                Debug.Log("[ServerStats] forestSign doesn't exist in OnJoin");
            }
        }

        public void OnLeave(string gamemode)
        {
            Debug.Log("[ServerStats] Left a room.");
            inRoom = false;

            // Stop the timer coroutine
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                time = System.TimeSpan.FromSeconds(0);
                playTime = "00:00:00";
            }

            if (forestSign != null)
            {
                signText = forestSign.GetComponent<Text>();
                signText.text = "WELCOME TO GORILLA TAG!\r\n\r\nPLEASE JOIN A ROOM FOR STATS TO APPEAR!";
            }
        }

        IEnumerator Timer()
        {
            Debug.Log("[ServerStats] Timer coroutine started.");
            while (true) // Run continuously
            {
                yield return new WaitForSeconds(1); 
                time = time.Add(System.TimeSpan.FromSeconds(1));
                playTime = time.ToString(@"hh\:mm\:ss");

                // Update signText directly here
                if (forestSign != null)
                {
                    signText = forestSign.GetComponent<Text>();
                    signText.text = boardStatsUpdate();
                }
                else
                {
                    Debug.LogWarning("[ServerStats] forestSign not found in Timer Coroutine.");
                }

                Debug.Log("[ServerStats] Timer Coroutine Running. Current playTime: " + playTime);
            }
        }
    }
}