using System;
using FishNet.Object;
using UnityEngine;

public class BasicAttack : NetworkBehaviour
{
    private CharacterStats _stats;
    private TeamComponent _team;

    private float _lastAttackTime;

    public event Action<GameObject> OnPreAttack;
    public event Action<GameObject, float> OnPostAttack; // target, damage done

    private void Awake()
    {
        _stats = GetComponent<CharacterStats>();
        _team = GetComponent<TeamComponent>();

        if (_stats == null)
            Debug.LogError($"[BasicAttack] No CharacterStats found on {gameObject.name}!");
        if (_team == null)
            Debug.LogError($"[BasicAttack] No TeamComponent found on {gameObject.name}!");
    }

    public bool CanAttack(GameObject target)
    {
        if (target == null) return false;

        float attackCooldown = 1f / _stats.attackSpeed.Value;
        if (Time.time - _lastAttackTime < attackCooldown) return false;

        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > _stats.attackRange.Value) return false;

        TeamComponent targetTeam = target.GetComponent<TeamComponent>();
        if (!_team.IsEnemy(targetTeam)) return false;

        return true;
    }

    public void Attack(GameObject target)
    {
        if (!CanAttack(target)) return;

        _lastAttackTime = Time.time;

        OnPreAttack?.Invoke(target);

        NetworkObject targetNetObj = target.GetComponent<NetworkObject>();
        if (targetNetObj == null)
        {
            Debug.LogWarning($"[BasicAttack] Target {target.name} has no NetworkObject!");
            return;
        }

        ServerAttack(targetNetObj);
    }

    [ServerRpc]
    private void ServerAttack(NetworkObject targetNetObj)
    {
        if (targetNetObj == null) return;

        HealthComponent health = targetNetObj.GetComponent<HealthComponent>();
        if (health == null)
        {
            Debug.LogWarning($"[BasicAttack] Target {targetNetObj.name} has no HealthComponent!");
            return;
        }

        DamageType damageType = _stats.data.basicAttackDamageType;
        float damage = damageType == DamageType.Magical
            ? _stats.abilityPower.Value
            : _stats.attackDamage.Value;

        float recievedDamage = health.TakeDamage(damage, damageType, _stats);

        RpcOnPostAttack(targetNetObj.gameObject, recievedDamage);

    }

    [ObserversRpc]
    private void RpcOnPostAttack(GameObject target, float damage)
    {
        OnPostAttack?.Invoke(target, damage);
    }

    private void OnDrawGizmosSelected()
    {
        if (_stats == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _stats.attackRange.Value);
    }
}