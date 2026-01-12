using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI healthText; // Optional
    [SerializeField] private Canvas canvas;
    
    [Header("UI Settings")]
    [SerializeField] private bool useFixedRotation = false;
    [SerializeField] private Vector3 fixedRotation = new Vector3(45, 0, 0);

    private Camera mainCamera;

    private void Start()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChangedEvent += UpdateHealthBar;
            UpdateHealthBar(healthSystem.CurrentHealth, healthSystem.MaxHealth);
        }

        mainCamera = Camera.main;

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }
    }

    private void LateUpdate()
    {
        if (canvas == null) return;

        if (useFixedRotation)
        {
            canvas.transform.rotation = Quaternion.Euler(fixedRotation);
        }
        else if (mainCamera != null)
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