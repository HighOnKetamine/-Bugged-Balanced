using UnityEngine;

public class LobbyCameraController : MonoBehaviour
{
    private void OnEnable()
    {
        NetworkGameManager.OnGameStarted += OnGameStarted;
    }

    private void OnDisable()
    {
        NetworkGameManager.OnGameStarted -= OnGameStarted;
    }

    private void OnGameStarted()
    {
        gameObject.SetActive(false);
    }
}
