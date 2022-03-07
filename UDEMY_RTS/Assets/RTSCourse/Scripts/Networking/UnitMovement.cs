using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using RTSCourse.Combat;
using RTSCourse.Controllers;
namespace RTSCourse.Networking
{
    public class UnitMovement : NetworkBehaviour
    {
        [SerializeField] 
        private NavMeshAgent agent = null;
        [SerializeField] 
        private float navMeshOffsetLimit = 1;
        [SerializeField]
        private Targeter targeter;

        [SerializeField]
        private float chaseRange = 10f;
        

        #region "Lifecicle"

        #endregion

        Camera mainCamera;
        
        #region "Server"

        [ServerCallback]
        private void Update() 
        {
            if(targeter.GetTarget() != null)
            {
                Targetable target = targeter.GetTarget();
                
                if((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
                {
                    agent.SetDestination(target.transform.position);
                }
                else if(agent.hasPath)
                {
                    agent.ResetPath();
                }
                return;
            }

            if(!agent.hasPath) { return; }

            if(agent.remainingDistance > agent.stoppingDistance) { return; }

            agent.ResetPath();    
        }

        public override void OnStartServer()
        {
            GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
        }

        public override void OnStopServer()
        {
            GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
        }

        [Server]
        private void ServerHandleGameOver()
        {
            agent.ResetPath();
        }

        [Server]
        public void ServerMove(Vector3 position)
        {
            targeter.ClearTarget();

            if(!NavMesh.SamplePosition(position, out NavMeshHit hit, navMeshOffsetLimit, NavMesh.AllAreas)) return;

            agent.SetDestination(hit.position);
        }

        /// <summary>
        /// Request the server to move this unit to the target position
        /// </summary>
        /// <param name="position"></param>
        [Command]
        public void CmdMove(Vector3 position)
        {
            ServerMove(position);
        }

        #endregion

        /*#region "Client"

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            mainCamera = Camera.main;
        }

        [ClientCallback]
        private void Update() {
            if(!hasAuthority) return;

            if(!Mouse.current.rightButton.wasPressedThisFrame) return;

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) return;

            CmdMove(hit.point);
        }

        #endregion*/
    }
}