# Debug and Visualization Guide

## Health Visualization

### Viewing Agent Health

**Health text is automatically displayed above each agent** when the scene is running:

- **Format**: `CurrentHealth/MaxHealth` (e.g., "75/100")
- **Position**: 2.5 units above agent's head
- **Color coding**:
  - **Red/Blue** (team color) = Health > 60% (healthy)
  - **Yellow** = Health 30-60% (damaged)
  - **Red** = Health < 30% (critical)
- **Visible in Game view** (not just Scene view)
- **Text faces camera** (billboard effect) so it's always readable

**To toggle health text:**
1. Select an agent prefab or agent in the scene
2. In the Inspector, find the `BaseAgent` component
3. Under "Debug/Visualization", toggle `Show Health Bar`

### Additional Visualizations (Scene View Only)

When viewing in Scene view, you'll also see:
- **Red/Blue wire sphere** = Attack range (team-colored)
- **Green wire sphere** = Interaction range (for picking up mana)
- **Yellow line** (when agent is selected) = Line to nearest mana item

**Note**: Health text is visible in both Game view and Scene view. Range indicators are only visible in Scene view.

## Attack Range Adjustment

**Attack range has been reduced from `3f` to `1.5f`** to better suit a 2x2 arena.

**To adjust attack range:**
1. Select agent prefab
2. In `BaseAgent` component, find `Attack Range`
3. Recommended values for 2x2 arena:
   - `1.0f` = Very close combat
   - `1.5f` = Default (current)
   - `2.0f` = Longer range

## Mana Pickup Smoke Testing

### Debug Logging

**Debug logging is enabled by default** to help you test mana pickup functionality.

**To enable/disable:**
1. Select agent prefab
2. In `BaseAgent` component, under "Debug/Visualization"
3. Toggle `Debug Mana Pickup`

### What You'll See in Console

When `Debug Mana Pickup` is enabled, you'll see detailed logs:

**Successful Pickup:**
```
[MANA PICKUP SUCCESS] Noxus Agent 0 picked up mana! 
Mana carried: 1/3, Reward: +0.1, Distance: 1.23, Position: (1.2, 0.5, 0.8)
```

**Failed Pickup (too far):**
```
[MANA PICKUP FAILED] Noxus Agent 0 too far from mana. 
Distance: 2.45, Interaction Range: 2, Mana Carried: 0/3
```

**Failed Pickup (already picked up):**
```
[MANA PICKUP FAILED] Noxus Agent 0 - Mana already picked up or pickup failed. 
Distance: 1.20, IsPickedUp: True
```

**No Mana Found:**
```
[MANA PICKUP] Noxus Agent 0 - No nearby mana found.
```

**Inventory Full:**
```
[MANA PICKUP] Noxus Agent 0 - Inventory full (3/3)
```

**Successful Deposit:**
```
[MANA DEPOSIT] Noxus Agent 0 deposited 2 mana! 
Reward: +2, Team Mana Banked: 5
```

### Testing Mana Pickup

**To smoke test mana pickup:**

1. **Enable Debug Logging:**
   - Ensure `Debug Mana Pickup` is checked on agent prefabs

2. **Open Console:**
   - Window → General → Console (or Ctrl+Shift+C)

3. **Play the Scene:**
   - Press Play in Unity
   - Watch the Console for pickup/deposit messages

4. **Check for Issues:**
   - If you see "too far from mana" messages, agents might not be getting close enough
   - If you see "No nearby mana found", check that mana items are spawning correctly
   - If you see "Inventory full", agents are successfully picking up but not depositing

5. **Verify Rewards:**
   - Check that rewards are being logged (+0.1 for pickup, +1.0 per mana for deposit)
   - In TensorBoard, you should see reward spikes when pickups/deposits occur

### Common Issues and Solutions

**Agents not picking up mana:**
- Check `Interaction Range` (default: 2f) - agents must be within this distance
- Verify mana items have `ManaItem` component
- Check that mana items are active (not already picked up)
- Look for "too far" messages in console

**Agents picking up but not depositing:**
- Check that heal zones are positioned correctly
- Verify `Interaction Range` is sufficient to reach heal zone
- Check that heal zones have `HealZone` component with correct team type
- Look for deposit messages in console

**No debug messages appearing:**
- Ensure `Debug Mana Pickup` is enabled on agent prefabs
- Check that agents are actually trying to interact (discrete action = 1)
- Verify Console is open and not filtered

## Public Debug Methods

You can access agent state in code:

```csharp
BaseAgent agent = ...;
int health = agent.GetCurrentHealth();
int mana = agent.GetManaCarried();
float healthPercent = agent.GetHealthPercent();
```

## Performance Note

- Health bars and gizmos only render in Scene view (not Game view)
- Debug logging can impact performance if many agents are logging simultaneously
- Consider disabling `Debug Mana Pickup` during long training runs

## Tips

1. **Use Scene View** to see health bars and range indicators
2. **Keep Console open** during testing to monitor pickup/deposit events
3. **Select an agent** in Scene view to see yellow line to nearest mana
4. **Adjust interaction range** if agents struggle to pick up (try 2.5f or 3f)
5. **Check reward values** in TensorBoard to verify rewards are being given

