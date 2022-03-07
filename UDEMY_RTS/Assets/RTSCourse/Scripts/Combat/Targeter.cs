using System.Collections;
using System.Collections.Generic;
using Mirror;
using RTSCourse.Controllers;
using UnityEngine;

namespace RTSCourse.Combat
{
    public class Targeter : NetworkBehaviour
    {
        private Targetable target;

        public override void OnStartServer()
        {
            GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
        }

        public override void OnStopServer()
        {
            GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
        }

        public Targetable GetTarget()
        {
            return target;
        }


        #region "Server"
        [Command]
        public void CmdSetTarget(GameObject targetGameObject)
        {
            if(!targetGameObject.TryGetComponent<Targetable>(out  Targetable target)) { return; }

            this.target = target;
        }

        [Server]
        public void ClearTarget()
        {
            this.target = null;
        }

        [Server]
        private void ServerHandleGameOver()
        {
            ClearTarget();
        }

        #endregion

        #region "Client"

        #endregion
    }
}