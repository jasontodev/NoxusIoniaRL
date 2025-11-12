# Quick Fix: Agents Not Moving

## Immediate Solution

The "Couldn't connect to trainer" message is normal. To get agents moving right now:

### Option 1: Use Heuristic Mode (Fastest Test)

1. **Select an agent** in the scene (or select the prefab)
2. In Inspector, find **Behavior Parameters** component
3. Change **Behavior Type** from `Default` to `Heuristic Only`
4. **Press Play**
5. **Select an agent** in Hierarchy (during Play mode)
6. **Press WASD** keys - agent should move!

If this works, your agent setup is correct. The issue is just that no trainer is connected.

### Option 2: Connect to Trainer

1. **Open Terminal/PowerShell** in your project root
2. **Navigate to RL folder**:
   ```bash
   cd rl
   ```
3. **Install ML-Agents** (if not already):
   ```bash
   pip install mlagents
   ```
4. **Start Training**:
   ```bash
   mlagents-learn ../unity/NoxusIoniaRL/config/ppo_config.yaml --run-id=test_run
   ```
5. **Wait for** "Start training by pressing the Play button in the Unity Editor"
6. **Press Play in Unity**
7. Agents should start moving (randomly at first, then learning)

## Why Agents Aren't Moving

When Unity says "Will perform inference instead", it means:
- If you have a trained model (.onnx file) → uses that model
- If you DON'T have a model → uses **random actions**

Random actions might not be strong enough to move the agent, or there might be an issue with action application.

## Verify Agent Setup

Quick checklist - verify these on your agent prefab:

1. **Behavior Parameters**:
   - Behavior Name: `NoxusAgent` or `IoniaAgent`
   - Behavior Type: `Default` (for training) or `Heuristic Only` (for testing)
   - Vector Observation: 48
   - Actions: Hybrid (3 continuous, 2 discrete branches)

2. **Decision Requester**:
   - Decision Period: 5
   - Take Actions Between Decisions: ✓

3. **Rigidbody**:
   - Is Kinematic: ✗ (unchecked)
   - Use Gravity: ✓
   - Mass: 1

4. **BaseAgent**:
   - Move Speed: 5
   - Team Type: Set correctly

## Test Heuristic Mode Step-by-Step

1. Open your training scene
2. Select `NoxusAgent` prefab (or instance in scene)
3. In Inspector → **Behavior Parameters** → **Behavior Type** → Select `Heuristic Only`
4. Press Play
5. In Hierarchy, find a spawned agent (e.g., `NoxusAgent(Clone)`)
6. Select it
7. Press **W** (forward), **S** (backward), **A** (left), **D** (right)
8. Agent should move!

If heuristic mode works → Your setup is correct, just need to connect trainer.
If heuristic mode doesn't work → There's a configuration issue (see troubleshooting guide).

## Common Issues

**"Agents spawn but don't move in Heuristic mode"**:
- Check that you're selecting the agent DURING Play mode
- Verify Input Manager has Horizontal/Vertical axes (Edit → Project Settings → Input Manager)
- Check Console for errors

**"Training connects but agents don't move"**:
- Check behavior name matches config file exactly
- Verify Decision Requester is present
- Check that actions are being received (add Debug.Log in OnActionReceived)

**"Random actions but no movement"**:
- Random actions might be too small
- Check Move Speed > 0
- Verify Rigidbody is not kinematic
- Check for physics issues (agents stuck, etc.)

## Debug: Add Logging

To see if actions are being received, temporarily add to BaseAgent.cs:

```csharp
public override void OnActionReceived(ActionBuffers actions)
{
    // Add this at the start
    Debug.Log($"Actions received: X={actions.ContinuousActions[0]}, Z={actions.ContinuousActions[1]}");
    
    // ... rest of method
}
```

This will show in Console if actions are being received.

