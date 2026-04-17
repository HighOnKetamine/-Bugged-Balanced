using UnityEngine;
using System.Collections;
using TMPro; // Ensure you have TextMeshPro imported

public class HighAltitudeResultFlip : MonoBehaviour
{
    [Header("Coin Prefab")]
    public GameObject blankCoinPrefab;

    [Header("Scene Elements")]
    public GameObject testSceneRoot;

    [Header("Flip Settings")]
    public float duration = 3.0f;
    public int totalFullFlips = 5;

    private Vector3 highSpawnPos;
    private Camera spawnedFlipCam;
    private GameObject activeCoin;
    private bool isRunning = false;

    void Update()
    {
        if (isRunning) return;
        if (Input.GetKeyDown(KeyCode.M)) StartCoroutine(FlipSequence());
    }

    IEnumerator FlipSequence()
    {
        isRunning = true;

        // 1. Calculate isolated position (+50 altitude)
        highSpawnPos = transform.position + new Vector3(0, 50, 0);

        if (testSceneRoot != null) testSceneRoot.SetActive(false);

        // 2. Auto-Spawn Camera
        GameObject camObj = new GameObject("FlipCamera_Isolated");
        spawnedFlipCam = camObj.AddComponent<Camera>();
        spawnedFlipCam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        spawnedFlipCam.clearFlags = CameraClearFlags.SolidColor;

        spawnedFlipCam.transform.position = highSpawnPos + Vector3.up * 5f;
        spawnedFlipCam.transform.rotation = Quaternion.Euler(90, 0, 0);

        // 3. Spawn Coin
        activeCoin = Instantiate(blankCoinPrefab, highSpawnPos, Quaternion.identity);

        // 4. Smooth Slerp to 90 degrees Z
        float totalZDegrees = (360f * totalFullFlips) + 90f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            float currentZ = Mathf.Lerp(0, totalZDegrees, easedT);

            if (activeCoin != null)
                activeCoin.transform.rotation = Quaternion.Euler(0, 0, currentZ);

            elapsed += Time.deltaTime;
            yield return null;
        }

        activeCoin.transform.rotation = Quaternion.Euler(0, 0, 90f);

        // 5. RNG Result & Text Spawn
        bool won = Random.value > 0.5f;
        SpawnResultText(won);

        yield return new WaitForSeconds(3.5f);

        // 6. Cleanup
        if (activeCoin != null) Destroy(activeCoin);
        if (spawnedFlipCam != null) Destroy(spawnedFlipCam.gameObject);

        // Find and destroy the text we spawned
        GameObject resultText = GameObject.Find("FlipResultText");
        if (resultText != null) Destroy(resultText);

        if (testSceneRoot != null) testSceneRoot.SetActive(true);

        isRunning = false;
    }

    void SpawnResultText(bool won)
    {
        GameObject textObj = new GameObject("FlipResultText");
        // Place it slightly above the coin so it doesn't clip
        textObj.transform.position = highSpawnPos + Vector3.up * 0.5f;
        // Rotate it to face the overhead camera
        textObj.transform.rotation = Quaternion.Euler(90, 0, 0);

        TextMeshPro tm = textObj.AddComponent<TextMeshPro>();
        tm.text = won ? "WIN" : "DEFEAT";
        tm.color = won ? Color.green : Color.red;
        tm.alignment = TextAlignmentOptions.Center;
        tm.fontSize = 10;

        // Ensure it's rendered on top
        tm.sortingOrder = 5;
    }
}