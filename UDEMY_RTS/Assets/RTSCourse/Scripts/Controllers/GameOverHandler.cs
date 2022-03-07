using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RTSCourse.Buildings;
using System;
using RTSCourse.Networking;

namespace RTSCourse.Controllers
{
    public class GameOverHandler : NetworkBehaviour
    {

        private List<UnitBase> unitBases = new List<UnitBase>();
        public static event Action<string> ClientOnGameOver;
        public static event Action ServerOnGameOver;
        #region "Server"

        public override void OnStartServer()
        {
            UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
            UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
        }

        public override void OnStopServer()
        {
            UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
            UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
        }

        private void ServerHandleBaseSpawned(UnitBase unitBase)
        {
            unitBases.Add(unitBase);
        }

        private void ServerHandleBaseDespawned(UnitBase unitBase)
        {
            unitBases.Remove(unitBase);
            if(unitBases.Count != 1) { return; }
            int playerId = unitBases[0].connectionToClient.connectionId;


            RpcGameOver($"Player {playerId}");

            ServerOnGameOver?.Invoke();

            Debug.Log("Game Over");
        }

        #endregion

        #region "Client"

        [ClientRpc]
        private void RpcGameOver(string winner)
        {
            ClientOnGameOver?.Invoke(winner);
        }

        #endregion
    }
}