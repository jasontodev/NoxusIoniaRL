# Troubleshooting Guide

## Issue: "Couldn't connect to trainer on port 5004"

This message is **normal** when Unity Editor is not connected to the ML-Agents Python trainer. It means Unity will use inference mode (run a trained model) or random actions if no model exists.

### Solutions:

#### Option 1: Connect to Trainer (For Training)

1. **Start Python Training**:
   ```bash
   cd ../../rl
   mlagents-learn unity/NoxusIoniaRL/config/ppo_config.yaml --run-id=test_run
   ```

2. **Then press Play in Unity** - Unity will automatically connect

3. **Verify Connection**: You should see in Python terminal:
   ```
   Connected to new environment: ...
   ```

#### Option 2: Use Heuristic Mode (For Testing Without Training)

If you want to test agent movement without training:

1. Select an agent in the scene (or the prefab)
2. In Inspector, find **Behavior Parameters** component
3. Change **Behavior Type** from `Default` to `Heuristic Only`
4. Press Play
5. Use **WASD** keys to move the agent manually
6. Use **Space** to interact

**Note**: In Heuristic mode, the agent uses the `Heuristic()` method in BaseAgent.cs which maps keyboard input to actions.

#### Option 3: Load a Trained Model (For Inference)

If you have a trained model:

1. Train a model first (Option 1)
2. After training, find the `.onnx` file in `results/test_run/NoxusAgent.onnx`
3. In Unity, select agent prefab
4. In **Behavior Parameters**, drag the `.onnx` file to **Model** field
5. Set **Behavior Type** to `Inference Only`
6. Press Play - agent will use trained model

## Issue: Agents Not Moving

### Check 1: Behavior Parameters Settings

Verify these settings on your agent prefab:

- **Behavior Name**: Must be `NoxusAgent` or `IoniaAgent` (exact match)
- **Behavior Type**: 
  - `Default` - Connects to trainer (requires training running)
  - `Heuristic Only` - Uses keyboard input (for testing)
  - `Inference Only` - Uses trained model (requires .onnx file)
- **Vector Observation Space Size**: 48
- **Action Space**: Hybrid (3 continuous, 2 discrete branches)

### Check 2: Decision Requester

Ensure **Decision Requester** component is present:

- **Decision Period**: 5 (decide every 5 steps)
- **Take Actions Between Decisions**: ✓ (checked)

### Check 3: Rigidbody Settings

Verify **Rigidbody** component:

- **Is Kinematic**: ✗ (unchecked)
- **Use Gravity**: ✓ (checked)
- **Mass**: 1
- **Constraints**: Freeze Rotation X and Z only

### Check 4: BaseAgent Component

Check **BaseAgent** settings:

- **Move Speed**: 5 (should be > 0)
- **Team Type**: Set correctly
- **Agent ID**: Will be set by GameManager

### Check 5: Console Errors

Look for errors in Console:
- Red errors will prevent agents from working
- Check for missing component references
- Verify all scripts compile without errors

### Check 6: Test with Heuristic Mode

1. Set **Behavior Type** to `Heuristic Only`
2. Press Play
3. Select an agent in Hierarchy (during play mode)
4. Press **WASD** keys
5. Agent should move if everything is configured correctly

If heuristic mode works but training doesn't:
- The issue is with ML-Agents connection
- Make sure training command is running
- Check that behavior names match config file

## Issue: Agents Spawn But Don't Receive Commands

### Check Behavior Name Match

The behavior name in Unity must **exactly match** the config file:

**In Unity (Behavior Parameters)**:
- Behavior Name: `NoxusAgent`

**In config/ppo_config.yaml**:
```yaml
behaviors:
  NoxusAgent:  # Must match exactly
    trainer_type: ppo
    ...
```

### Check Academy Component

ML-Agents requires an **Academy** in the scene:

1. Check if Academy exists (it's usually auto-created)
2. If missing: Hierarchy → Right-click → **ML-Agents → Academy**
3. Academy settings should be default

## Issue: Training Starts But Agents Don't Learn

### Check Observation Space

Verify observation size matches:
- **Expected**: 48 observations
- **In Behavior Parameters**: Set to 48
- **In BaseAgent.cs**: Count the `AddObservation()` calls

### Check Action Space

Verify action space matches:
- **Continuous**: 3 (move X, move Z, rotate)
- **Discrete Branch 0**: 5 (none, interact, attack, defend, signal)
- **Discrete Branch 1**: 5 (intent codes)

### Check Rewards

Agents need rewards to learn:
- Verify `AddReward()` is being called
- Check reward values in BaseAgent component
- Ensure win/loss rewards are given

## Quick Test Checklist

Run through this checklist to verify setup:

- [ ] Agents spawn in scene (visible in Hierarchy during Play)
- [ ] No red errors in Console
- [ ] Behavior Parameters component present
- [ ] Behavior Name matches config file
- [ ] Decision Requester component present
- [ ] Rigidbody component present and not kinematic
- [ ] BaseAgent component present
- [ ] Academy exists in scene
- [ ] GameManager is spawning agents correctly

## Testing Workflow

### Step 1: Test Scene Setup
1. Press Play
2. Check Console for errors
3. Verify agents spawn
4. Verify mana items spawn

### Step 2: Test Agent Movement (Heuristic)
1. Set Behavior Type to `Heuristic Only`
2. Press Play
3. Use WASD to move agent
4. Verify movement works

### Step 3: Test ML-Agents Connection
1. Start training: `mlagents-learn config/ppo_config.yaml`
2. Press Play in Unity
3. Check Python terminal for "Connected" message
4. Verify agents start moving (randomly at first)

### Step 4: Monitor Training
1. Open TensorBoard: `tensorboard --logdir runs`
2. Watch learning curves
3. Verify rewards are increasing over time

## Common Mistakes

1. **Behavior Name Mismatch**: `NoxusAgent` vs `Noxus` - must be exact
2. **Observation Size Wrong**: 47 vs 48 - must match code
3. **Missing Decision Requester**: Agents won't request decisions
4. **Kinematic Rigidbody**: Agents can't move
5. **No Academy**: ML-Agents won't initialize
6. **Training Not Running**: Unity can't connect to trainer

## Getting Help

If issues persist:
1. Check Unity Console for specific errors
2. Check Python terminal for training errors
3. Verify all components are attached
4. Test with Heuristic mode first
5. Check ML-Agents documentation

