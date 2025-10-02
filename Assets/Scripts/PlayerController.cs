using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    uint speed;

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

        logger.Fatal("TEST");
    }

    void Update()
    {

    }


}
