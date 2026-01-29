# Passive Neutral Entities

## Overview

Neutral entities (jungle camps, neutral monsters) are now **passive** - they won't attack unless attacked first.

## How It Works

### Before Attack

```
Player: "I'll walk past this dragon"
Dragon: *ignores player*
```

### After Attack

```
Player: *attacks dragon*
Dragon: "You have provoked me!" *fights back*
```

## Technical Details

### Provocation System

1. **Initial State**: Neutral entity is passive
    - Doesn't attack anyone
    - `IsEnemyOf(player)` returns `false`

2. **Player Attacks Neutral**:
    - `HealthSystem.TakeDamage()` called
    - Neutral entity marks player's team as "provoked"
    - `provokedByBlue = true` or `provokedByRed = true`

3. **Neutral Becomes Hostile**:
    - Now `IsEnemyOf(player)` returns `true`
    - Neutral entity will attack that team
    - Stays hostile for the rest of its lifetime

### Asymmetric Enemy Detection

| Attacker             | Target               | IsEnemyOf? | Result                   |
| -------------------- | -------------------- | ---------- | ------------------------ |
| Blue                 | Neutral (unprovoked) | ✓          | Blue can attack          |
| Neutral (unprovoked) | Blue                 | ✗          | Neutral won't attack     |
| Blue                 | Neutral (provoked)   | ✓          | Blue can still attack    |
| Neutral (provoked)   | Blue                 | ✓          | **Neutral fights back!** |

## Example Scenarios

### Scenario 1: Jungle Camp

```
Dragon Camp (Neutral)
- Initial: Passive, doesn't attack anyone
- Blue player attacks → Dragon provoked by Blue
- Dragon now attacks Blue team players
- Red team can still walk by safely
- Red player attacks → Dragon now hostile to both teams
```

### Scenario 2: Multiple Neutrals

```
Dragon 1 (Neutral) and Dragon 2 (Neutral)
- Blue attacks Dragon 1 → Dragon 1 provoked
- Dragon 1 fights Blue
- Dragon 2 is still passive (separate entity)
- Dragon 2 won't attack until it's provoked
```

### Scenario 3: Reset on Leash

```
Neutral Golem at camp
- Blue attacks → Golem provoked
- Blue runs away beyond leash distance
- Golem returns to camp
- Golem is still provoked by Blue!
- Blue can't safely approach anymore
```

## Console Logs

**Unprovoked:**

```
[TeamComponent] Player(Blue) vs Dragon(Neutral) = true (Neutral can be attacked)
[TeamComponent] Dragon(Neutral) vs Player(Blue) = false (Neutral provoked=false)
```

**After Being Attacked:**

```
[HealthSystem] Dragon took 50 damage
[TeamComponent] Dragon provoked by Blue team
[TeamComponent] Dragon(Neutral) vs Player(Blue) = true (Neutral provoked=true)
```

## Configuration

### Make Jungle Camp Passive

```
GameObject: DragonCamp
├─ TeamComponent → Initial Team: Neutral
├─ HealthSystem
├─ MinionAI
│   ├─ Max Chase Distance: 8 (short leash)
│   └─ Detection Range: 10
└─ Collider

Result:
- Won't attack until hit
- Fights back when provoked
- Stays near camp (leash)
```

### Make Aggressive Neutral

If you want a neutral that attacks on sight:

- Set Initial Team to **Blue** or **Red** (not Neutral)
- Or manually call `ProvokeByTeam(TeamId.Blue)` and `ProvokeByTeam(TeamId.Red)` on spawn

## Use Cases

### 1. Jungle Camps (Passive)

✓ Dragon, Baron, Buffs

- Reward for players who engage
- Risk vs reward
- No accidental aggro

### 2. Roaming Monsters (Aggressive)

For monsters that should attack on sight:

- Don't use Neutral team
- Use a custom "Monster" team
- Or provoke both teams on spawn

### 3. Neutral Lanes

Minions that help whoever attacks the enemy:

- Start as Neutral
- Change team when attacked by enemy

## Important Notes

⚠️ **Provocation is permanent** for the entity's lifetime

- Once provoked, stays provoked
- Doesn't reset on leash return
- Doesn't reset on healing

⚠️ **Team-based, not player-based**

- Provoke affects entire Blue or Red team
- Not individual players
- All Blue players are targets once provoked by one

✅ **Safe exploration**

- Players can scout jungle without fighting
- Choose when to engage
- Strategic decision-making

---

Perfect for MOBA-style jungle objectives! 🎮
