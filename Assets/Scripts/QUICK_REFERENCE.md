# MOBA Systems - Quick Reference

## Scripts Created (8 new files)

| Script                | Purpose                   | Lines |
| --------------------- | ------------------------- | ----- |
| `VictoryDefeatUI.cs`  | Victory/Defeat UI handler | ~115  |
| `MinionAI.cs`         | Minion AI with FSM        | ~320  |
| `EconomyManager.cs`   | Gold/XP distribution      | ~200  |
| `TargetingUtility.cs` | Shared targeting logic    | ~165  |
| `TowerAI.cs`          | Tower AI with aggro swap  | ~245  |
| `TowerProjectile.cs`  | Tower attack visuals      | ~75   |
| `RespawnManager.cs`   | Player respawn system     | ~190  |
| `SpawnPoint.cs`       | Spawn location marker     | ~20   |

## Scripts Enhanced (4 files)

| Script                | Changes                                             |
| --------------------- | --------------------------------------------------- |
| `GameStateManager.cs` | Added `DisableAllPlayerInput()`                     |
| `PlayerController.cs` | Added `isInputEnabled` flag and `SetInputEnabled()` |
| `HealthSystem.cs`     | Added `lastAttacker` tracking                       |
| `AutoAttackSystem.cs` | Added `GetCurrentTarget()` method                   |

## Key Components

### Minion FSM States

1. **LanePush** - Following waypoints
2. **AggroChampion** - Chasing enemy champion
3. **Attacking** - In combat
4. **Dead** - Disabled

### Target Priority (Minions & Towers)

1. Enemy Champion attacking Allied Champion
2. Enemy Minion attacking Allied Champion
3. Closest Enemy Minion
4. Closest Enemy Tower

### Tower "Call for Help"

- Detects enemy champion damaging ally in range
- Immediately switches aggro to attacker
- Overrides normal priority

### Economy System

- **Gold**: Via `[TargetRpc]` to killer only
- **XP**: Distributed to all allies in 15 unit range
- Rewards:
    - Minion: 20g, 50xp
    - Champion: 300g, 200xp
    - Tower: 150g, 0xp

### Respawn Flow

1. Death detected → Visuals disabled
2. 10 second timer
3. Teleport to team spawn point
4. Reset health, re-enable movement
5. Re-enable visuals

## FishNet Patterns Summary

```csharp
// Server-only logic
[Server]
private void ServerMethod() { }

// Sync state to all clients
[SerializeField]
private readonly SyncVar<T> variable = new SyncVar<T>(
    new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers)
);

// Call on all clients
[ObserversRpc]
private void RpcMethod() { }

// Call on specific client
[TargetRpc]
private void TargetMethod(NetworkConnection conn) { }
```

## Unity Editor TODO

- [ ] Create layers: Champion, Minion, Tower
- [ ] Bake NavMesh
- [ ] Configure prefabs (see SETUP_GUIDE.md)
- [ ] Add manager objects to scene
- [ ] Create spawn points

## Next Steps

1. Open Unity → Check compilation
2. Configure layers via Edit → Project Settings
3. Bake NavMesh via Window → AI → Navigation
4. Create prefabs following SETUP_GUIDE.md
5. Test each system in Play Mode

---

**Documentation Files:**

- [SETUP_GUIDE.md](file:///d:/programming/-Bugged-Balanced/Assets/Scripts/SETUP_GUIDE.md) - Detailed Unity setup
- [walkthrough.md](file:///C:/Users/dubma/.gemini/antigravity/brain/d9f4224f-f9fe-4d46-9b4b-df15fbfc35f2/walkthrough.md) - Implementation details
- [implementation_plan.md](file:///C:/Users/dubma/.gemini/antigravity/brain/d9f4224f-f9fe-4d46-9b4b-df15fbfc35f2/implementation_plan.md) - Original technical plan
