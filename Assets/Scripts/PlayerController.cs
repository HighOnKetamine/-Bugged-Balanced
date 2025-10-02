using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Failed to locate the main camera!");
            Application.Quit();
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    
}
