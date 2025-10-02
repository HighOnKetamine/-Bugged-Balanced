using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    uint speed;

    uint defaultSpeed = 10;

    [SerializeField]
    Logger logger;
    Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;


        if (logger == null)
        {
            Debug.LogWarning("Failed to load error handler!");
            logger = FindFirstObjectByType<Logger>();
            if (logger == null)
            {
                Debug.LogError("[Fatal error] ErrorHandler not found in the scene!");
                Application.Quit();
                return;
            }
        }

        if (mainCamera == null)
        {
            logger.Fatal("Failed to load camera!");
        }
        if (speed <= 0)
        {
            logger.Warn($"Speed was not set or was invalid. Using default {defaultSpeed}");
            speed = defaultSpeed;
        }
    }

    void Update()
    {

    }


}
