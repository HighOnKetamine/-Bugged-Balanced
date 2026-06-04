// using UnityEngine;

// public class DamageTest : MonoBehaviour
// {
//     [Header("Settings")]
//     public GameObject damagePrefab;
//     public float heightOffset = 2.5f;
//     public float jitterAmount = 0.5f;

//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             SpawnDamageNumber();
//         }
//     }

//     void SpawnDamageNumber()
//     {
//         if (damagePrefab == null) return;

//         float randomX = Random.Range(-jitterAmount, jitterAmount);
//         float randomZ = Random.Range(-jitterAmount, jitterAmount);

//         Vector3 spawnPos = transform.position + new Vector3(randomX, heightOffset, randomZ);

//         GameObject go = Instantiate(damagePrefab, spawnPos, Quaternion.identity);

//         if (go.TryGetComponent<FloatingDmg>(out FloatingDmg dmgScript))
//         {
//             dmgScript.SetDamageValue(Random.Range(10, 100));
//         }
//     }
// }