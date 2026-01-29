using System;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    public string PlayerId;
    public int MMR; // Matchmaking Rating
    public int Wins;
    public int Losses;
    public float WinRate => Losses == 0 ? 1f : (float)Wins / (Wins + Losses);

    // Constructor
    public PlayerProfile(string playerId, int initialMMR = 1000)
    {
        PlayerId = playerId;
        MMR = initialMMR;
        Wins = 0;
        Losses = 0;
    }

    // Update MMR after a match
    public void UpdateMMR(bool won, int opponentMMR, float kFactor = 32f)
    {
        // Elo rating system
        float expectedScore = 1f / (1f + Mathf.Pow(10f, (opponentMMR - MMR) / 400f));
        float actualScore = won ? 1f : 0f;
        int mmrChange = Mathf.RoundToInt(kFactor * (actualScore - expectedScore));

        MMR += mmrChange;

        if (won) Wins++;
        else Losses++;
    }
}