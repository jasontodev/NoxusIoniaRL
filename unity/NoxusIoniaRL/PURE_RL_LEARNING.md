# Pure Reinforcement Learning Verification

## Actions Are Learned, Not Instructed

Agent behaviors are learned through **reinforcement learning** - agents discover optimal strategies through trial and error, with minimal hardcoded constraints.

## What Agents Must Learn

### 1. **Team Identification**
- ✅ **Provided**: "Is Teammate" observation (prevents friendly fire)
- ✅ **Hardcoded**: Attack action only targets enemies (cannot attack teammates)
- ✅ **Learned**: Agents must still learn:
  - **When** to attack (timing, positioning)
  - **Where** to position for combat
  - **How** to coordinate with teammates

### 2. **Combat Strategy**
- ❌ **Removed**: Reward for attacking enemies (+0.1f)
- ✅ **Learned**: Agents discover that:
  - Attacking enemies → Eliminations → Win rewards (+10.0f)
  - Agents must learn **when** to attack and **where** to position
  - No immediate reward for attacking - must learn through elimination/win rewards

### 3. **Mana Collection** (Pure RL - No Shaping)
- ❌ **Removed**: Pickup reward (+0.1f)
- ❌ **Removed**: Deposit reward (+1.0f per mana)
- ❌ **Removed**: Mana shaping reward (+0.05f per frame)
- ✅ **Pure RL**: Agents must learn through win/loss that:
  - Picking up mana → Depositing in heal zone → Resource-based wins
  - Must discover the entire chain: pickup → carry → deposit → win
  - No intermediate rewards guide this behavior

### 4. **Movement and Positioning**
- ✅ **Minimal shaping**: Idle penalty (-0.01f per frame) encourages activity
- ✅ **Learned**: Agents learn movement through:
  - Win/loss rewards guide overall strategy
  - Must discover optimal paths to mana, heal zones, and combat positions

## Complete Rewards and Penalties Reference

### 📊 **ACTIVE REWARDS AND PENALTIES**

This section documents **exactly** what rewards and penalties are currently active in the code.

#### **Sparse Rewards (Episode-Level Outcomes)**

| Reward/Penalty | Value | When Awarded | Scope | Purpose |
|---------------|-------|--------------|-------|---------|
| **Win** | **+10.0** | Team wins the episode (elimination or resource-based) | Team-level | Main learning signal for successful strategies |
| **Loss** | **-5.0** | Team loses the episode | Team-level | Penalty for losing, encourages winning |
| **Death** | **-1.0** | Agent dies (health reaches 0) | Individual | Penalty for being eliminated |

#### **Dense Rewards (Continuous Shaping)**

| Reward/Penalty | Value | When Awarded | Frequency | Purpose |
|---------------|-------|--------------|-----------|---------|
| **Idle Penalty** | **-0.01** | Agent is stationary (not moving) | Per frame while idle | Encourages activity and exploration |

#### **Disabled/Removed Rewards (Not Currently Active)**

The following reward variables exist in the code but are **NOT** being awarded:

| Reward Variable | Default Value | Status | Reason |
|----------------|---------------|--------|--------|
| `rewardPickup` | 0.1 | ❌ **DISABLED** | Agents must learn pickup through win/loss |
| `rewardDeposit` | 1.0 | ❌ **DISABLED** | Agents must learn deposit through win/loss |
| `rewardManaShaping` | 0.05 | ❌ **DISABLED** | Agents must learn mana collection through win/loss |
| `rewardElimination` | 2.0 | ❌ **NOT USED** | Defined but not awarded (elimination leads to win reward instead) |
| `rewardFriendlyBlock` | -0.1 | ❌ **NOT USED** | Friendly fire is prevented by hardcoded constraint |

#### **No Rewards Given For:**

- ❌ **Attacking enemies** - No immediate reward (must learn through elimination → win)
- ❌ **Picking up mana** - No reward (must learn through deposit → win)
- ❌ **Depositing mana** - No reward (must learn through resource-based win)
- ❌ **Moving toward heal zone with mana** - No shaping reward
- ❌ **Eliminating an enemy** - No direct reward (elimination leads to team win)

### 🎯 **Reward Flow Summary**

**Combat Path:**
```
Attack Enemy → Enemy Dies → Team Eliminates All Enemies → Team Wins → +10.0 reward
                                                                    → -5.0 penalty (losers)
```

**Resource Path:**
```
Pickup Mana → Carry Mana → Deposit in Heal Zone → Team Has More Mana at Timeout → Team Wins → +10.0 reward
                                                                                              → -5.0 penalty (losers)
```

**Death Path:**
```
Agent Dies → -1.0 penalty (individual)
```

**Idle Path:**
```
Agent Not Moving → -0.01 per frame (continuous penalty)
```

### 🔒 **Hardcoded Constraints (Not Learned)**

- ✅ **Team identification**: "Is Teammate" observation provided (prevents friendly fire)
- ✅ **Friendly fire prevention**: Attack action only targets enemies (cannot attack teammates)
- ❌ **No hardcoded pathfinding or strategy** - All movement and positioning must be learned

## Learning Process

Agents must discover:
1. **When to attack**: Through elimination rewards and win/loss (who to attack is hardcoded - only enemies)
2. **How to collect mana**: Through win/loss rewards only (no pickup/deposit rewards)
3. **Where to go**: Through zone distance observations and win/loss rewards
4. **Team coordination**: Through win/loss rewards (implicit team learning)
5. **Resource vs Combat strategy**: Must learn when to prioritize mana collection vs combat

## Verification Checklist

✅ **Attack Actions**: No reward for attacking enemies - must learn through elimination/win rewards  
✅ **Team Identification**: "Is Teammate" observation provided (prevents friendly fire)  
✅ **Friendly Fire**: Hardcoded prevention (attack only targets enemies)  
✅ **Mana Collection**: No pickup/deposit rewards - must learn through win/loss  
✅ **Movement**: No pathfinding instructions - must learn optimal paths  
✅ **Strategy**: No hardcoded playbooks - must discover through trial and error  
✅ **Rewards**: Only sparse win/loss and minimal idle penalty - no detailed instructions  

## Expected Learning Behavior

- **Early Training**: Random exploration, agents may not understand mana collection
- **Mid Training**: Agents discover that depositing mana leads to resource-based wins
- **Late Training**: Agents learn to balance combat (elimination wins) vs resource collection (resource wins)
- **Advanced**: Agents learn coordinated strategies (through team win/loss rewards)

## Pure RL for Resource Collection

Resource collection (pickup → deposit) is now **completely learned** through win/loss rewards:
- No intermediate rewards guide pickup behavior
- No intermediate rewards guide deposit behavior
- Agents must discover the entire chain through trial and error
- This makes training harder but produces more robust resource collection strategies

This approach emphasizes pure RL learning for resource collection while preventing friendly fire through hardcoded constraints.

