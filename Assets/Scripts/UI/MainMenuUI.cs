using FishNet;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField ipInput;   // leave blank for localhost
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
        InstanceFinder.ServerManager.OnServerConnectionState += OnServerState;
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientState;
    }

    private void OnDestroy()
    {
        if (InstanceFinder.NetworkManager == null) return;
        InstanceFinder.ServerManager.OnServerConnectionState -= OnServerState;
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientState;
    }

    private void OnHostClicked()
    {
        string name = nameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) { statusText.text = "Enter a name."; return; }

        LocalPlayerInfo.Name = name;
        SetButtonsInteractable(false);
        statusText.text = "Starting server...";

        InstanceFinder.NetworkManager.ServerManager.StartConnection();
        InstanceFinder.NetworkManager.ClientManager.StartConnection();
    }

    private void OnJoinClicked()
    {
        string name = nameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) { statusText.text = "Enter a name."; return; }

        string ip = ipInput.text.Trim();
        if (string.IsNullOrEmpty(ip)) ip = "localhost";

        LocalPlayerInfo.Name = name;
        SetButtonsInteractable(false);
        statusText.text = $"Connecting to {ip}...";

        InstanceFinder.NetworkManager.ClientManager.StartConnection(ip);
    }

    private void OnServerState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState != LocalConnectionState.Started) return;

        statusText.text = "Loading game...";

        // Load "Main" for every current and future connection, replace MainMenu scene.
        SceneLoadData sld = new SceneLoadData("Main");
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.NetworkManager.SceneManager.LoadGlobalScenes(sld);
    }

    private void OnClientState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            statusText.text = "Failed to connect.";
            SetButtonsInteractable(true);
        }
    }

    private void SetButtonsInteractable(bool value)
    {
        hostButton.interactable = value;
        joinButton.interactable = value;
    }
}