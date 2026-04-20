using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [SerializeField] private GameObject _popupPrefab;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Show(float damage, DamageType damageType, Vector3 worldPosition)
    {
        var go = Instantiate(_popupPrefab, worldPosition + Vector3.up * 2f, Quaternion.identity);
        go.GetComponent<DamagePopup>().Setup(damage, damageType);
    }
}