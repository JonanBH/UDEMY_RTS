using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RTSCourse.Networking;
public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject landingPagePanel = null;
    [SerializeField]
    private TMP_InputField addressInput;
    [SerializeField]
    private Button joinButton;
    [SerializeField]
    private Button hostButton;
    
    public void HostLobby()
    {
        landingPagePanel.SetActive(false);
        NetworkManager.singleton.StartHost();

        joinButton.interactable = false;
        hostButton.interactable = false;
        addressInput.interactable = false;
    }

    private void OnEnable() {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSNetworkManager.ClientOnDisconnected += HandleClientDisconnected;
    }

    private void OnDisable() {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSNetworkManager.ClientOnDisconnected -= HandleClientDisconnected;
    }

    public void Join()
    {
        string address = addressInput.text;

        NetworkManager.singleton.networkAddress = address;
        NetworkManager.singleton.StartClient();
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;
        hostButton.interactable = true;
        addressInput.interactable = true;
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
        hostButton.interactable = true;
        addressInput.interactable = true;
    }
}
