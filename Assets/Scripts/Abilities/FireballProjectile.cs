using FishNet;
using FishNet.Object;
using UnityEngine;

public class FireballProjectile : NetworkBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float maxRange = 20f;
    [SerializeField] private GameObject hitVfxPrefab;
    [SerializeField] private AudioClip hitSound;

    private Vector3 _direction;
    private float _damage;
    private DamageType _damageType;
    private CharacterStats _attackerStats;
    private GameObject _ownerObject;
    private Vector3 _startPosition;

    [Server]
    public void Initialize(Vector3 direction, float damage, DamageType damageType, CharacterStats attackerStats, GameObject owner)
    {
        _direction = direction.normalized;
        _damage = damage;
        _damageType = damageType;
        _attackerStats = attackerStats;
        _ownerObject = owner;
        _startPosition = transform.position;
    }

    private void Update()
    {
        if (!IsServerInitialized) return;

        transform.position += _direction * speed * Time.deltaTime;
        transform.forward = _direction;

        // Max range check
        if (Vector3.Distance(transform.position, _startPosition) > maxRange)
        {
            InstanceFinder.ServerManager.Despawn(NetworkObject);
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized) return;
        if (other.gameObject == _ownerObject) return;

        // Ignore triggers (like ability zones)
        if (other.isTrigger) return;

        TeamComponent myTeam = _ownerObject?.GetComponent<TeamComponent>();
        TeamComponent otherTeam = other.GetComponentInParent<TeamComponent>();

        // Don't hit teammates
        if (myTeam != null && otherTeam != null && !myTeam.IsEnemy(otherTeam)) return;

        HealthComponent health = other.GetComponentInParent<HealthComponent>();
        if (health != null && !health.IsDead)
            health.TakeDamage(_damage, _damageType, _attackerStats);

        RpcSpawnHitEffects(transform.position);
        InstanceFinder.ServerManager.Despawn(NetworkObject);
    }

    [ObserversRpc]
    private void RpcSpawnHitEffects(Vector3 position)
    {
        if (hitVfxPrefab != null)
        {
            GameObject vfx = Instantiate(hitVfxPrefab, position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, position);
    }
}