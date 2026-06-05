using FishNet;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button returnButton;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        returnButton.onClick.AddListener(OnReturnClicked);
        NetworkGameManager.OnGameOver += OnGameOver;
    }

    private void OnDestroy()
    {
        NetworkGameManager.OnGameOver -= OnGameOver;
    }

    private void OnGameOver(sbyte winningTeam)
    {
        gameOverPanel.SetActive(true);
        string team = winningTeam == 0 ? "Blue" : "Red";
        resultText.text = $"{team} Team Wins!";
    }

    private void OnReturnClicked()
    {
        // Stop server (kicks all clients) then stop local client, then load menu
        if (InstanceFinder.IsServerStarted)
            InstanceFinder.NetworkManager.ServerManager.StopConnection(true);
        else
            InstanceFinder.NetworkManager.ClientManager.StopConnection();

        SceneManager.LoadScene("MainMenu");
    }
}