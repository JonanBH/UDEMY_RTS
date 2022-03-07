using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTSCourse.Networking;
using System;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace RTSCourse.UI{
    public class LobbyMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject lobbyUI = null;
        [SerializeField]
        private Button startGameButton;
        [SerializeField]
        private TMP_Text[] playerNameTexts = null;

        private void Start() {
            RTSNetworkManager.ClientOnConnected += HandleClientConnected;
            RTSNetworkManager.ClientOnDisconnected += HandleClientDisconnected;
            RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdate;
            RTSPlayer.ClientOnInfoUpdated += ClientHandleOnInfoUpdated;
        }

        private void OnDestroy() {
            RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
            RTSNetworkManager.ClientOnDisconnected -= HandleClientDisconnected;
            RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdate;
            RTSPlayer.ClientOnInfoUpdated -= ClientHandleOnInfoUpdated;
        }

        private void ClientHandleOnInfoUpdated()
        {
            List<RTSPlayer> players = ((RTSNetworkManager)(NetworkManager.singleton)).Players;

            for(int i = 0; i < players.Count; i++)
            {
                playerNameTexts[i].text = players[i].GetDisplayName();
            }

            for(int i= players.Count; i < playerNameTexts.Length; i++)
            {
                playerNameTexts[i].text = "Waiting for player...";
            }

            startGameButton.interactable = players.Count >= 2;
        }


        private void HandleClientConnected()
        {
            lobbyUI.SetActive(true);
        }

        private void HandleClientDisconnected()
        {
            lobbyUI.SetActive(false);
        }

        public void LeaveLobby()
        {
            if(NetworkServer.active && NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopHost();
            }
            else
            {
                NetworkManager.singleton.StopClient();

                SceneManager.LoadScene(0);
            }
        }

        public void StartGame()
        {
            NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();
        }

        private void AuthorityHandlePartyOwnerStateUpdate(bool state)
        {
            startGameButton.gameObject.SetActive(state);
        }
    }
}
