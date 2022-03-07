using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using RTSCourse.Combat;
using RTSCourse.Networking;

namespace RTSCourse.Units
{
    public class Unit : NetworkBehaviour
    {
        #region "Fields"

        [SerializeField] 
        private UnityEvent onSelected = null;
        [SerializeField] 
        private UnityEvent onDeselected = null;
        [SerializeField] 
        private UnitMovement unitMovement = null;
        [SerializeField]
        private Targeter targeter;
        [SerializeField]
        private Health health;
        [SerializeField]
        private int resourcesCost;
        public static event Action<Unit> ServerOnUnitSpawned;
        public static event Action<Unit> ServerOnUnitDespawned;
        public static event Action<Unit> AuthorityOnUnitSpawned;
        public static event Action<Unit> AuthorityOnUnitDespawned;

        
        #region "Public Methods"
            public UnitMovement GetUnitMovement()
            {
                return unitMovement;
            }

            public Targeter GetTargeter()
            {
                return targeter;
            }

            public int GetResourceCost()
            {
                return resourcesCost;
            }
        #endregion
        #region "Client"

        [Client]
        public void Select(){
            if(!hasAuthority) return;

            onSelected?.Invoke();
        }

        [Client]
        public void Deselect(){
            if(!hasAuthority) return;

            onDeselected?.Invoke();
        }

        public override void OnStartAuthority()
        {
            if(!hasAuthority) return;

            AuthorityOnUnitSpawned?.Invoke(this);
        }

        public override void OnStopClient()
        {
            if(!hasAuthority) return;

            AuthorityOnUnitDespawned?.Invoke(this);
        }

        #endregion

        #endregion

        #region "Server"

        public override void OnStartServer()
        {
            ServerOnUnitSpawned?.Invoke(this);    
            health.ServerOnDie += ServerHandleDie;        
        }

        public override void OnStopServer()
        {
            ServerOnUnitDespawned?.Invoke(this);   
            health.ServerOnDie -= ServerHandleDie;         
        }

        [Server]
        private void ServerHandleDie()
        {
            NetworkServer.Destroy(gameObject);
        }

        #endregion
    }
}