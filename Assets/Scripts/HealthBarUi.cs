using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Image fillImage;
    [SerializeField] private Text healthText; // Optional
    [SerializeField] private Canvas canvas;

    private Camera mainCamera;

    private void Start()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChangedEvent += UpdateHealthBar;
            UpdateHealthBar(healthSystem.CurrentHealth, healthSystem.MaxHealth);
        }

        mainCamera = Camera.main;

        // Make sure canvas faces camera
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }
    }

    private void LateUpdate()
    {
        // Make health bar face camera
        if (mainCamera != null && canvas != null)
        {
            canvas.transform.LookAt(canvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChangedEvent -= UpdateHealthBar;
        }
    }
}