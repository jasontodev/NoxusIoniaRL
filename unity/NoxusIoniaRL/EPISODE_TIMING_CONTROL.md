# Episode Timing Control

## Controlling Episode Playback Speed

You can now control how fast episodes play back using the **Time Scale** setting in the GameManager.

## How to Use

### In Unity Editor

1. Select the **GameManager** GameObject in the Hierarchy
2. In the Inspector, find the **"Time Control"** section
3. Adjust the **Time Scale** slider:
   - **1.0** = Normal speed (real-time)
   - **0.5** = Half speed (slow motion)
   - **0.1** = Very slow (for detailed observation)
   - **2.0** = Double speed (faster training)

### During Runtime

You can change the Time Scale in the Inspector while the game is running, and it will update immediately.

## Technical Details

- **Time Scale** affects all time-based operations in Unity:
  - `Time.deltaTime` is multiplied by `Time.timeScale`
  - `Time.fixedDeltaTime` is multiplied by `Time.timeScale`
  - Physics, animations, and timers all run at the scaled speed

- **ML-Agents Training**: 
  - When training with `mlagents-learn`, the time scale still applies
  - For faster training, you can set it to 2.0 or higher
  - For observation/debugging, set it to 0.5 or lower

## Recommended Settings

- **Training**: 1.0 (normal) or 2.0 (faster)
- **Testing/Manual Control**: 0.5 (easier to observe)
- **Debugging**: 0.1-0.3 (very slow, detailed observation)

## Note

The time scale is set in `GameManager.Start()` and updated in `GameManager.Update()` if changed during runtime.

