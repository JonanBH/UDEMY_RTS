using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RTSCourse.Controllers;
using UnityEngine.SceneManagement;
using System;

namespace RTSCourse.Networking
{
    public class RTSNetworkManager : NetworkManager
    {
        [SerializeField] 
        private GameObject unitBasePrefab;
        [SerializeField]
        private GameOverHandler gameOverHandlerPrefab;

        public static event Action ClientOnConnected;
        public static event Action ClientOnDisconnected;
        public List<RTSPlayer> Players {get;} = new List<RTSPlayer>();

        private bool isGameInProgress = false;

        #region "Server"
        
        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            base.OnServerAddPlayer(conn);

            RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
            Players.Add(player);
            player.SetTeamColor( new Color(
                UnityEngine.Random.value,
                UnityEngine.Random.value,
                UnityEngine.Random.value
            ));

            player.SetPartyOwner(Players.Count == 1);
            player.SetDisplayName($"Player {Players.Count}");
        }

        public void StartGame()
        {
            if(Players.Count < 2)
            {
                return;
            }

            isGameInProgress = true;

            ServerChangeScene("Scene_Map_01");
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
            Players.Remove(player);

            base.OnServerDisconnect(conn);
        }

        public override void OnStopServer()
        {
            Players.Clear();

            isGameInProgress = false;
        }

        public override void OnServerSceneChanged(string newSceneName)
        {
            if(SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
            {
                GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
                NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

                foreach(RTSPlayer player in Players)
                {
                    GameObject baseInstance = Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);
                    NetworkServer.Spawn(baseInstance, player.connectionToClient);
                }
            }
            else{
                Debug.Log("Erro ao instanciar GameOverHandler");
            }
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            if(!isGameInProgress){return;}
            conn.Disconnect();
        }

        #endregion

        #region "Client"

        public override void OnClientConnect()
        {
            base.OnClientConnect();

            ClientOnConnected?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();

            ClientOnDisconnected?.Invoke();
        }

        public override void OnStopClient()
        {
            Players.Clear();
        }

        #endregion
    }
}