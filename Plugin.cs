using System;
using BepInEx;
using UnityEngine;
using System.Collections.Generic;
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
using Zenject;

namespace GorillaServerStats
{
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin("com.thaterror404.gorillatag.SererStats", "ServerStats", "1.0.2")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance; // Singleton instance

        bool inRoom;
        public List<GameObject> signs;
        public List<Text> signTexts;
        public bool init;
        PhotonNetworkController networkController;

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
            signs.Add(GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/Tree Room Texts/WallScreenForest"));
            signs.Add(GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/Tree Room Texts/WallScreenCave"));
            signs.Add(GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/Tree Room Texts/WallScreenCity Front"));
            signs.Add(GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/Tree Room Texts/WallScreenCanyon"));
            signs.Add(GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/Tree Room Texts/WallScreenSkyJungle"));
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
                "\r\nPLAY TIME: " + playTime +
                "\r\n\r\nMAP: " + getMap(networkController.currentGameType);

        }

        public string getMap(string map)
        {
            bool isMapDone = false;
            if (map.Contains("forest") && !isMapDone)
            {
                return "FOREST";
                isMapDone = true;
            }
            if (map.Contains("mountains") && !isMapDone)
            {
                return "MOUNTAINS";
                isMapDone = true;
            }
            if (map.Contains("city") && !isMapDone)
            {
                return "CITY";
                isMapDone = true;
            }
            if (map.Contains("canyons") && !isMapDone)
            {
                return "CANYONS";
                isMapDone = true;
            }
            if (map.Contains("beach") && !isMapDone)
            {
                return "BEACH";
                isMapDone = true;
            }
            if ((map.Contains("skyjungle") || map.Contains("clouds")) && !isMapDone)
            {
                return "CLOUDS";
                isMapDone = true;
            }
            if (!isMapDone && inRoom)
            { 
                return "GLITCH/NO MAP";
            }
            return "";
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            Debug.Log("[ServerStats] Game Initialized.");
            init = true;
            networkController = FindObjectOfType<PhotonNetworkController>();
            foreach(GameObject sign in signs)
            {
                try 
                { 
                    signTexts.Add(sign.GetComponent<Text>());
                } 
                catch
                {
                    Debug.LogError("[ServerStats] Could not find Text component for sign");
                    return;
                }
            }
            if (PhotonNetwork.CurrentRoom == null)
            {
                Debug.LogError("[ServerStats] Current room is null");
                return;
            }
            else
            {
                foreach(Text signText in signTexts) {
                    try
                    {
                        signText.text = boardStatsUpdate();
                    } catch
                    {
                        Debug.LogError("[ServerStats] Cannot update board stats!");
                    }
                }
            }
        }

        void OnEnable()
        {
            if (init)
            {
                foreach(Text signText in signTexts) {
                    try
                    {
                        signText.text = boardStatsUpdate();
                    }
                    catch
                    {
                        Debug.LogError("[ServerStats] Cannot update board stats!");
                    }
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
                // "WELCOME TO GORILLA TAG!\r\n\r\nPLEASE JOIN A ROOM FOR STATS TO APPEAR!"
                foreach (Text signText in signTexts) {
                    try
                    {
                        signText.text = "WELCOME TO GORILLA TAG!\r\n\r\nPLEASE JOIN A ROOM FOR STATS TO APPEAR!";
                    }
                    catch
                    {
                        Debug.LogError("[ServerStats] Cannot update board stats!");
                    }
                }
            }
            else
            {
                Debug.Log("[ServerStats] Not initialized in OnDisable");
            }
        }

        void Update()
        {
            foreach(Text signText in signTexts) {
                try
                {
                    signText.text = boardStatsUpdate();
                }
                catch
                {
                    Debug.LogError("[ServerStats] Cannot update board stats!");
                }
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

            foreach(Text signText in signTexts) {
                try
                {
                    signText.text = boardStatsUpdate();
                }
                catch
                {
                    Debug.LogError("[ServerStats] Cannot update board stats!");
                }
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

            foreach(Text signText in signTexts) {
                try
                {
                    signText.text = "WELCOME TO GORILLA TAG!\r\n\r\nPLEASE JOIN A ROOM FOR STATS TO APPEAR!";
                }
                catch
                {
                    Debug.LogError("[ServerStats] Cannot update board stats!");
                }
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
                foreach(Text signText in signTexts) {
                    try
                    {
                        signText.text = boardStatsUpdate();
                    }
                    catch
                    {
                        Debug.LogError("[ServerStats] Cannot update board stats!");
                    }
                }

                Debug.Log("[ServerStats] Timer Coroutine Running. Current playTime: " + playTime);
            }
        }
    }
}