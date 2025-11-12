# Heuristic Mode Behavior Explained

## Why All Agents Move When You Set Prefab to Heuristic

**This is normal behavior!** Here's why:

### How Prefabs Work

When you change a **prefab's** Behavior Type to `Heuristic Only`:
- All **instances** spawned from that prefab inherit this setting
- If GameManager spawns 2 Noxus agents and 2 Ionia agents
- And you set `IoniaAgent` prefab to Heuristic Only
- **Both Ionia agents** will respond to keyboard input

### The Issue

In Heuristic mode, **all agents of that type** share the same keyboard input:
- Pressing **W** moves ALL agents that are in Heuristic mode
- This is because `Input.GetAxis()` is global, not per-agent

### Solutions

#### Option 1: Test One Agent at a Time

1. Set **only one agent** in the scene (not prefab) to Heuristic
2. Set others to `Default` or `Inference Only`
3. Or temporarily disable other agents

#### Option 2: Use Different Behavior Types

1. Keep one agent as `Heuristic Only` for testing
2. Set others to `Default` (they'll use random actions or trainer)
3. This way only one responds to keyboard

#### Option 3: Test in Scene (Not Prefab)

1. **Don't** change the prefab
2. In Play mode, select a specific agent instance in Hierarchy
3. In Inspector, change **that instance's** Behavior Type to `Heuristic Only`
4. Only that one agent will respond

**Note**: Changes to instances in Play mode are temporary and won't save.

### Best Practice for Testing

1. **For Prefab Testing**: Set prefab to `Heuristic Only`, but expect all instances to respond
2. **For Individual Testing**: Change instance in Play mode (temporary)
3. **For Training**: Set all prefabs back to `Default` before training

## Understanding Behavior Types

- **Default**: Connects to trainer (requires `mlagents-learn` running)
- **Heuristic Only**: Uses keyboard input (all instances of that type respond)
- **Inference Only**: Uses trained model (.onnx file)

## Testing Workflow

1. **Initial Setup**: Test with Heuristic mode to verify movement works
2. **Once Verified**: Set prefabs back to `Default`
3. **Start Training**: Run `mlagents-learn` command
4. **Press Play**: Agents will connect and start learning

