using FishNet.Object;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Static utility class for target priority calculations used by Minions and Towers.
/// Implements MOBA-standard target priority: Champion attacking ally > Minion attacking ally > Closest enemy.
/// Uses HealthSystem-based detection instead of layer masks for simplicity.
/// </summary>
public static class TargetingUtility
{
    /// <summary>
    /// Get the best target based on MOBA priority rules.
    /// Priority: 1. Enemy Champion attacking Allied Champion
    ///           2. Enemy Minion attacking Allied Champion
    ///           3. Closest Enemy with HealthSystem
    /// </summary>
    /// <param name="sourcePosition">Position of the entity looking for a target</param>
    /// <param name="sourceTeam">Team component of the entity</param>
    /// <param name="detectionRange">Range to search for targets</param>
    /// <param name="championLayer">Layer mask for champions (used for priority targeting)</param>
    /// <returns>Transform of the best target, or null if none found</returns>
    public static Transform GetBestTarget(
        Vector3 sourcePosition,
        TeamComponent sourceTeam,
        float detectionRange,
        LayerMask championLayer)
    {
        if (sourceTeam == null) return null;

        // Get all potential targets in range
        Collider[] allColliders = Physics.OverlapSphere(sourcePosition, detectionRange);
        List<Transform> enemies = new List<Transform>();

        // Filter to only enemies with HealthSystem
        foreach (Collider col in allColliders)
        {
            HealthSystem health = col.GetComponent<HealthSystem>();
            if (health == null || health.IsDead) continue;

            TeamComponent targetTeam = col.GetComponent<TeamComponent>();
            if (targetTeam == null) continue;

            // Only consider enemies
            if (!sourceTeam.IsEnemyOf(targetTeam)) continue;

            enemies.Add(col.transform);
        }

        if (enemies.Count == 0) return null;

        // Priority 1: Enemy attacking Allied Champion
        Transform enemyAttackingAllyChampion = FindEnemyAttackingAllyChampion(enemies, sourceTeam, championLayer);
        if (enemyAttackingAllyChampion != null)
        {
            return enemyAttackingAllyChampion;
        }

        // Priority 2: Closest enemy
        return FindClosestEnemy(sourcePosition, enemies);
    }

    private static Transform FindEnemyAttackingAllyChampion(List<Transform> enemies, TeamComponent sourceTeam, LayerMask championLayer)
    {
        foreach (Transform enemy in enemies)
        {
            // Check if this enemy is attacking someone
            AutoAttackSystem attackSystem = enemy.GetComponent<AutoAttackSystem>();
            MinionAI minionAI = enemy.GetComponent<MinionAI>();
            
            Transform currentTarget = null;
            if (attackSystem != null)
            {
                currentTarget = attackSystem.GetCurrentTarget();
            }
            else if (minionAI != null)
            {
                currentTarget = minionAI.GetCurrentTarget();
            }

            if (currentTarget != null)
            {
                TeamComponent targetTeam = currentTarget.GetComponent<TeamComponent>();
                if (targetTeam != null && sourceTeam.IsAllyOf(targetTeam))
                {
                    // Check if target is a champion by layer
                    bool isChampion = ((1 << currentTarget.gameObject.layer) & championLayer) != 0;
                    if (isChampion)
                    {
                        return enemy;
                    }
                }
            }
        }

        return null;
    }

    private static Transform FindClosestEnemy(Vector3 sourcePosition, List<Transform> enemies)
    {
        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (Transform enemy in enemies)
        {
            float distance = Vector3.Distance(sourcePosition, enemy.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    /// <summary>
    /// Check if a transform is within range of a position
    /// </summary>
    public static bool IsInRange(Vector3 sourcePosition, Transform target, float range)
    {
        if (target == null) return false;
        return Vector3.Distance(sourcePosition, target.position) <= range;
    }
}
