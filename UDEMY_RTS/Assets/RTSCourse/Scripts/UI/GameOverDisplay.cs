using System.Collections;
using System.Collections.Generic;
using Mirror;
using RTSCourse.Controllers;
using TMPro;
using UnityEngine;

namespace RTSCourse.UI
{
    public class GameOverDisplay : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text winnerNameText = null;
        [SerializeField]
        private GameObject gameOverDisplayParent = null;
        private void Start() 
        {
            GameOverHandler.ClientOnGameOver += ClientHandleGameOver;     
        }

        private void OnDestroy() 
        {
            GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;     
        }

        public void LeaveGame()
        {
            if(NetworkServer.active && NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopHost();
            }
            else
            {
                NetworkManager.singleton.StopClient();
            }
        }

        private void ClientHandleGameOver(string winner)
        {
            winnerNameText.text = $"{winner} Has Won!";
            gameOverDisplayParent.SetActive(true);
        }
    }
}