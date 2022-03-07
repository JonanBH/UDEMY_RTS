using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using RTSCourse.Buildings;

namespace RTSCourse.Combat
{
    public class Health : NetworkBehaviour
    {
        [SerializeField]
        private int maxHealth = 100;

        [SyncVar(hook = nameof(HandleOnHealthUpdated))]
        private int currentHealth;

        public event Action ServerOnDie;
        public event Action<int, int> ClientOnHealthUpdated;
        #region "Server"

        public override void OnStartServer()
        {
            currentHealth = maxHealth;
            UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
        }

        public override void OnStopServer()
        {
            UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
        }

        [Server]
        public void DealDamage(int damageAmount)
        {
            if(currentHealth == 0) { return; }

            currentHealth = Mathf.Max(currentHealth - damageAmount, 0);
            if(currentHealth != 0) { return; }

            ServerOnDie?.Invoke();
            Debug.Log("We Died");
        }

        #endregion

        #region "Client"

        private void HandleOnHealthUpdated(int oldHealth, int newHealth)
        {
            ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
        }

        [Server]
        private void ServerHandlePlayerDie(int playerId)
        {
            if(connectionToClient.connectionId != playerId) { return; }
            DealDamage(currentHealth);
        }

        #endregion
    }
}