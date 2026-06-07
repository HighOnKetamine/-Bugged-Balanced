using UnityEngine;

public class LobbyCamera : MonoBehaviour
{
    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        NetworkGameManager.OnGameStarted += DisableCamera;
    }

    private void OnDisable()
    {
        NetworkGameManager.OnGameStarted -= DisableCamera;
    }

    private void DisableCamera()
    {
        _cam.enabled = false;
        Debug.Log("[LobbyCamera] Disabled.");
    }
}