# MOBA Systems - Setup Guide

This guide explains how to properly configure NetworkObject components and integrate the MOBA systems into your Unity project using FishNet.

---

## Prerequisites

### Required Unity Packages

- **FishNet Networking** (already installed)
- **Unity NavMesh** components
- **TextMeshPro** (for UI)

### Scene Setup

1. Bake a NavMesh surface in your game scene:
    - Window → AI → Navigation
    - Select walkable surfaces
    - Click "Bake"

### Layer Configuration

Create the following layers in your project:

- `Champion` (Layer 6 recommended)
- `Minion` (Layer 7 recommended)
- `Tower` (Layer 8 recommended)

To create layers:

1. Edit → Project Settings → Tags and Layers
2. Add the three layers above

---

## Component Setup by Entity Type

### 1. Player/Champion Prefab

**Required Components:**

```
GameObject (Layer: Champion)
├─ NetworkObject
├─ PlayerController
├─ TeamComponent
├─ HealthSystem
├─ AutoAttackSystem
├─ VictoryDefeatUI (attach to player)
├─ NavMeshAgent
├─ NetworkTransform (or PredictedObject)
├─ Collider (for detection)
└─ Camera (child object)
```

**Inspector Settings:**

- **NetworkObject**: Enable "Is Spawnable", set as Player spawnable
- **TeamComponent**: Set Team to Blue or Red (assigned at runtime)
- **HealthSystem**: Set Max Health (e.g., 500)
- **AutoAttackSystem**:
    - Attack Damage: 50
    - Attack Range: 2
    - Attack Cooldown: 1
    - Enemy Layer: Set to Minion, Tower, Champion
- **NavMeshAgent**: Configure speed, acceleration, etc.
- **VictoryDefeatUI**: Assign UI panel references (see UI Setup section)

---

### 2. Minion Prefab

**Required Components:**

```
GameObject (Layer: Minion)
├─ NetworkObject
├─ MinionAI
├─ TeamComponent
├─ HealthSystem
├─ NavMeshAgent
├─ NetworkTransform (or PredictedObject)
└─ Collider
```

**Inspector Settings:**

- **NetworkObject**: Enable "Is Spawnable"
- **MinionAI**:
    - Detection Range: 10
    - Attack Range: 2
    - Attack Damage: 15
    - Attack Cooldown: 1
    - **Lane Waypoints**: Create empty GameObjects along the lane path and assign them here
    - **Layer Masks**: Assign Champion, Minion, and Tower layers
- **TeamComponent**: Set Team to Blue or Red
- **HealthSystem**: Set Max Health (e.g., 100)
- **NavMeshAgent**: Configure speed

**Lane Waypoint Setup:**

1. Create empty GameObjects along the lane (e.g., "BlueTopLane_WP1", "BlueTopLane_WP2", etc.)
2. Drag them into the MinionAI's "Lane Waypoints" array in order
3. Minions will follow these waypoints when not in combat

---

### 3. Tower Prefab

**Required Components:**

```
GameObject (Layer: Tower)
├─ NetworkObject
├─ TowerAI
├─ TeamComponent
├─ HealthSystem
├─ SphereCollider (trigger, for detection)
└─ Collider (for being targeted)
```

**Inspector Settings:**

- **NetworkObject**: Enable "Is Spawnable"
- **TowerAI**:
    - Detection Range: 12
    - Attack Damage: 50
    - Attack Cooldown: 1.5
    - **Projectile Spawn Point**: Create a child transform at the top of the tower
    - **Projectile Prefab**: Assign TowerProjectile prefab
    - **Layer Masks**: Assign Champion, Minion, and Tower layers
- **TeamComponent**: Set Team to Blue or Red
- **HealthSystem**: Set Max Health (e.g., 2000)
- **SphereCollider**: Set Radius to match Detection Range, enable "Is Trigger"

---

### 4. Nexus/Base Prefab

**Required Components:**

```
GameObject
├─ NetworkObject (optional, if you want it networked)
├─ TeamComponent
├─ HealthSystem
└─ Collider
```

**Inspector Settings:**

- **TeamComponent**: Set Team to Blue or Red
- **HealthSystem**: Set Max Health (e.g., 5000)

**GameStateManager Integration:**

1. Place `GameStateManager` in your scene (one instance)
2. In GameStateManager Inspector:
    - Assign Blue Base's HealthSystem to "Blue Base Health"
    - Assign Red Base's HealthSystem to "Red Base Health"

---

### 5. Spawn Points

**Setup:**

1. Create two empty GameObjects in your scene:
    - "BlueSpawnPoint"
    - "RedSpawnPoint"
2. Add the `SpawnPoint` component to each
3. Set the Team field appropriately (Blue/Red)
4. Position them at your team's fountain/base

---

### 6. Tower Projectile Prefab

**Required Components:**

```
GameObject
├─ TowerProjectile
├─ MeshRenderer (visual)
└─ Optional: Trail Renderer, Particle System
```

**Inspector Settings:**

- **TowerProjectile**:
    - Speed: 15
    - Arc Height: 2
    - **Impact Effect**: Optional particle effect prefab

---

## Manager Objects (Scene Singletons)

Place these in your scene (one instance each):

### 1. GameStateManager

```
GameObject "GameStateManager"
└─ NetworkObject
└─ GameStateManager
```

- Assign Blue and Red Nexus HealthSystems in inspector

### 2. EconomyManager

```
GameObject "EconomyManager"
└─ NetworkObject
└─ EconomyManager
```

- Configure gold/XP rewards in inspector

### 3. RespawnManager

```
GameObject "RespawnManager"
└─ NetworkObject
└─ RespawnManager
```

- Set Respawn Delay (e.g., 10 seconds)

---

## UI Setup

### Victory/Defeat UI

Create a Canvas with two panels:

**Victory Panel:**

```
Canvas
└─ VictoryPanel (GameObject)
    ├─ Image (background)
    └─ TextMeshProUGUI ("VICTORY!")
```

**Defeat Panel:**

```
Canvas
└─ DefeatPanel (GameObject)
    ├─ Image (background)
    └─ TextMeshProUGUI ("DEFEAT")
```

Then on each Player prefab:

1. Reference the VictoryDefeatUI component
2. Assign:
    - Victory Panel → VictoryPanel GameObject
    - Defeat Panel → DefeatPanel GameObject
    - Victory Text → TextMeshProUGUI in victory panel
    - Defeat Text → TextMeshProUGUI in defeat panel

---

## Network Spawning

### Server-Side Minion Spawning Example

Create a `MinionSpawner.cs` script:

```csharp
using FishNet.Object;
using UnityEngine;

public class MinionSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TeamId team;
    [SerializeField] private float spawnInterval = 30f;

    public override void OnStartServer()
    {
        base.OnStartServer();
        InvokeRepeating(nameof(SpawnMinion), 5f, spawnInterval);
    }

    [Server]
    private void SpawnMinion()
    {
        GameObject minion = Instantiate(minionPrefab, spawnPoint.position, spawnPoint.rotation);

        // Set team before spawning
        TeamComponent teamComp = minion.GetComponent<TeamComponent>();
        if (teamComp != null)
        {
            teamComp.Team = team;
        }

        ServerManager.Spawn(minion);
    }
}
```

---

## Testing Checklist

### Phase 1: Win/Loss

- [ ] Reduce Nexus health to 0 in Inspector
- [ ] Verify Victory/Defeat screen shows correctly
- [ ] Verify all player input is disabled

### Phase 2: Minions

- [ ] Spawn minions from both teams
- [ ] Verify they follow lane waypoints
- [ ] Verify they attack each other when meeting
- [ ] Kill a minion and verify gold/XP rewards in console

### Phase 3: Towers

- [ ] Stand near enemy tower without attacking
- [ ] Attack ally champion near tower
- [ ] Verify tower switches aggro to you ("Call for Help")
- [ ] Verify projectile visual spawns

### Phase 4: Respawn

- [ ] Kill player character
- [ ] Verify visuals disappear
- [ ] Wait for respawn timer
- [ ] Verify player respawns at team spawn point

---

## Common Issues

### "NavMeshAgent not on NavMesh"

- Ensure NavMesh is baked
- Verify spawn points are on the NavMesh surface

### "Target is null" errors

- Check that all entities have TeamComponent
- Verify layer masks are correctly assigned

### Gold/XP not working

- Ensure EconomyManager is in scene with NetworkObject
- Verify HealthSystem.OnDeath events are firing
- Check that killer has NetworkObject component

### Tower not attacking

- Verify SphereCollider is set to "Is Trigger"
- Check layer mask assignments
- Ensure TowerAI Detection Range matches collider radius

### Players don't respawn

- Verify RespawnManager is in scene
- Check that SpawnPoints exist with correct team assignments
- Ensure players have HealthSystem component

---

## Performance Tips

1. **Use Object Pooling**: For minions and projectiles
2. **Limit Detection Ranges**: Don't make them too large
3. **Use Physics Layers**: Properly configure layer collision matrix
4. **Optimize NavMesh**: Lower precision for minions

---

## Next Steps

1. Add ability systems (already have FireballAbility as example)
2. Implement items and inventory
3. Add creep score (CS) tracking to UI
4. Create jungle camps with neutral monsters
5. Add fog of war system
6. Implement minimap

---

For questions or issues, refer to the implementation plan document or review the individual script comments.
