using System.Collections;
using System.Collections.Generic;
using Mirror;
using RTSCourse.Combat;
using RTSCourse.Controllers;
using RTSCourse.Networking;
using UnityEngine;

namespace RTSCourse.Buildings
{
    public class ResourceGenerator : NetworkBehaviour
    {
        [SerializeField]
        private Health health;

        [SerializeField]
        private int resourcesPerInterval = 10;
        [SerializeField]
        private float interval = 2f;

        private float timer;
        private RTSPlayer player;

        public override void OnStartServer()
        {
            timer = interval;
            player = connectionToClient.identity.GetComponent<RTSPlayer>();

            health.ServerOnDie += ServerHandleDie;
            GameOverHandler.ServerOnGameOver += ServerHandleOnGameOver;
        }

        [ServerCallback]
        private void Update() 
        {
            timer -= Time.deltaTime;

            if(timer <= 0)
            {
                timer += interval;
                player.AddResources(resourcesPerInterval);
            }    
        }

        public override void OnStopServer()
        {
            health.ServerOnDie -= ServerHandleDie;
            GameOverHandler.ServerOnGameOver -= ServerHandleOnGameOver;
        }

        private void ServerHandleDie()
        {
            NetworkServer.Destroy(gameObject);
        }

        private void ServerHandleOnGameOver()
        {
            enabled = false;
        }
    }
}