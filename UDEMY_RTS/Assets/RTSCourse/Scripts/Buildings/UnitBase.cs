using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RTSCourse.Combat;
using System;

namespace RTSCourse.Buildings
{
    public class UnitBase : NetworkBehaviour
    {
        [SerializeField]
        private Health health;

        public static event Action<UnitBase> ServerOnBaseSpawned;
        public static event Action<UnitBase> ServerOnBaseDespawned;
        public static event Action<int> ServerOnPlayerDie;

        #region "Server"

        public override void OnStartServer()
        {
            health.ServerOnDie += ServerHandleDie;

            ServerOnBaseSpawned?.Invoke(this);
        }

        public override void OnStopServer()
        {
            health.ServerOnDie -= ServerHandleDie;

            ServerOnBaseDespawned?.Invoke(this);
        }

        [Server]
        private void ServerHandleDie()
        {
            Debug.Log("Game OVer");
            ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);
            NetworkServer.Destroy(gameObject);
        }
        #endregion

        #region "Client"

        #endregion
    }
}
