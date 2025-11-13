# Agent Observations - What Agents Can See

## Yes, Agents Know the Other Team Exists!

Agents **can observe and distinguish** between teammates and enemies. Here's what they see:

## Entity Observations (k-NN System)

For each of the **5 nearest entities** (teammates + enemies), agents observe:

1. **Distance** (normalized): How far away the entity is
2. **Is Teammate** (0 or 1): 
   - `1.0` = Same team (teammate)
   - `0.0` = Different team (enemy)
3. **Health** (normalized): Entity's current health percentage
4. **Carrying Mana** (0 or 1): Whether the entity is carrying mana

### Example Observation

If a Noxus agent sees:
- Entity 1: Distance=0.5, IsTeammate=1.0, Health=0.8, HasMana=1.0
  → This is a Noxus teammate at medium distance, 80% health, carrying mana

- Entity 2: Distance=0.2, IsTeammate=0.0, Health=0.5, HasMana=0.0
  → This is an Ionia enemy very close, 50% health, not carrying mana

## Complete Observation Space

### Self State (8 features)
- Health (normalized)
- Mana carried (normalized)
- Attack cooldown (normalized)
- Team ID (Noxus=1, Ionia=0)
- Position X (normalized)
- Position Z (normalized)
- Rotation Y (normalized)
- Velocity magnitude (normalized)

### Nearby Entities (5 × 4 = 20 features)
- 5 nearest agents (teammates + enemies)
- For each: distance, is_teammate, health, carrying_mana

### Nearest Mana Items (3 × 3 = 9 features)
- 3 nearest mana items
- For each: distance, direction_x, direction_z

### Nearest Obstacles (3 × 2 = 6 features)
- 3 nearest obstacles
- For each: distance, velocity

### Zone Distances (2 features)
- Distance to home heal zone
- Distance to enemy heal zone

### Global Summary (3 features)
- Team mana banked (normalized)
- Enemy mana banked (normalized)
- Time remaining (normalized)

**Total: 48 observation features**

## What This Means

✅ **Agents CAN:**
- See enemies vs teammates
- Know enemy health
- Know if enemies are carrying mana
- Know distances to enemies
- See which enemies are closest

❌ **Agents CANNOT directly see:**
- Enemy team's total mana banked (only their own team's)
- Exact enemy positions (only relative distances)
- Enemy attack cooldowns
- Enemy movement direction (only distance)

## Training Implications

Since agents can observe enemies, they can learn to:
- **Target weak enemies** (low health)
- **Avoid strong enemies** (high health)
- **Steal mana** from enemies carrying it
- **Coordinate attacks** on specific enemies
- **Defend teammates** from enemies

The `Is Teammate` feature (0 or 1) is crucial - it tells the agent whether to help (teammate) or fight (enemy).

## Debugging Observations

To see what agents are observing, you can add debug logging in `CollectObservations()`:

```csharp
// Example: Log enemy detection
var enemies = nearbyEntities
    .Where(e => {
        var agent = e.GetComponent<BaseAgent>();
        return agent != null && agent.teamType != teamType;
    })
    .Count();
    
if (enemies > 0)
    Debug.Log($"{teamType} Agent {agentId} sees {enemies} enemies");
```

