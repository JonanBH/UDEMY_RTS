using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RTSCourse.Networking;
using Mirror;

namespace RTSCourse.UI
    {
    public class ResourcesDisplay : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text resourcesText = null;

        private RTSPlayer player;

        private void Start() {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            
            ClientHandleResourcesUpdated(player.GetResources());

            player.ClientOnResourcesChanged += ClientHandleResourcesUpdated;
        }

        private void OnDestroy() 
        {
            player.ClientOnResourcesChanged -= ClientHandleResourcesUpdated;    
        }

        private void ClientHandleResourcesUpdated(int resources)
        {
            resourcesText.text = $"Resources: {resources}";
        }
    }
}