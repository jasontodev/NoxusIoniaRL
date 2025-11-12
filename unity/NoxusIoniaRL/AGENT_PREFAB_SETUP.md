# Agent Prefab Setup Guide

This guide walks through setting up the Noxus and Ionia agent prefabs for ML-Agents training.

## Overview

You'll need to create two prefabs:
1. **NoxusAgent** - Red team agents
2. **IoniaAgent** - Blue team agents

Both use the same structure but different team assignments.

## Step-by-Step Prefab Creation

### 1. Create Base GameObject

1. In Unity Hierarchy, right-click → **Create Empty**
2. Name it `NoxusAgent` (or `IoniaAgent`)
3. Position at (0, 0, 0) for now

### 2. Add Visual Representation (Optional but Recommended)

1. Right-click the GameObject → **3D Object → Capsule**
2. Name it `Body`
3. Make it a child of the agent GameObject
4. Position at (0, 1, 0) - this centers the capsule on the agent
5. Scale: (1, 1, 1)
6. **Material**: 
   - Create a material (Assets → Create → Material)
   - Name it `NoxusMaterial` (red) or `IoniaMaterial` (blue)
   - Set color: Red for Noxus, Blue for Ionia

### 3. Add Required Components

Select the root agent GameObject and add these components:

#### A. Rigidbody Component
- **Mass**: 1
- **Drag**: 5 (prevents sliding)
- **Angular Drag**: 5
- **Use Gravity**: ✓ (checked)
- **Is Kinematic**: ✗ (unchecked)
- **Constraints**: 
  - Freeze Rotation X: ✓
  - Freeze Rotation Z: ✓
  - Freeze Rotation Y: ✗ (allow Y rotation for turning)
  - Freeze Position Y: ✗ (allow jumping/falling if needed)

#### B. BaseAgent Component (or NoxusAgent/IoniaAgent)
- **Team Type**: Noxus (for NoxusAgent) or Ionia (for IoniaAgent)
- **Agent ID**: 0 (will be set by GameManager)
- **Move Speed**: 5
- **Rotation Speed**: 180
- **Interaction Range**: 2
- **Attack Range**: 3
- **Attack Cooldown**: 1
- **Max Health**: 100
- **Max Mana Carried**: 3
- **K Nearest Entities**: 5
- **Observation Radius**: 20

**Reward Weights** (default values):
- **Reward Deposit**: 1.0
- **Reward Pickup**: 0.1
- **Reward Elimination**: 2.0
- **Reward Death**: -1.0
- **Reward Win**: 10.0
- **Reward Loss**: -5.0
- **Reward Idle**: -0.01
- **Reward Friendly Block**: -0.1
- **Reward Mana Shaping**: 0.05

#### C. Behavior Parameters (ML-Agents)
This is the critical ML-Agents component:

**Behavior Name**: 
- `NoxusAgent` (for Noxus prefab)
- `IoniaAgent` (for Ionia prefab)

**Vector Observation**:
- **Space Size**: 48
  - Self state: 8 features
  - k-NN entities (5 × 4): 20 features
  - Nearest mana (3 × 3): 9 features
  - Nearest obstacles (3 × 2): 6 features
  - Distances to zones: 2 features
  - Global summary: 3 features
  - Total: 8 + 20 + 9 + 6 + 2 + 3 = 48

**Actions**:
- **Space Type**: Hybrid (Continuous + Discrete)
- **Continuous Actions**: 3
  - [0]: Move X (-1 to 1)
  - [1]: Move Z (-1 to 1)
  - [2]: Rotate (-1 to 1)
- **Discrete Actions**: 2 branches
  - Branch 0: 5 actions (0=none, 1=interact, 2=attack, 3=defend, 4=signal)
  - Branch 1: 5 actions (intent codes for signal)

**Model**: Leave empty (will be assigned during training)

**Inference Device**: CPU (or GPU if available)

**Behavior Type**: Default

#### D. Decision Requester (ML-Agents)
- **Decision Period**: 5 (decide every 5 steps)
- **Take Actions Between Decisions**: ✓ (checked)

#### E. Collider Component
Add a **Capsule Collider**:
- **Is Trigger**: ✗ (unchecked)
- **Center**: (0, 1, 0)
- **Radius**: 0.5
- **Height**: 2

This is for:
- Collision detection with obstacles
- Trigger detection with heal zones
- Physics interactions

### 4. Add Tag and Layer (Optional)

1. **Tag**: Create tags `NoxusAgent` and `IoniaAgent` in Project Settings
2. **Layer**: Create layers `Agents` in Project Settings
3. Assign appropriate tag and layer to the prefab

### 5. Create Prefab

1. Drag the GameObject from Hierarchy to `Assets/Prefabs/` folder
2. Delete the instance from the scene (keep the prefab)
3. Repeat for the other team

## Complete Component Checklist

For each agent prefab, ensure you have:

- [x] **Transform** (always present)
- [x] **Rigidbody** (for physics)
- [x] **Capsule Collider** (for collisions)
- [x] **BaseAgent** (or NoxusAgent/IoniaAgent script)
- [x] **Behavior Parameters** (ML-Agents)
- [x] **Decision Requester** (ML-Agents)
- [x] Visual representation (Capsule mesh, optional)

## Behavior Parameters Detailed Settings

### For NoxusAgent Prefab:

```
Behavior Name: NoxusAgent
Vector Observation:
  Space Size: 48
  Stacked Vectors: 1

Actions:
  Space Type: Hybrid
  Continuous Actions: 3
  Discrete Actions:
    Branch 0: 5
    Branch 1: 5

Model: (empty - assigned during training)
Inference Device: CPU
Behavior Type: Default
```

### For IoniaAgent Prefab:

```
Behavior Name: IoniaAgent
Vector Observation:
  Space Size: 48
  Stacked Vectors: 1

Actions:
  Space Type: Hybrid
  Continuous Actions: 3
  Discrete Actions:
    Branch 0: 5
    Branch 1: 5

Model: (empty - assigned during training)
Inference Device: CPU
Behavior Type: Default
```

## Scene Setup Requirements

In your training scene, the GameManager needs:

1. **Agent Prefabs**:
   - Drag `NoxusAgent` prefab to `Agent Prefab Noxus` field
   - Drag `IoniaAgent` prefab to `Agent Prefab Ionia` field

2. **Spawn Points**:
   - Create empty GameObjects named `NoxusSpawnPoint0`, `NoxusSpawnPoint1`, etc.
   - Create empty GameObjects named `IoniaSpawnPoint0`, `IoniaSpawnPoint1`, etc.
   - Position them where you want agents to spawn
   - Assign to GameManager's spawn point arrays

3. **Heal Zones**:
   - Create GameObjects with `HealZone` component
   - Set team type appropriately
   - Add Sphere Collider (Is Trigger = true)
   - Assign to GameManager

## Testing the Prefab

1. Drag prefab into scene
2. Check Console for any errors
3. In Play mode, agent should:
   - Respond to ML-Agents commands
   - Move with physics
   - Detect collisions

## Troubleshooting

**Error: "Behavior name not found"**
- Ensure Behavior Parameters component has correct behavior name
- Match the name in `ppo_config.yaml`

**Agent not moving**
- Check Rigidbody is not kinematic
- Verify Decision Requester is attached
- Check Behavior Parameters has correct action space

**Observations not working**
- Verify Vector Observation Space Size matches BaseAgent output (48)
- Check BaseAgent's CollectObservations method

**Collisions not detected**
- Ensure Collider is not set as Trigger (unless for heal zones)
- Check Rigidbody is present

## Example Hierarchy

```
NoxusAgent (GameObject)
├── Body (Capsule Mesh)
│   └── Mesh Renderer
│   └── Mesh Filter
└── Components:
    ├── Transform
    ├── Rigidbody
    ├── Capsule Collider
    ├── BaseAgent (or NoxusAgent)
    ├── Behavior Parameters
    └── Decision Requester
```

## Next Steps

After creating prefabs:
1. Set up training scene with GameManager
2. Configure ML-Agents config file (`ppo_config.yaml`)
3. Test with random policy
4. Start training with `mlagents-learn`

