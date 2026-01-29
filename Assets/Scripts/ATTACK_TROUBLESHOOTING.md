# Attack System Troubleshooting Guide

## Issue: Minions/Players Not Attacking

### Debug Logs to Check

When you run the game, you should see these logs in order:

#### 1. Team Initialization

```
[TeamComponent] Champion initialized with team: Blue
[TeamComponent] Minion initialized with team: Red
```

**Problem if you see:**

- Both are `Neutral` → They won't attack each other (Neutral can't attack anyone)
- Both are same team (Blue/Blue or Red/Red) → Allies don't attack allies

**Fix:** Set Initial Team in Inspector:

- Champion → TeamComponent → Initial Team = Blue
- Minion → TeamComponent → Initial Team = Red

---

#### 2. Detection

```
[MinionAI] Minion found target: Champion (IsChampion: True)
```

**Problem if you DON'T see this:**

- Detection range too small (increase Detection Range in MinionAI)
- No Collider on Champion (must have Collider for Physics.OverlapSphere)
- Champion doesn't have HealthSystem component

---

#### 3. Team Check

```
[TeamComponent] Minion(Red) vs Champion(Blue) = true
```

**Problem if you see:**

- `= false` → They're on same team or one is Neutral
- `(one is Neutral)` → One entity is still Neutral team

---

#### 4. State Transition

```
[MinionAI] Minion transitioning from LanePush to AggroChampion
```

**Problem if you DON'T see this:**

- Target not recognized as champion (championLayer not set)
- Champion GameObject not on "Champion" layer in Inspector

---

#### 5. In Range Check

```
[MinionAI] Minion in range (1.50 <= 2), switching to Attacking
[MinionAI] Minion transitioning from AggroChampion to Attacking
```

**Problem if you DON'T see this:**

- NavMeshAgent not reaching target (no NavMesh baked)
- Attack Range too small (default 2 units, try 3-4)
- Minion stuck or can't pathfind

---

#### 6. Attack Execution

```
[MinionAI] Minion performing attack (cooldown ready: 1.02 >= 1)
```

**Problem if you DON'T see this:**

- Target became invalid (check for "target invalid" logs)
- Attack cooldown not ready
- PerformAttack not being called

---

## Common Issues & Solutions

### Issue 1: "Both entities are Neutral"

**Console shows:** `Minion(Neutral) vs Champion(Neutral) = false (one is Neutral)`

**Solution:**

1. Select Champion → TeamComponent component
2. Set "Initial Team" dropdown to **Blue**
3. Select Minion → TeamComponent component
4. Set "Initial Team" dropdown to **Red**
5. Make sure to save the prefab if editing prefab

---

### Issue 2: "Same team"

**Console shows:** `Minion(Blue) vs Champion(Blue) = false`

**Solution:**

- Set Champion to Blue, Minion to Red (or vice versa)
- They must be on **opposite teams**

---

### Issue 3: "No detection logs at all"

**Console shows:** Nothing related to MinionAI

**Check:**

1. Game is in **Host** or **Server** mode (not Editor-only)
2. Minion has `NetworkObject` and is spawned via `ServerManager.Spawn()`
3. Champion has:
    - `HealthSystem` component
    - `TeamComponent` component
    - A `Collider` (BoxCollider, SphereCollider, etc.)
    - NOT on same layer as Minion

---

### Issue 4: "Detection works but no attack"

**Console shows:** Detection and state transitions, but no "performing attack"

**Check:**

1. Minion's Attack Range in Inspector (try increasing to 3-4)
2. NavMesh is baked in the scene
3. Minion can actually reach the Champion (no obstacles)

---

### Issue 5: "Champion can't attack Minion back"

**If AutoAttackSystem is on Champion:**

**Check:**

1. Champion has `TeamComponent` with Initial Team set
2. Minion has `TeamComponent` with opposite team
3. Check Console for team comparison logs
4. Verify AutoAttackSystem's Attack Range (default 2 units)

---

## Quick Diagnostic Test

Add this to MinionAI's `Update()` method temporarily:

```csharp
// In Update(), add after "if (!IsServerInitialized) return;"
if (Input.GetKeyDown(KeyCode.T))
{
    Debug.Log($"=== MINION DEBUG ===");
    Debug.Log($"Current State: {currentState.Value}");
    Debug.Log($"Current Target: {(currentTarget != null ? currentTarget.name : "null")}");
    Debug.Log($"My Team: {teamComponent.Team}");

    // Test detection
    Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
    Debug.Log($"Found {hits.Length} colliders in range");
    foreach (Collider col in hits)
    {
        HealthSystem hs = col.GetComponent<HealthSystem>();
        TeamComponent tc = col.GetComponent<TeamComponent>();
        Debug.Log($"  - {col.name}: HealthSystem={hs != null}, Team={tc?.Team}, IsEnemy={tc != null && teamComponent.IsEnemyOf(tc)}");
    }
}
```

Press **T** in Play mode to see detailed debug info!

---

## Step-by-Step Checklist

- [ ] Champion has HealthSystem component
- [ ] Champion has TeamComponent with Initial Team = Blue
- [ ] Champion has Collider (not trigger)
- [ ] Champion GameObject layer = "Champion"
- [ ] Minion has HealthSystem component
- [ ] Minion has TeamComponent with Initial Team = Red
- [ ] Minion has Collider
- [ ] Minion has MinionAI component
- [ ] MinionAI has "Champion Layer" set to "Champion"
- [ ] NavMesh is baked in scene
- [ ] Both have NetworkObject and are spawned
- [ ] Game running in Host/Server mode

---

If you've checked everything and it still doesn't work, **copy all Console logs** and share them - they'll tell us exactly where it's failing!
