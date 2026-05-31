using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Client-side UI that displays kill announcements.
/// Attach to a Canvas GameObject with a TextMeshProUGUI child.
///
/// WHY a coroutine for hide?
/// We want the banner to appear instantly but fade/disappear after N seconds
/// without blocking any other logic. Coroutines are Unity's idiomatic tool
/// for this kind of "show then hide after delay" pattern.
/// </summary>
public class KillFeedUI : MonoBehaviour
{
    public static KillFeedUI Instance { get; private set; }

    [SerializeField] private GameObject panel;       // root panel to show/hide
    [SerializeField] private TMP_Text killText;
    [SerializeField] private float displayTime = 3f;

    private Coroutine _hideCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowKill(string killerName, string victimName)
    {
        killText.text = $"<b>{killerName}</b>  killed  <b>{victimName}</b>";
        panel.SetActive(true);

        // Cancel any previous hide coroutine so rapid kills don't interfere
        if (_hideCoroutine != null)
            StopCoroutine(_hideCoroutine);

        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        panel.SetActive(false);
    }
}