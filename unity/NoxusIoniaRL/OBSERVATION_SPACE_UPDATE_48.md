# Observation Space Update: Back to 48 Features

## Change Summary

The "Is Teammate" observation has been **added back** to prevent agents from attacking teammates.

## Observation Space Breakdown

**Total: 48 features**

- **Self state**: 8 features
- **k-NN entities (5 × 4)**: 20 features
  - Distance (normalized)
  - **Is Teammate** (1.0 if teammate, 0.0 if enemy)
  - Health (normalized)
  - Carrying mana (1.0 if yes, 0.0 if no)
- **Nearest mana (3 × 3)**: 9 features
- **Nearest obstacles (3 × 2)**: 6 features
- **Distances to zones**: 2 features
- **Global summary**: 3 features

**Total: 8 + 20 + 9 + 6 + 2 + 3 = 48 features**

## Why "Is Teammate" Was Added Back

1. **Prevents Friendly Fire**: Agents can now see which entities are teammates and will not attack them
2. **Attack Logic**: `HandleAttack()` now uses `FindNearestEnemy()` instead of `FindNearestAgent()`, ensuring only enemies can be targeted
3. **Pure RL for Resource Collection**: While team identification is provided, resource collection (pickup/deposit) is still learned through win/loss rewards only

## Action Changes

- **Attack**: Now only targets enemies (teammates cannot be attacked at all)
- **Mana Pickup**: No reward - agents learn through win/loss
- **Mana Deposit**: No reward - agents learn through win/loss
- **Mana Shaping**: Removed - agents learn through win/loss

## Required Unity Update

⚠️ **IMPORTANT**: You must update the Vector Observation Space Size in Unity:

1. Select your agent prefab (NoxusAgent or IoniaAgent)
2. In the Inspector, find the **Behavior Parameters** component
3. Under **Vector Observation**, find **Space Size**
4. Change **Vector Observation Space Size** from `43` to `48`
5. Save the prefab

## Verification

After updating, you should see:
- No observation size warnings in the Console
- Agents cannot attack teammates (attack action only targets enemies)
- Agents learn resource collection through win/loss rewards only

