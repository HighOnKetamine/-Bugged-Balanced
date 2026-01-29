# Minion Detection Troubleshooting Guide

## Issue: Minions not attacking champions

### Checklist:

#### 1. **Layer Configuration**

- [ ] Created layers: Champion (Layer 6), Minion (Layer 7), Tower (Layer 8)
    - Edit → Project Settings → Tags and Layers
- [ ] Player/Champion GameObject is on the **Champion** layer
    - Select player prefab/instance
    - Set Layer dropdown to "Champion"
- [ ] Minion GameObject is on the **Minion** layer

#### 2. **MinionAI Inspector Settings**

- [ ] `Champion Layer` field is assigned to "Champion" layer
- [ ] `Minion Layer` field is assigned to "Minion" layer
- [ ] `Tower Layer` field is assigned to "Tower" layer
- [ ] Detection Range is reasonable (default: 10 units)

#### 3. **Collider Requirements**

- [ ] Player/Champion has a **Collider** component (Box/Sphere/Capsule)
    - Must NOT be a trigger for OverlapSphere to detect it
- [ ] Minion has a **Collider** component

#### 4. **TeamComponent Configuration**

- [ ] Player has `TeamComponent` with Initial Team set to Blue or Red
- [ ] Minion has `TeamComponent` with Initial Team set to opposite team
- [ ] Teams are actually different (enemy detection requires opposite teams)

#### 5. **NetworkObject Requirements**

- [ ] Both Player and Minion have `NetworkObject` component
- [ ] Both are spawned via `ServerManager.Spawn()`
- [ ] Game is running in Host mode or Server/Client mode (not Editor alone)

---

## Debug Steps:

### Step 1: Check Console Logs

Play the game and watch for these logs:

```
[MinionAI] MinionName found target: PlayerName (Layer: 6, IsChampion: True)
[MinionAI] MinionName transitioning from LanePush to AggroChampion
```

**If you see these logs**: Detection is working! Issue is in attack logic.
**If you don't see these logs**: Detection is failing. Continue below.

### Step 2: Verify Layer Masks in Inspector

1. Select the Minion in the scene
2. Look at MinionAI component
3. Click on "Champion Layer" field
4. Verify "Champion" is checked ✓

### Step 3: Check Colliders

1. Select Player in scene
2. Verify it has a Collider (BoxCollider, SphereCollider, or CapsuleCollider)
3. Verify "Is Trigger" is **UNCHECKED** (regular collider)

### Step 4: Verify Team Setup

1. Select Player → Check TeamComponent → Initial Team should be Blue or Red
2. Select Minion → Check TeamComponent → Initial Team should be opposite
3. Play the game → Check Console for "Initial Team" being applied

### Step 5: Test Detection Range Visually

1. Select Minion in scene
2. With MinionAI selected, you should see a **yellow wire sphere** in Scene view
3. This is the detection range (default 10 units)
4. Ensure Player is inside this sphere

### Step 6: Simplified Test

Create a simple test to verify OverlapSphere works:

```csharp
// Add to MinionAI Update() temporarily
if (IsServerInitialized && Input.GetKeyDown(KeyCode.T))
{
    Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, championLayer);
    Debug.Log($"Found {hits.Length} champions in range");
    foreach(Collider hit in hits)
    {
        Debug.Log($"  - {hit.name} on layer {hit.gameObject.layer}");
    }
}
```

Press T in Play mode to manually test detection.

---

## Common Issues & Solutions:

| Issue                         | Solution                                                   |
| ----------------------------- | ---------------------------------------------------------- |
| "Found 0 champions"           | Layer mask not assigned or player not on Champion layer    |
| "IsChampion: False"           | Player GameObject layer is not set to Champion             |
| No logs at all                | Minion not spawned on server, or not in LanePush state     |
| Detection works but no attack | Check HealthSystem and attack range settings               |
| Champions detected as minions | Layer assignment wrong - check player's layer in Inspector |

---

## Quick Fix Command:

If you have access to the player GameObject at runtime:

```csharp
// Set player to Champion layer (Layer 6)
player.gameObject.layer = LayerMask.NameToLayer("Champion");
```

---

**Most Common Cause**: Layer masks not assigned in Inspector! Check the MinionAI component on your minion prefab/instance.
