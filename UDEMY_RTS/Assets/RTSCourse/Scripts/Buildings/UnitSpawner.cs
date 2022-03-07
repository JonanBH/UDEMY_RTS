using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using RTSCourse.Combat;
using RTSCourse.Units;
using TMPro;
using UnityEngine.UI;
using RTSCourse.Networking;

namespace RTSCourse.Buildings
{
    public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
    {
        [SerializeField] 
        private Unit unitPrefab;
        [SerializeField] 
        private Transform unitSpawnPoint;
        [SerializeField]
        private Health health;
        [SerializeField]
        private TMP_Text remainingUnitsText = null;
        [SerializeField]
        private Image unitProgressImage = null;
        [SerializeField]
        private int maxUnitQueue = 5;
        [SerializeField]
        private float spawnMoveRange = 7f;
        [SerializeField]
        private float unitSpawnDuration = 5f;

        [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
        private int queuedUnits;
        [SyncVar]
        private float unitTimer;

        private float progressImageVelocity;

        private void Update() 
        {
            if(isServer)
            {
                ProduceUnits();
            }   

            if(isClient)
            {
                UpdateTimerDisplay();
            } 
        }

        #region "Server"

        public override void OnStartServer()
        {
            health.ServerOnDie += ServerHandleDie;
        }

        public override void OnStopServer()
        {
            health.ServerOnDie -= ServerHandleDie;
        }

        [Server]
        private void ServerHandleDie()
        {
            NetworkServer.Destroy(gameObject);
        }

        [Server]
        private void ProduceUnits()
        {
            if(queuedUnits == 0) { return; }

            unitTimer += Time.deltaTime;

            if(unitTimer < unitSpawnDuration) {return;}

            GameObject unitInstance = Instantiate(unitPrefab.gameObject, 
                                                    unitSpawnPoint.position, 
                                                    unitSpawnPoint.rotation);

            NetworkServer.Spawn(unitInstance, connectionToClient);

            Vector3 spawnOFfset = Random.insideUnitSphere * spawnMoveRange;
            spawnOFfset.y = unitSpawnPoint.position.y;

            UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
            unitMovement.ServerMove(unitSpawnPoint.position + spawnOFfset);

            queuedUnits--;
            unitTimer = 0;
        }

        [Command]
        private void CmdSpawnUnit()
        {
            if(queuedUnits == maxUnitQueue) { return; }

            RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
            int resources = player.GetResources();

            if(resources < unitPrefab.GetResourceCost()) { return; }

            player.RemoveResources(unitPrefab.GetResourceCost());
            queuedUnits++;
        }

        #endregion

        #region "Client"

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button != PointerEventData.InputButton.Left) { return;}
            if(!hasAuthority) { return; }

            CmdSpawnUnit();
        }

        private void ClientHandleQueuedUnitsUpdated(int oldValue, int newValue)
        {
            remainingUnitsText.text = newValue.ToString();
        }

        private void UpdateTimerDisplay()
        {
            float newProgress = unitTimer / unitSpawnDuration;

            if(newProgress < unitProgressImage.fillAmount)
            {
                unitProgressImage.fillAmount = newProgress;
            }
            else{
                unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount, 
                                                                newProgress,
                                                                ref progressImageVelocity,
                                                                0.1f);
            }
        }

        #endregion
    }
}