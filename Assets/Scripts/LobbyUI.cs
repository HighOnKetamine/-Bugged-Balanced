using System.Text;
using FishNet;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TextMeshProUGUI playerListText;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startButton;      // host only
    [SerializeField] private Button[] characterButtons; // one per character slot (max 3)

    private bool _isReady;

    private void Start()
    {
        lobbyPanel.SetActive(true);

        readyButton.onClick.AddListener(OnReadyClicked);
        startButton.onClick.AddListener(OnStartClicked);

        // Only the host can see the Start button
        startButton.gameObject.SetActive(InstanceFinder.IsServerStarted);

        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i; // capture for lambda
            characterButtons[i].onClick.AddListener(() => OnCharacterSelected(index));
        }

        NetworkGameManager.Instance.Players.OnChange += OnPlayersChanged;
        NetworkGameManager.OnGameStarted += OnGameStarted;

        RefreshPlayerList();
        RefreshCharacterButtons();
    }

    private void OnDestroy()
    {
        if (NetworkGameManager.Instance != null)
            NetworkGameManager.Instance.Players.OnChange -= OnPlayersChanged;
        NetworkGameManager.OnGameStarted -= OnGameStarted;
    }

    private void OnReadyClicked()
    {
        _isReady = !_isReady;
        NetworkGameManager.Instance.ServerSetReady(_isReady);
        readyButton.GetComponentInChildren<TextMeshProUGUI>().text = _isReady ? "Unready" : "Ready";
    }

    private void OnCharacterSelected(int index)
    {
        NetworkGameManager.Instance.ServerSetCharacter(index);
    }

    private void OnStartClicked()
    {
        NetworkGameManager.Instance.ServerRequestStartGame();
    }

    private void OnGameStarted()
    {
        lobbyPanel.SetActive(false);
    }

    private void OnPlayersChanged(SyncListOperation op, int index,
        LobbyPlayerData prev, LobbyPlayerData next, bool asServer)
    {
        RefreshPlayerList();
    }

    private void RefreshPlayerList()
    {
        if (NetworkGameManager.Instance == null) return;

        StringBuilder sb = new StringBuilder();
        foreach (var p in NetworkGameManager.Instance.Players)
        {
            string team = p.TeamId == 0 ? "Blue" : "Red";
            string ready = p.IsReady ? "[READY]" : "[NOT READY]";
            string charName = GetCharName(p.CharacterIndex);
            sb.AppendLine($"{p.PlayerName}  |  {team}  |  {charName}  |  {ready}");
        }
        playerListText.text = sb.ToString();
    }

    private void RefreshCharacterButtons()
    {
        var chars = NetworkGameManager.Instance?.characters;
        for (int i = 0; i < characterButtons.Length; i++)
        {
            bool exists = chars != null && i < chars.Length;
            characterButtons[i].gameObject.SetActive(exists);
            if (exists)
                characterButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = chars[i].characterName;
        }
    }

    private string GetCharName(int index)
    {
        var chars = NetworkGameManager.Instance?.characters;
        if (chars == null || index < 0 || index >= chars.Length) return "?";
        return chars[index].characterName;
    }
}