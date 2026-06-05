using UnityEngine;
using System.Collections;

public class TestAb : MonoBehaviour
{
    [Header("Spin Settings")]
    public float spinVelocity = 10000f;
    public AudioClip spinSound;
    private bool _isSpinning = false;
    private float _spinDuration = 4.0f;

    [Header("Shooting Settings")]
    public GameObject rockPrefab;
    public float shootForce = 20f;
    public ForceMode shootMode = ForceMode.VelocityChange;
    public AudioClip shootSound;

    private AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !_isSpinning)
        {
            StartCoroutine(SpinRoutine());
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (rockPrefab != null)
        {
            if (shootSound != null)
            {
                _audioSource.PlayOneShot(shootSound);
            }

            GameObject rock = Instantiate(rockPrefab, transform.position + transform.forward, transform.rotation);

            Rigidbody rb = rock.GetComponent<Rigidbody>();

            if (rb == null)
            {
                rb = rock.AddComponent<Rigidbody>();
            }

            rb.isKinematic = false;
            rb.useGravity = false;

            rb.AddForce(transform.forward * shootForce, shootMode);

            Destroy(rock, 2.0f);
        }
    }

    private IEnumerator SpinRoutine()
    {
        _isSpinning = true;

        if (spinSound != null)
        {
            _audioSource.PlayOneShot(spinSound);
        }

        float elapsed = 0f;

        while (elapsed < _spinDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / _spinDuration;

            float currentSpeed = Mathf.Sin(normalizedTime * Mathf.PI) * spinVelocity;

            transform.Rotate(Vector3.up, currentSpeed * Time.deltaTime);

            yield return null;
        }

        _isSpinning = false;
    }
}