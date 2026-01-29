# Minion Leash System

## Overview

Minions now have a "leash" that prevents them from chasing enemies too far from their current waypoint. This ensures minions stay near their lane and don't get pulled into bad positions.

## How It Works

### Leash Distance

- **Default**: 15 units from current waypoint
- **Configurable**: Via Inspector → MinionAI → "Max Chase Distance"

### Behavior

1. **Normal State (LanePush)**
    - Minion follows lane waypoints
    - No leash restrictions

2. **Chasing Champion (AggroChampion)**
    - Minion pursues enemy champion
    - **Checks distance from current waypoint every frame**
    - If distance > `maxChaseDistance`:
        - Stops chasing
        - Returns to LanePush state
        - Resumes lane pushing

3. **In Combat (Attacking)**
    - Even while attacking, leash is checked
    - If pulled beyond leash distance:
        - Disengages from combat
        - Returns to lane

### Visual Gizmos

When selecting a minion in Scene view:

- **Yellow sphere** = Detection Range
- **Red sphere** = Attack Range
- **Blue waypoints** = Lane path

## Configuration

**In Inspector (MinionAI component):**

```
Detection Range: 10     - How far minion can see enemies
Attack Range: 2         - How close to attack
Max Chase Distance: 15  - How far from waypoint before giving up
```

### Recommended Settings

**Aggressive Minions** (chase farther):

- Max Chase Distance: 20-25

**Defensive Minions** (stay in lane):

- Max Chase Distance: 10-12

**Neutral Jungle Camps** (don't chase at all):

- Max Chase Distance: 5
- Detection Range: 8

## Example Scenario

```
Waypoint is at position (0, 0, 0)
Minion detects enemy at (0, 0, 10)
Max Chase Distance = 15

1. Minion at (0, 0, 0) - distance from waypoint: 0 ✓
2. Minion at (0, 0, 8) - distance from waypoint: 8 ✓
3. Minion at (0, 0, 14) - distance from waypoint: 14 ✓
4. Minion at (0, 0, 16) - distance from waypoint: 16 ✗
   → Leash triggered! Returns to waypoint
```

## Console Logs

When leash activates:

```
[MinionAI] Minion exceeded leash distance (18.23 > 15), returning to lane
[MinionAI] Minion transitioning from AggroChampion to LanePush
```

---

# Neutral Team Mechanics

## Overview

Neutral entities (jungle camps, neutral monsters) can now attack and be attacked by both Blue and Red teams.

## Team Behavior Changes

### Before:

- Neutral couldn't attack anyone
- Nobody could attack Neutral
- Jungle camps were useless

### After:

- **Neutral attacks everyone** (including other Neutrals)
- **Everyone can attack Neutral**
- Perfect for jungle camps!

## Use Cases

### 1. Jungle Camps

```
GameObject: DragonCamp
├─ HealthSystem
├─ TeamComponent (Initial Team: Neutral)
├─ MinionAI
└─ Collider

Result: Both teams can fight the dragon!
```

### 2. Neutral Minions

```
GameObject: NeutralCreep
└─ TeamComponent (Team: Neutral)

- Blue team can attack it
- Red team can attack it
- It can attack both teams
```

### 3. Environmental Hazards

```
GameObject: AngryTreent
└─ TeamComponent (Team: Neutral)

- Attacks any player nearby
- Both teams can destroy it
```

## Team Interaction Matrix

| Attacker → Target | Blue | Red | Neutral |
| ----------------- | ---- | --- | ------- |
| **Blue**          | ✗    | ✓   | ✓       |
| **Red**           | ✓    | ✗   | ✓       |
| **Neutral**       | ✓    | ✓   | ✓       |

✓ = Can attack
✗ = Cannot attack (allies)

## Console Example

```
[TeamComponent] Dragon(Neutral) vs BluePlayer(Blue) = true (Neutral can attack/be attacked)
[TeamComponent] RedPlayer(Red) vs Dragon(Neutral) = true (Neutral can attack/be attacked)
[TeamComponent] Dragon1(Neutral) vs Dragon2(Neutral) = true (Neutral can attack/be attacked)
```

---

## Setup Example: Jungle Camp

1. **Create Dragon Prefab**
    - Add HealthSystem (e.g., 1000 HP)
    - Add TeamComponent → Initial Team = **Neutral**
    - Add MinionAI
    - Set Max Chase Distance = 8 (short leash, stays near spawn)
    - Add Collider

2. **Configure Waypoints**
    - Create 2-3 waypoints in small circle around spawn
    - Dragon patrols this area
    - Won't chase beyond 8 units

3. **Result**
    - Both teams can fight the dragon
    - Dragon fights back
    - Doesn't chase players across map
    - Respawns at camp location

Perfect for jungle objectives! 🐉
