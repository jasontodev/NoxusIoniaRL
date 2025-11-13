# Manual Control (Heuristic Mode) Guide

## How to Enable Manual Control

1. **Select your agent** in the Hierarchy or Scene view
2. **Find the Behavior Parameters component** in the Inspector
3. **Change Behavior Type** from "Default" to "Heuristic Only"
4. **Play the scene**
5. The agent will now respond to keyboard input

## Controls

### Movement
- **WASD** or **Arrow Keys**: Move agent
  - W/Up Arrow: Forward
  - S/Down Arrow: Backward
  - A/Left Arrow: Left
  - D/Right Arrow: Right

### Actions
- **E**: Pickup/Deposit Mana
  - If not carrying: Pickup nearest mana (within range)
  - If carrying and at heal zone: Deposit (get reward)
  - If carrying and not at heal zone: Does nothing (use Q to drop)
  
- **Q**: Drop Mana
  - Drops mana at current position (anywhere)
  - No reward for dropping
  - Use this to drop mana outside heal zones
  
- **Space**: Attack
  - Attacks nearest agent (enemy or teammate)
  - Enemies take damage
  - Teammates are protected (no damage, but penalty applied)

## Testing Checklist

### Test Pickup:
1. Move agent near a mana item
2. Press **E** while close to mana
3. Check console for `[MANA PICKUP SUCCESS]` message
4. Verify health text or debug shows mana carried = 1

### Test Deposit:
1. Pick up mana (press E near mana)
2. Move to your team's heal zone
3. Press **E** while in heal zone
4. Check console for `[MANA DEPOSIT]` message
5. Verify reward was given

### Test Drop:
1. Pick up mana (press E near mana)
2. Move away from heal zone
3. Press **Q** to drop mana
4. Check console for `[MANA DROP]` message
5. Verify mana appears on ground

### Test Attack:
1. Move near an enemy agent
2. Press **Space** to attack
3. Check console for attack messages
4. Verify enemy health decreases (if enemy) or no damage (if teammate)

## Important: Update Behavior Parameters

⚠️ **You must update the discrete action space size:**

1. Select agent prefab
2. In **Behavior Parameters** component
3. Find **Discrete Actions** section
4. **Branch 0 Size**: Change from `5` to `6` (to support drop action)
5. Save the prefab

**Current actions:**
- 0: None
- 1: Interact (pickup/deposit)
- 2: Attack
- 3: Defend
- 4: Signal
- 5: Drop (new)

## Troubleshooting

**Agent doesn't respond to keys:**
- Check Behavior Type is set to "Heuristic Only"
- Make sure you're in Play mode
- Check that the agent is selected (if using scene view)

**Drop doesn't work:**
- Verify Branch 0 Size is 6 (not 5)
- Check console for error messages
- Make sure agent is carrying mana (manaCarried > 0)

**Attack doesn't work:**
- Check if there's a nearby agent
- Verify attack range (default 1.5f)
- Check attack cooldown (default 1 second)

