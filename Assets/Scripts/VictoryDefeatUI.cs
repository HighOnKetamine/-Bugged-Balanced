using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles Victory/Defeat UI display for the local player.
/// Listens to GameStateManager events and shows personalized screens based on player's team.
/// </summary>
public class VictoryDefeatUI : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private TextMeshProUGUI defeatText;

    [Header("Optional: Audio")]
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;
    private AudioSource audioSource;

    private TeamComponent localPlayerTeam;

    private void Awake()
    {
        // Hide panels by default
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        audioSource = GetComponent<AudioSource>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) return;

        // Subscribe to game state events
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnVictory += HandleVictory;
        }
        else
        {
            Debug.LogWarning("[VictoryDefeatUI] GameStateManager.Instance is null!");
        }

        // Find the local player's team component
        localPlayerTeam = GetComponent<TeamComponent>();
        if (localPlayerTeam == null)
        {
            Debug.LogWarning("[VictoryDefeatUI] No TeamComponent found on local player!");
        }
    }

    private void HandleVictory(TeamId winningTeam)
    {
        if (localPlayerTeam == null)
        {
            Debug.LogWarning("[VictoryDefeatUI] Cannot determine local player team!");
            return;
        }

        bool isVictory = (localPlayerTeam.Team == winningTeam);

        if (isVictory)
        {
            ShowVictory(winningTeam);
        }
        else
        {
            ShowDefeat(winningTeam);
        }
    }

    private void ShowVictory(TeamId winningTeam)
    {
        Debug.Log($"[VictoryDefeatUI] VICTORY! {winningTeam} team won!");

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (victoryText != null)
            {
                victoryText.text = $"{winningTeam} Team Victory!";
            }
        }

        if (audioSource != null && victorySound != null)
        {
            audioSource.PlayOneShot(victorySound);
        }
    }

    private void ShowDefeat(TeamId winningTeam)
    {
        Debug.Log($"[VictoryDefeatUI] DEFEAT! {winningTeam} team won...");

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
            if (defeatText != null)
            {
                defeatText.text = $"{winningTeam} Team Victory\nDefeat";
            }
        }

        if (audioSource != null && defeatSound != null)
        {
            audioSource.PlayOneShot(defeatSound);
        }
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnVictory -= HandleVictory;
        }
    }
}
