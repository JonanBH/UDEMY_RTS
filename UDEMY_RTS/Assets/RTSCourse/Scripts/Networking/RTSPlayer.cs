using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RTSCourse.Units;
using RTSCourse.Buildings;
using System;

namespace RTSCourse.Networking
{
    public class RTSPlayer : NetworkBehaviour
    {
        [SerializeField]
        private Transform cameraTransform = null;
        [SerializeField] 
        private List<Unit> myUnits = new List<Unit>();
        [SerializeField] 
        private List<Building> myBuildings = new List<Building>();
        [SerializeField]
        private Building[] buildings = new Building[0];
        [SerializeField]
        private float buindingRangeLimit = 5f;
        [SerializeField]
        private LayerMask buildingBlockLayer = new LayerMask();
        [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
        private int resources = 1000;
        [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
        private string displayName;
        public event Action<int> ClientOnResourcesChanged;
        public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;
        public static event Action ClientOnInfoUpdated;
        private Color teamColor = new Color();
        [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStarteUpdated))]
        private bool isPartyOwner = false;

        public List<Unit> GetMyUnits()
        {
            return myUnits;
        }

        public List<Building> GetMyBuildings()
        {
            return myBuildings;
        }

        public bool GetIsPartyOwner()
        {
            return isPartyOwner;
        }

        public int GetResources()
        {
            return resources;
        }

        public Color GetTeamColor()
        {
            return teamColor;
        }

        public Transform GetCameraTransform()
        {
            return cameraTransform;
        }

        public string GetDisplayName()
        {
            return displayName;
        }

        public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 position)
        {
            if(Physics.CheckBox(position + buildingCollider.center, buildingCollider.size / 2, Quaternion.identity, buildingBlockLayer))
            {
                return false;
            }

            foreach(Building building in myBuildings)
            {
                if((position- building.transform.position).sqrMagnitude <= buindingRangeLimit * buindingRangeLimit)
                {
                    return true;
                }
            }
            return false;
        }

        #region "Server"

        public override void OnStartServer()
        {
            Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
            Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
            Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
            Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;

            DontDestroyOnLoad(gameObject);
        }

        public override void OnStopServer()
        {
            Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
            Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
            Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
            Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
        }

        [Server]
        public void AddResources(int resources)
        {
            this.resources += resources;
        }

        [Server]
        public void RemoveResources(int resources)
        {
            this.resources -= resources;
        }

        [Server]
        public void SetTeamColor(Color newTeamColor)
        {
            teamColor = newTeamColor;
        }

        [Server]
        public void SetDisplayName(string displayName)
        {
            this.displayName = displayName;
        }

        [Command]
        public void CmdSetDisplayName(string displayName)
        {
            SetDisplayName(displayName);
        }

        [Command]
        public void CmdTryPlaceBuilding(int buildingId, Vector3 position)
        {
            Building buildingToPlace = null;
            foreach(Building building in buildings)
            {
                if(building.GetId() == buildingId)
                {
                    buildingToPlace = building;
                    break;
                }
            }

            if(buildingToPlace == null) { return; }
            if(resources < buildingToPlace.GetPrice()) { return; }

            BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

            if(!CanPlaceBuilding(buildingCollider, position)){ return; }

            RemoveResources(buildingToPlace.GetPrice());

            GameObject buildingPlaced = Instantiate(buildingToPlace.gameObject, position, buildingToPlace.transform.rotation);
            NetworkServer.Spawn(buildingPlaced, connectionToClient);
        }

        private void ServerHandleUnitSpawned(Unit unit)
        {
            if(unit.connectionToClient.connectionId != connectionToClient.connectionId) {return;}

            myUnits.Add(unit);
        }

        [Command]
        public void CmdStartGame()
        {
            if(!isPartyOwner) { return;}

            ((RTSNetworkManager)NetworkManager.singleton).StartGame();

        }

        private void ServerHandleUnitDespawned(Unit unit)
        {
            if(unit.connectionToClient.connectionId != connectionToClient.connectionId) {return;}

            myUnits.Remove(unit);
        }

        private void ServerHandleBuildingSpawned(Building build)
        {
            if(build.connectionToClient.connectionId != connectionToClient.connectionId) {return;}

            myBuildings.Add(build);
        }

        private void ServerHandleBuildingDespawned(Building build)
        {
            if(build.connectionToClient.connectionId != connectionToClient.connectionId) {return;}

            myBuildings.Remove(build);
        }

        [Server]
        public void SetPartyOwner(bool state)
        {
            isPartyOwner = state;
        }

        #endregion
   
        #region "Client"

        public override void OnStartAuthority(){
            if(NetworkServer.active) return;

            Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
            Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
            Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
            Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
        }

        private void AuthorityHandleUnitDespawned(Unit unit)
        {

            myUnits.Add(unit);
        }

        private void AuthorityHandleUnitSpawned(Unit unit)
        {

            myUnits.Remove(unit);
        }

        public override void OnStartClient()
        {
            if(NetworkServer.active){ return; }

            DontDestroyOnLoad(gameObject);

            ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
        }

        public override void OnStopClient()
        {
            ClientOnInfoUpdated?.Invoke();
            if(!isClientOnly) return;
            
            ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);

            if(!hasAuthority) {return;}

            Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
            Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
            
            Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
            Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
        }

        [Client]
        private void AuthorityHandleBuildingSpawned(Building build)
        {
            myBuildings.Add(build);
        }

        [Client]
        private void AuthorityHandleBuildingDespawned(Building build)
        {
            myBuildings.Remove(build);
        }

        private void AuthorityHandlePartyOwnerStarteUpdated(bool oldState, bool newState)
        {
            if(!hasAuthority){return;}

            AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
        }

        [Client]
        private void ClientHandleResourcesUpdated(int oldResources, int newResources)
        {
            ClientOnResourcesChanged?.Invoke(newResources);
        }

        [Client]
        private void ClientHandleDisplayNameUpdated(string oldName, string newName)
        {
            ClientOnInfoUpdated?.Invoke();  
        }

        #endregion
    }

}

