# Training Scene Setup Guide

Complete guide for creating the Noxus vs Ionia training scene with all game elements.

## Overview

The training scene needs:
- **GameManager** - Controls game flow and win conditions
- **Heal Zones** - Team-specific zones for depositing mana
- **Forest Area** - Central area with obstacles and mana items
- **Spawn Points** - Starting positions for agents
- **Obstacles** - Movable boxes, walls, and ramps
- **EventLogger** - Logs game events for analytics

## Step 1: Create New Scene

1. **File → New Scene**
2. Choose **3D (URP)** or **3D Core** template
3. Save as `TrainingScene.unity` in `Assets/Scenes/`

## Step 2: Set Up Ground Plane

1. Right-click Hierarchy → **3D Object → Plane**
2. Name it `Ground`
3. **Transform**:
   - Position: (0, 0, 0)
   - Scale: (10, 1, 10) - Creates a 100x100 unit ground
4. **Material**: 
   - Create material `GroundMaterial`
   - Color: Dark green or brown (forest theme)

## Step 3: Create Heal Zones

### Noxus Heal Zone

1. Right-click Hierarchy → **Create Empty**
2. Name it `NoxusHealZone`
3. **Transform**: Position at (-40, 0, 0) - Left side of map

4. Add **Sphere** child:
   - Right-click `NoxusHealZone` → **3D Object → Sphere**
   - Name it `ZoneVisual`
   - **Transform**:
     - Position: (0, 0, 0) - relative to parent
     - Scale: (10, 0.1, 10) - flat disc shape
   - **Material**: Create `NoxusZoneMaterial` (Red color, semi-transparent)

5. Add **Sphere Collider** to `NoxusHealZone`:
   - **Is Trigger**: ✓ (checked)
   - **Radius**: 5
   - **Center**: (0, 1, 0)

6. Add **HealZone Component**:
   - **Team Type**: Noxus
   - **Heal Rate**: 10
   - **Zone Radius**: 5

### Ionia Heal Zone

1. Repeat steps above, but:
   - Name: `IoniaHealZone`
   - Position: (40, 0, 0) - Right side of map
   - Material: Blue color (Ionia theme)
   - **Team Type**: Ionia

**Final Layout**:
```
NoxusHealZone (-40, 0, 0) ← Left
IoniaHealZone (40, 0, 0)  ← Right
```

## Step 4: Create Forest Area

### What is the Forest Area?

The **Forest Area** is the central combat/collection zone where:
- **Mana items spawn** randomly (handled by GameManager)
- **Obstacles are placed** (boxes, walls, ramps)
- **Agents fight and collect** resources
- **Strategic gameplay** happens (cover, paths, chokepoints)

It's the "neutral ground" between the two team bases (heal zones).

### Forest Setup Options

#### Option A: Visual Forest (Recommended for Development)

Create a visible forest area to help with placement:

1. Right-click Hierarchy → **Create Empty**
2. Name it `ForestArea`
3. **Transform**: Position (0, 0, 0) - Center of map

4. **Add Visual Boundary** (helps see the area):
   - Right-click `ForestArea` → **3D Object → Plane**
   - Name it `ForestBoundary`
   - **Transform**:
     - Position: (0, 0.01, 0) - Slightly above ground
     - Scale: (4, 1, 4) - Creates 40×40 unit area
   - **Material**: Create `ForestBoundaryMaterial`
     - Color: Dark green with low opacity (0.3 alpha)
     - Or use a subtle texture
   - This is just a visual guide - you can disable it later

5. **Add Trees/Props** (optional, for atmosphere):
   - Add **Tree** objects or simple **Cylinder** objects as trees
   - Scatter a few for visual reference
   - These are decorative only - obstacles provide gameplay cover

#### Option B: Invisible Forest (Minimal)

If you don't need visual markers:

1. Right-click Hierarchy → **Create Empty**
2. Name it `ForestArea`
3. **Transform**: Position (0, 0, 0)
4. Leave it empty - it's just an organizational container

### Forest Bounds and GameManager

**Important**: The forest bounds are **hardcoded in GameManager.cs**:

```csharp
Bounds forestBounds = new Bounds(Vector3.zero, new Vector3(40f, 0f, 40f));
```

This means:
- **Center**: (0, 0, 0) - Origin
- **Size**: 40 units on X, 40 units on Z
- **Bounds**: X from -20 to +20, Z from -20 to +20

**Mana items will spawn randomly within these bounds** when GameManager calls `SpawnManaItems()`.

### Forest Layout

```
Top View (X-Z plane):

        Noxus Base          Forest Area          Ionia Base
    ┌─────────────┐      ┌─────────────┐    ┌─────────────┐
    │             │      │             │    │             │
    │   Heal      │      │  -20 to +20 │    │   Heal      │
    │   Zone      │      │  on X & Z   │    │   Zone      │
    │             │      │             │    │             │
    │  X = -40    │      │  X = 0      │    │  X = +40    │
    │             │      │             │    │             │
    │             │      │ Obstacles   │    │             │
    │             │      │ & Mana      │    │             │
    │             │      │ Items       │    │             │
    └─────────────┘      └─────────────┘    └─────────────┘
```

### Placing Obstacles in Forest

When placing obstacles:
- **X coordinate**: Between -20 and +20 (within forest bounds)
- **Z coordinate**: Between -20 and +20
- **Y coordinate**: 0.5 (half the obstacle height, so bottom sits on ground)

**Example Positions**:
- Box at (5, 0.5, -3)
- Wall at (-10, 1.5, 8) - Y is higher because walls are tall
- Ramp at (0, 0.25, 0) - Y is lower because ramps are flat

### Forest Organization in Hierarchy

```
ForestArea (Empty GameObject, organizational)
├── ForestBoundary (Plane, visual guide - optional)
├── BoxObstacle_1
├── BoxObstacle_2
├── WallObstacle_1
├── RampObstacle_1
└── ... (more obstacles)
```

**Note**: The `ForestArea` GameObject is mainly for organization. The actual forest functionality comes from:
1. The hardcoded bounds in GameManager
2. The obstacles you place within those bounds
3. The mana items that spawn there

## Step 5: Create Obstacles

**Two Approaches - Choose One:**

### Approach A: Create Prefab First (Recommended)

This is cleaner and ensures all obstacles have identical settings:

1. **Create the Prefab**:
   - Right-click Hierarchy → **3D Object → Cube**
   - Name it `BoxObstacle`
   - Configure all components (see below)
   - **Drag to `Assets/Prefabs/Obstacles/`** to create prefab
   - **Delete the instance from scene** (prefab is saved, instance not needed)

2. **Place Instances in Scene**:
   - Drag `BoxObstacle.prefab` from Project window into Hierarchy
   - Position it in forest area (e.g., (5, 0.5, -3))
   - Repeat 5-10 times with different positions
   - Each instance shares the prefab settings but can have unique positions

### Approach B: Create Directly in Scene

If you prefer to place obstacles manually:

1. Create each obstacle directly in the scene
2. Configure components individually
3. Optionally create prefab later for consistency

**I recommend Approach A** because:
- Ensures all obstacles have identical physics settings
- Easy to add/remove obstacles
- Can update all obstacles by editing the prefab
- Cleaner scene organization

### Box Obstacle Configuration

For each box obstacle (whether prefab or instance):

1. **Transform**:
   - Position: Random in forest area (e.g., (5, 0.5, -3))
   - Scale: (2, 2, 2) - 2x2x2 unit box
   - Rotation: Vary for visual interest (e.g., (0, 15, 0))

2. **Material**: 
   - Create `BoxMaterial` (Brown or gray)
   - Assign to obstacle

3. **Rigidbody**:
   - Mass: 10
   - Drag: 1
   - Use Gravity: ✓
   - Constraints: None (boxes can move freely)

4. **Box Collider** (already present):
   - Verify it's enabled
   - Size should match scale

5. **Obstacle Component**:
   - **Obstacle Type**: Box
   - **Push Force**: 5
   - **Max Push Distance**: 10

**Placement**: Scatter 5-10 boxes randomly in the forest area (roughly -20 to 20 on X and Z axes)

### Wall Obstacles

1. Right-click Hierarchy → **3D Object → Cube**
2. Name it `WallObstacle`
3. **Transform**:
   - Position: In forest area
   - Scale: (1, 3, 5) - Tall, thin wall
   - Rotation: (0, 45, 0) - Angled for variety
4. **Material**: `WallMaterial` (Gray or stone texture)
5. Add **Rigidbody**:
   - Mass: 50
   - Drag: 2
   - Use Gravity: ✓
   - **Constraints**: 
     - Freeze Position Y: ✓ (walls don't fall)
     - Freeze Rotation: ✓ (walls don't rotate)
6. Add **Box Collider**
7. Add **Obstacle Component**:
   - **Obstacle Type**: Wall
   - **Push Force**: 5
   - **Max Push Distance**: 10

**Placement**: Create prefab, then place 3-5 wall instances in forest

### Ramp Obstacles

1. **Create Prefab** (same approach as boxes):
   - Right-click Hierarchy → **3D Object → Cube**
   - Name it `RampObstacle`
   - Configure components (see below)
   - Drag to `Assets/Prefabs/Obstacles/RampObstacle.prefab`
   - Delete instance

2. **Ramp Configuration**:
   - **Transform**:
     - Scale: (3, 0.5, 5) - Wide and flat
     - Rotation: (0, 0, 15) - Tilted to create ramp effect
   - **Material**: `RampMaterial`
   - **Rigidbody**:
     - Mass: 20
     - Drag: 1.5
     - Use Gravity: ✓
   - **Box Collider**
   - **Obstacle Component**:
     - **Obstacle Type**: Ramp
     - **Push Force**: 5
     - **Max Push Distance**: 10

3. **Place Instances**: Drag prefab into scene 2-3 times with different positions and rotations

**Obstacle Placement Tips**:
- Scatter obstacles randomly in forest area (roughly -20 to 20 on X and Z)
- Avoid blocking spawn points (keep spawn areas clear)
- Create interesting paths and cover for strategic gameplay
- Use different rotations for visual variety
- Mix box, wall, and ramp types for diverse terrain

## Step 6: Create Spawn Points

### Noxus Spawn Points

1. Right-click Hierarchy → **Create Empty**
2. Name it `NoxusSpawnPoints` (parent container)
3. Create children:
   - Right-click `NoxusSpawnPoints` → **Create Empty**
   - Name: `NoxusSpawnPoint0`
   - **Transform**: Position (-35, 0.5, -5)
   - Repeat for `NoxusSpawnPoint1` at (-35, 0.5, 5)

**Visual Helper** (optional):
- Add **Capsule** child to each spawn point
- Scale: (0.5, 0.1, 0.5)
- Material: Red (Noxus color)
- This helps visualize spawn locations in editor

### Ionia Spawn Points

1. Right-click Hierarchy → **Create Empty**
2. Name it `IoniaSpawnPoints` (parent container)
3. Create children:
   - `IoniaSpawnPoint0` at (35, 0.5, -5)
   - `IoniaSpawnPoint1` at (35, 0.5, 5)

**Visual Helper**: Blue capsules for Ionia spawns

**Spawn Point Layout**:
```
Noxus Spawns (Left side):
  Spawn0: (-35, 0.5, -5)
  Spawn1: (-35, 0.5, 5)

Ionia Spawns (Right side):
  Spawn0: (35, 0.5, -5)
  Spawn1: (35, 0.5, 5)
```

## Step 7: Create Mana Item Prefab

1. Right-click Hierarchy → **3D Object → Sphere**
2. Name it `ManaItem`
3. **Transform**:
   - Scale: (0.5, 0.5, 0.5) - Small collectible
4. **Material**: Create `ManaMaterial`
   - Color: Bright purple or blue
   - Add glow effect (emission)
5. Add **Sphere Collider**:
   - **Is Trigger**: ✗ (unchecked - for pickup detection)
   - **Radius**: 0.5
6. Add **ManaItem Component**
7. **Create Prefab**: `Assets/Prefabs/ManaItem.prefab`
8. Delete from scene (GameManager will spawn them)

## Step 8: Set Up GameManager

1. Right-click Hierarchy → **Create Empty**
2. Name it `GameManager`
3. **Transform**: Position (0, 0, 0)

4. Add **GameManager Component**:
   - **Agents Per Team**: 2
   - **Episode Max Time**: 300 (5 minutes)
   - **Win Condition Mana**: 20

5. **Assign References**:
   - **Noxus Spawn Points**: Drag `NoxusSpawnPoint0` and `NoxusSpawnPoint1` to array
   - **Ionia Spawn Points**: Drag `IoniaSpawnPoint0` and `IoniaSpawnPoint1` to array
   - **Noxus Heal Zone**: Drag `NoxusHealZone` GameObject
   - **Ionia Heal Zone**: Drag `IoniaHealZone` GameObject
   - **Agent Prefab Noxus**: Drag `NoxusAgent` prefab
   - **Agent Prefab Ionia**: Drag `IoniaAgent` prefab
   - **Mana Item Prefab**: Drag `ManaItem` prefab
   - **Mana Items Count**: 15

## Step 9: Set Up EventLogger

1. Right-click Hierarchy → **Create Empty**
2. Name it `EventLogger`
3. Add **EventLogger Component**:
   - **Log Directory**: `data/logs`
   - **Log File Name**: `events`
   - **Log To File**: ✓
   - **Log To Console**: ✗ (unchecked, unless debugging)
   - **Flush Interval**: 5

## Step 10: Lighting and Camera Setup

### Lighting

1. **Directional Light** (already in scene):
   - Rotation: (50, -30, 0) - Angled sunlight
   - Intensity: 1
   - Color: Slight warm tint

2. **Ambient Light** (Window → Rendering → Lighting):
   - Ambient Intensity: 0.5
   - Sky Color: Light blue

### Camera (Optional - for observation)

1. Select **Main Camera**
2. **Transform**:
   - Position: (0, 30, -20)
   - Rotation: (45, 0, 0) - Top-down view
3. **Camera Settings**:
   - Field of View: 60
   - Clear Flags: Skybox

## Step 11: Final Scene Organization

Organize Hierarchy for clarity:

```
TrainingScene
├── GameManager
├── EventLogger
├── Ground
├── NoxusHealZone
│   └── ZoneVisual
├── IoniaHealZone
│   └── ZoneVisual
├── ForestArea
│   ├── BoxObstacle_1
│   ├── BoxObstacle_2
│   ├── WallObstacle_1
│   ├── RampObstacle_1
│   └── ... (more obstacles)
├── NoxusSpawnPoints
│   ├── NoxusSpawnPoint0
│   └── NoxusSpawnPoint1
├── IoniaSpawnPoints
│   ├── IoniaSpawnPoint0
│   └── IoniaSpawnPoint1
├── Main Camera
└── Directional Light
```

## Step 12: Component Checklist

Verify all components are properly configured:

### GameManager
- [x] Agents Per Team: 2
- [x] Episode Max Time: 300
- [x] Win Condition Mana: 20
- [x] Noxus Spawn Points assigned (2)
- [x] Ionia Spawn Points assigned (2)
- [x] Heal Zones assigned
- [x] Agent Prefabs assigned
- [x] Mana Item Prefab assigned

### Heal Zones
- [x] Sphere Collider (Is Trigger = true)
- [x] HealZone component
- [x] Team Type set correctly
- [x] Zone Radius: 5

### Obstacles
- [x] Rigidbody component
- [x] Collider component
- [x] Obstacle component
- [x] Obstacle Type set correctly

### Spawn Points
- [x] Transform positions set
- [x] Assigned to GameManager arrays

### EventLogger
- [x] EventLogger component
- [x] Log directory configured

## Step 13: Testing the Scene

### Before Training

1. **Play Mode Test**:
   - Enter Play Mode
   - Check Console for errors
   - Verify agents spawn
   - Verify mana items spawn
   - Check GameManager initializes

2. **Manual Agent Test**:
   - Select an agent in scene
   - In Inspector, find **Behavior Parameters**
   - Set **Behavior Type** to **Heuristic Only**
   - Use WASD to move, Space to interact
   - Verify movement and interactions work

3. **ML-Agents Connection Test**:
   - Set **Behavior Type** back to **Default**
   - Start ML-Agents training: `mlagents-learn config/ppo_config.yaml`
   - Verify Unity connects and agents receive commands

## Step 14: Scene Layout Reference

```
Top View (X-Z plane):

        Noxus Side          Forest Area          Ionia Side
    ┌─────────────┐      ┌─────────────┐    ┌─────────────┐
    │             │      │             │    │             │
    │   Heal      │      │  Obstacles  │    │   Heal      │
    │   Zone      │      │   & Mana    │    │   Zone      │
    │             │      │   Items     │    │             │
    │  Spawn      │      │             │    │  Spawn      │
    │  Points     │      │             │    │  Points     │
    └─────────────┘      └─────────────┘    └─────────────┘
    X = -40               X = 0              X = +40
```

**Coordinate System**:
- **X-axis**: Left (-) to Right (+)
- **Z-axis**: Back (-) to Front (+)
- **Y-axis**: Down (-) to Up (+)

## Step 15: Optimization Tips

1. **Obstacle Pooling**: For many obstacles, consider object pooling
2. **LOD Groups**: Add LOD for distant obstacles
3. **Occlusion Culling**: Enable for better performance
4. **Physics Layers**: Use layers to optimize collision detection
5. **Batching**: Combine static obstacles for better rendering

## Troubleshooting

**Agents don't spawn**:
- Check spawn points are assigned in GameManager
- Verify prefabs are assigned
- Check spawn point positions are valid

**Mana items don't spawn**:
- Verify Mana Item Prefab is assigned
- Check Mana Items Count > 0
- Ensure forest bounds are set correctly in GameManager

**Heal zones don't work**:
- Verify Sphere Collider is set as Trigger
- Check HealZone component team type matches
- Ensure agents have colliders

**Obstacles don't move**:
- Check Rigidbody is not kinematic
- Verify Obstacle component is attached
- Check push force > 0

**GameManager errors**:
- Verify all references are assigned
- Check spawn point arrays match agents per team
- Ensure heal zones are assigned

## Next Steps

After scene setup:
1. Test with random policy
2. Configure ML-Agents behavior names
3. Start training with `mlagents-learn`
4. Monitor training in TensorBoard

## Scene Dimensions Reference

- **Ground**: 100 × 100 units (Scale 10, 1, 10)
- **Heal Zones**: 10 unit radius (5 unit trigger)
- **Forest Area**: ~40 × 40 units centered
- **Spawn Distance**: ~70 units apart (Noxus -40, Ionia +40)
- **Agent Size**: ~1 unit (Capsule radius 0.5, height 2)

